using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;
using VerdeNFC.Services;
using VerdeNFC.Util;
using Plugin.Toast;
using VerdeNFC.Models;
using System.Collections.Generic;

namespace VerdeNFC.ViewModels
{
    public class MainTabViewModel : BaseViewModel
    {
        public static MainTabViewModel Current;

        public IDataBag DataBag => DependencyService.Get<IDataBag>();
        
        public delegate void NFCControlListening(bool Write);
        public event NFCControlListening NFCStartListening;
        public event NFCControlListening NFCStopListening;

        private List<RoastProfile> _roastProfiles = new List<RoastProfile>();
        public List<RoastProfile> RoastProfiles
        {
            get { return _roastProfiles; }
            set
            {
                _roastProfiles = value;
                OnPropertyChanged(nameof(RoastProfiles));
            }
        }

        private RoastProfile lastSelectedRoastProfile;
        private RoastProfile _roastProfileSel = new RoastProfile();
        public RoastProfile RoastProfileSel
        {
            get { return _roastProfileSel; }
            set
            {
                _roastProfileSel = value;
                OnPropertyChanged(nameof(RoastProfileSel));
            }
        }

        public void RoastProfileSelChanged()
        {
            if ((RoastProfileSel.Id < 100) || (RoastProfileSel.Id != 0))
            {
                if ((lastSelectedRoastProfile?.Id > 100) || (lastSelectedRoastProfile?.Id == 0))
                    RoastProfiles.Remove(lastSelectedRoastProfile);
                lastSelectedRoastProfile = RoastProfileSel;
            }
            else
                RoastProfileSel = lastSelectedRoastProfile;
        }

        bool _cbNFCRead;
        public bool cbNFCRead
        {
            get
            {
                return _cbNFCRead;
            }
            set
            {
                try
                {
                    if (value)
                        NFCStartListening?.Invoke(false);
                    else
                    {
                        NFCStopListening?.Invoke(false);
                        MessagingCenter.Send(this, "DataChanged", DataBag.GetData());
                    }

                    _cbNFCRead = value;
                }
                catch (Exception e)
                {
                    CrossToastPopUp.Current.ShowToastMessage("NFC not enabled/error");
                    _cbNFCRead = false;
                }
                OnPropertyChanged("cbNFCRead");
                OnPropertyChanged("cbNFCWriteEnabled");
            }
        }
        public bool cbNFCReadEnabled
        {
            get
            {
                return !_cbNFCWrite;
            }
        }

        bool _cbMultiUse;
        public bool cbMultiUse
        {
            get
            {
                return _cbMultiUse;
            }
            set
            {
                _cbMultiUse = value;
                OnPropertyChanged("cbMultiUse");
            }
        }

        bool _cbNFCWrite;
        public bool cbNFCWrite
        {
            get
            {
                return _cbNFCWrite;
            }
            set
            {
                try
                {
                    if (value)
                        NFCStartListening?.Invoke(true);
                    else
                    { 
                        NFCStopListening?.Invoke(true);
                        MessagingCenter.Send(this, "DataChanged", DataBag.GetData());
                    }

                    _cbNFCWrite = value;
                }
                catch (Exception e)
                {
                    CrossToastPopUp.Current.ShowToastMessage("NFC not enabled/error");
                    _cbNFCWrite = false;
                }
                OnPropertyChanged("cbNFCWrite");
                OnPropertyChanged("cbNFCReadEnabled");
            }
        }

        public bool cbNFCWriteEnabled
        {
            get
            {
                return !_cbNFCRead;
            }
        }

        int _nPause;
      
        public int nPause
        { 
            get
            {
                return _nPause;
            }
            set
            {
                _nPause = value;
                byte[] mem = DataBag.GetData();
                mem[48] = Convert.ToByte(_nPause / 256);
                mem[49] = Convert.ToByte(_nPause % 256);
                DataBag.SetData(mem);
                OnPropertyChanged("nPause");
                MessagingCenter.Send(this, "DataChanged", DataBag.GetData());
            }
        }
        readonly string _downloadFolder;

