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
            RoastProfiles.Add(new RoastProfile() { Id = 101, Name = "(RGB from NFC or File)", Data = "", isRoastProfile = true, isGrindProfile = true, isBrewProfile = true });
            RoastProfiles.Add(new RoastProfile() { Id = 102, Name = "(Roast from NFC or File)", Data = "", isRoastProfile = true, isGrindProfile = false, isBrewProfile = false });
            RoastProfiles.Add(new RoastProfile() { Id = 103, Name = "(Grind/Brew from NFC or File)", Data = "", isRoastProfile = false, isGrindProfile = true, isBrewProfile = true });
            RoastProfiles.Add(new RoastProfile() { Id = 104, Name = "(Grind from NFC or File)", Data = "", isRoastProfile = false, isGrindProfile = true, isBrewProfile = false });
            RoastProfiles.Add(new RoastProfile() { Id = 105, Name = "(Brew from NFC or File)", Data = "", isRoastProfile = false, isGrindProfile = false, isBrewProfile = true });

            RoastProfiles.Add(new RoastProfile() { Id = 1, Name = "Brazil", Data = "2323", isRoastProfile = true, isGrindProfile = false, isBrewProfile = false });
            RoastProfiles.Add(new RoastProfile() { Id = 2, Name = "Brazil 2", Data = "2323", isRoastProfile = true, isGrindProfile = false, isBrewProfile = false });

            RoastProfiles.Add(new RoastProfile() { Id = 90, Name = "(Air Filter Reset)", Data = "", isRoastProfile = false, isGrindProfile = false, isBrewProfile = false });
            RoastProfiles.Add(new RoastProfile() { Id = 91, Name = "(Maintenance: Descale)", Data = "", isRoastProfile = false, isGrindProfile = false, isBrewProfile = true });
            RoastProfiles.Add(new RoastProfile() { Id = 92, Name = "(Maintenance: Grinder clean)", Data = "", isRoastProfile = false, isGrindProfile = true, isBrewProfile = false });
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