        public MainTabViewModel()
        {
            Title = "VerdeNFC 1.0";
//            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://xamarin.com"));
            OpenFilePickerSrc = new Command(async () => await OpenFilePickerSrcAsync());
            OpenFilePickerDest = new Command(async () => await OpenFilePickerDestAsync());
            cbMultiUse = true;
            Current = this;
            _downloadFolder = (string) Application.Current.Properties["FileSaveFolder"];
            _nPause = 0;
            cbMultiUse = true;

            // 0      - nothing choosen
            // 1...99 - data source is Data member
            // 100..  - external data source (file or scanned NFC tag)

            RoastProfiles.Add(new RoastProfile() { Id = 0, Name = "(choose one)", Data = "", isRoastProfile = false, isGrindProfile = false, isBrewProfile = false });
            // RoastProfiles.Add(new RoastProfile() { Id = 101, Name = "(RGB from NFC or File)", Data = "", isRoastProfile = true, isGrindProfile = true, isBrewProfile = true });
            // RoastProfiles.Add(new RoastProfile() { Id = 102, Name = "(Roast from NFC or File)", Data = "", isRoastProfile = true, isGrindProfile = false, isBrewProfile = false });
            // RoastProfiles.Add(new RoastProfile() { Id = 103, Name = "(Grind/Brew from NFC or File)", Data = "", isRoastProfile = false, isGrindProfile = true, isBrewProfile = true });
            // RoastProfiles.Add(new RoastProfile() { Id = 104, Name = "(Grind from NFC or File)", Data = "", isRoastProfile = false, isGrindProfile = true, isBrewProfile = false });
            // RoastProfiles.Add(new RoastProfile() { Id = 105, Name = "(Brew from NFC or File)", Data = "", isRoastProfile = false, isGrindProfile = false, isBrewProfile = true });

            RoastProfiles.Add(new RoastProfile() { Id = 1, Name = "Brazil",                  Data = "AAB84B4B00 AAB8324B5A  AAB64B4B00 AAB6324B64 3C3C462D 32 02  1E  000501 D810 0005", isRoastProfile = true, isGrindProfile = false, isBrewProfile = false });
            RoastProfiles.Add(new RoastProfile() { Id = 2, Name = "80g Roast only",          Data = "AAAE4B5A05 AAAE415078  AAB4415A05 AAB4415050 37465A50 23 02  0F  000601 6113 0000", isRoastProfile = true, isGrindProfile = false, isBrewProfile = false });
            RoastProfiles.Add(new RoastProfile() { Id = 3, Name = "universal light roast",   Data = "AAB94B3205 AAB94B325A  AAB94B3205 AAB94B3232 3C463250 32 02  1E  000601 5D03 000A", isRoastProfile = true, isGrindProfile = false, isBrewProfile = false });
            RoastProfiles.Add(new RoastProfile() { Id = 4, Name = "universal medium roast",  Data = "AA645A3205 AA9632327D  AAB4463C05 AAC0323C78 376E3C2D 23 02  0F  000601 F501 000A", isRoastProfile = true, isGrindProfile = false, isBrewProfile = false });
            RoastProfiles.Add(new RoastProfile() { Id = 5, Name = "80g Slow Roast RGB (10h)",Data = "AAB84B4B00 AAB832465A  AAB64B5A00 AAB6324664 3C465A2D 32 01  2D  000501 CCFE 0258", isRoastProfile = true, isGrindProfile = true, isBrewProfile = true });

            RoastProfiles.Add(new RoastProfile() { Id = 6, Name = "universal dark roast ",   Data = "AA645A3205 AA9632327D  AAB4463C05 AAC0323C96 376E3C2D 23 02  0F  000601 8312 000A", isRoastProfile = true, isGrindProfile = false, isBrewProfile = false });
            RoastProfiles.Add(new RoastProfile() { Id = 7, Name = "80g Roasters Grind/Brew", Data = "AAB0644B05 AAB0414678  AABA5F5A05 AABA41463C 376E5A2D 23 05  1E  000601 AB08 0000", isRoastProfile = false, isGrindProfile = true, isBrewProfile = true });
            RoastProfiles.Add(new RoastProfile() { Id = 8, Name = "20g Roasters Grind/Brew", Data = "AAB44B4B00 AAB432464B  AAAE4B5A00 AAAE32465A 3C6E5A14 14 05  2D  000501 4A01 0000", isRoastProfile = false, isGrindProfile = true, isBrewProfile = true });

            RoastProfiles.Add(new RoastProfile() { Id = 90, Name = "(Brew only 80g)",        Data = "AA41644B05 AA4B413278  AA565F5005 AA5D414696 37375A2D 23 06  0F  000601 2BF2 0000", isRoastProfile = false, isGrindProfile = false, isBrewProfile = true });
            RoastProfiles.Add(new RoastProfile() { Id = 91, Name = "(Air Filter Reset)",     Data = "AA96644B05 AA96413278  AAAC5F5005 AAB6414696 376E5A2D 23 0F  0F  000601 8D7C 0000", isRoastProfile = false, isGrindProfile = false, isBrewProfile = false });
            RoastProfiles.Add(new RoastProfile() { Id = 92, Name = "(Maintenance: Descale)", Data = "AAB44B4B05 AAB44B465F  AAB24B5005 AAB24B4687 37465A2D 23 13  2D  000601 7F56 0007", isRoastProfile = false, isGrindProfile = false, isBrewProfile = true });
            RoastProfiles.Add(new RoastProfile() { Id = 93, Name = "(Maintenance: Grinder clean)", Data = "AAB44B4B00 AAB432464E  AAB44B5A99 AAB4324666 3C465A2D 32 12  1E  000501 8D5E 0005", isRoastProfile = false, isGrindProfile = true, isBrewProfile = false });
            RoastProfileSel = RoastProfiles[0];
            lastSelectedRoastProfile = RoastProfiles[0];

        }

        public async Task OpenFilePickerSrcAsync()
        {
            try
            {
                FileData fileData = await CrossFilePicker.Current.PickFile();
                if (fileData == null)
                    return; // user canceled file picking

                MemoryStream ms = new MemoryStream(fileData.DataArray);
                StreamReader reader = new StreamReader(ms, System.Text.Encoding.ASCII);

                string line;
                byte[] mem = new byte[80];
                int index = 0;

                while (true)
                {
                    line = reader.ReadLine();
                    if (line == null)
                        break;

                    try
                    {
                        int j = 0;
                        while ((2 * j < line.Length) &&
                               (line[2 * j] != ' ') &&
                               (line[2 * j] != '\t') &&
                               (index < 80))
                            mem[index++] = byte.Parse(line.Substring(2 * j++, 2), NumberStyles.HexNumber);
                    }
                    catch
                    {
                        Console.WriteLine("Error: unknown line format: {0}", line);
                    }
                }

                nPause = 256 * mem[48] + mem[49];

                DataBag.SetData(mem);
                MessagingCenter.Send(this, "DataChanged", DataBag.GetData());

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception choosing file: " + ex.ToString());
            };

            return;
        }
        public async Task OpenFilePickerDestAsync()
        {
            try
            {
                FileData fileData = await CrossFilePicker.Current.PickFile();
                if (fileData == null)
                    return; // user canceled file picking

                MemoryStream ms = new MemoryStream(fileData.DataArray);
                StreamReader reader = new StreamReader(ms, System.Text.Encoding.ASCII);

                string line;
                byte[] mem = new byte[80];
                int index = 0;

                int i = 0;
                while (true)
                {
                    line = reader.ReadLine();
                    if (line == null)
                        break;

                    try
                    {
                        int j = 0;
                        while ((2 * j < line.Length) &&
                               (line[2 * j] != ' ') &&
                               (line[2 * j] != '\t') &&
                               (index < 80))
                            mem[index++] = byte.Parse(line.Substring(2 * j++, 2), NumberStyles.HexNumber);
                    }
                    catch
                    {
                        Console.WriteLine("Error: unknown line format: {0}", line);
                    }
                }

                byte[] mem1stCard = MergeTagData(DataBag.GetData(), mem);


                StreamWriter ofile = new StreamWriter(Path.Combine(_downloadFolder, fileData.FileName + "-new.txt"))
                {
                    NewLine = "\n"
                };

                index = 0;
                for (i = 0; i < 20; i++)
                {
                    line = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", mem1stCard[index], mem1stCard[index + 1], mem1stCard[index + 2], mem1stCard[index + 3]);
                    index += 4;
                    ofile.WriteLine(line);
                }

                ofile.Close();

            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception choosing file: " + ex.ToString());
            };

            return;
        }

        public static byte [] MergeTagData(byte [] mem1stCard, byte [] mem)
        {
            Buffer.BlockCopy(mem, 0, mem1stCard, 0, 16);

            if ((Current != null) && (Current.cbMultiUse))
            {
                mem1stCard[46] = Convert.ToByte(VerdeChecksums.crc16_multiuse(mem1stCard) & 0xff);
                mem1stCard[47] = Convert.ToByte((VerdeChecksums.crc16_multiuse(mem1stCard) >> 8) & 0xff);
            }
            else
            {
                mem1stCard[46] = Convert.ToByte(VerdeChecksums.crc16_singleuse(mem1stCard) & 0xff);
                mem1stCard[47] = Convert.ToByte((VerdeChecksums.crc16_singleuse(mem1stCard) >> 8) & 0xff);
            }

            mem1stCard[60] = Convert.ToByte(Crc8.ComputeChecksum(mem1stCard.Skip(0).Take(12).Concat(mem1stCard.Skip(16).Take(44)).ToArray()) & 0xff);

            Current.DataBag.SetData(mem1stCard);
            MessagingCenter.Send(Current, "DataChanged", Current.DataBag.GetData());

            return mem1stCard;
        }

        public ICommand OpenWebCommand { get; }
        public ICommand OpenFilePickerSrc { get; }
        public ICommand OpenFilePickerDest { get; }
    }
}