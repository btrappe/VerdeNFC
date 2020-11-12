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
using System.Collections.ObjectModel;

namespace VerdeNFC.ViewModels
{
    public class PauseDuration : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // minutes
            int v1 = GetMinutes((int)value);

            if (v1 < 60)
                return string.Format("{0} min", v1);
            if (v1 < 1440)
                return string.Format("{0}h {1}min", (int)(v1 / 60), v1 % 60);
            return string.Format("{0} days {1}h {2}min", (int)v1 / 1440, (int)((v1 % 1440) / 60), v1 % 60);
        }

        public static int GetMinutes(int v)
        {
            // first 2 hours - 1 minute steps 
            if (v < 120)
                return v;
            // 2-6 hours - 5 minute steps - (6-2)*12=48
            else if (v < 168)
                return 120 + 5 * (v - 120);
            // 6-24h -  15min steps (24-6)*4 = 72 steps
            else //if (v < 240)
                return 360 + 15 * (v - 168);
        }

        public static int GetSliderTicks(int v)
        {
            // first 2 hours - 1 minute steps 
            if (v < 120)
                return v;
            // 2-6 hours - 5 minute steps - (6-2)*12=48
            else if (v < 360)
                return (int)(v - 120) / 5 + 120;
            // 6-24h -  15min steps (24-6)*4 = 72 steps
            else //if (v < 240)
                return (v-360) / 15 + 168;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MainTabViewModel : BaseViewModel
    {
        public static MainTabViewModel Current;

        public IDataBag DataBag => DependencyService.Get<IDataBag>();
        
        public delegate void NFCControlListening(bool Write);
        public event NFCControlListening NFCStartListening;
        public event NFCControlListening NFCStopListening;

        #region RoastProfilesPicker
        private ObservableCollection<RoastProfile> _roastProfiles = new ObservableCollection<RoastProfile>();
        public ObservableCollection<RoastProfile> RoastProfiles
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
            // if changed manually in picker (this function is also called, if we programmatically change RoastProfileSel)m
            // we want to remove formerly selected special (id >= 100) entry, so it connot be choosen manually in the picker again
            if (RoastProfileSel.isManualChoiceAllowed ||                       // manually template choosen or
                (RoastProfileSel.Id != lastSelectedRoastProfile.Id))           // programmatically other special entry choosen
            {
                if ((lastSelectedRoastProfile?.Id > 100) || (lastSelectedRoastProfile?.Id == 0))
                {
                    RoastProfiles.Remove(lastSelectedRoastProfile);
                    OnPropertyChanged(nameof(RoastProfiles));
                }
            }

            if (!string.IsNullOrEmpty(RoastProfileSel.Data))
            {
                // if (manually) template choosen -> load template
                byte[] mem = new byte[80];
                int index = 16;
                string line = RoastProfileSel.Data;
                if (string.IsNullOrEmpty(line))
                    return;

                try
                {
                    int j = 0;
                    while ((j < line.Length) && (index < 80))
                    {
                        if ((line[j] == ' ') || (line[j] == '\t'))
                        {
                            j++;
                            continue;
                        }

                        mem[index++] = byte.Parse(line.Substring(j, 2), NumberStyles.HexNumber);
                        j += 2;
                    }
                }
                catch
                {
                    Console.WriteLine("Error: unknown line format: {0}", line);
                }

                nPause = PauseDuration.GetSliderTicks(256 * mem[48] + mem[49]);
                SetControlsVisibility(mem[41], false);

                DataBag.SetData(mem);
                MessagingCenter.Send(this, "DataChanged", DataBag.GetData());
            }

            lastSelectedRoastProfile = RoastProfileSel;
        }
        #endregion

        #region NFCReadButton
        bool _cbNFCRead;
        public bool cbNFCRead
        {
            get
            {
                return _cbNFCRead;
            }
            set
            {
                if (value == _cbNFCRead)
                    return;

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
        #endregion
        
        #region NFCWriteButton
        public bool cbNFCWriteEnabled
        {
            get
            {
                return !_cbNFCRead;
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
                if (value == _cbNFCWrite)
                    return;

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
        #endregion
        
        #region MultiUseCB
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
        private bool _cbMultiUseEnabled;
        public bool cbMultiUseEnabled
        {
            get
            {
                return _cbMultiUseEnabled;
            }
            set
            {
                _cbMultiUseEnabled = value;
                OnPropertyChanged("cbMultiUseEnabled");
            }
        }
        #endregion

        #region nPause
        private int _nPause;
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
                mem[48] = Convert.ToByte(PauseDuration.GetMinutes(_nPause) / 256);
                mem[49] = Convert.ToByte(PauseDuration.GetMinutes(_nPause) % 256);
                DataBag.SetData(mem);
                OnPropertyChanged("nPause");
                MessagingCenter.Send(this, "DataChanged", DataBag.GetData());
            }
        }

        private bool _nPauseEnabled;
        public bool nPauseEnabled
        {
            get
            {
                return _nPauseEnabled;
            }
            set
            {
                _nPauseEnabled = value;
                OnPropertyChanged("nPauseEnabled");
            }
        }
        #endregion

        readonly string _downloadFolder;

        public MainTabViewModel()
        {
            Title = "VerdeNFC 1.0";

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

            // sections :  1 - 40 -> RGB profiles from Bonaverde's suppliers
            //            41 - 60 -> roast profiles
            //            81 - 90 -> grind & brew profiles
            //            91 - 99 -> maintenance functions
//          Bonaverde supplier, but no rfid known yet.
//          RoastProfiles.Add(new RoastProfile() { Id =  X, Name = "Rodolfo Ruffatti/El Salvador (x/y)",     Data = "", isManualChoiceAllowed = true });
//          RoastProfiles.Add(new RoastProfile() { Id =  X, Name = "Aldo Parducci/El Salvador (x/y)",        Data = "", isManualChoiceAllowed = true });
//          RoastProfiles.Add(new RoastProfile() { Id =  X, Name = "Exelso Cafe/Colombia (x/y)",             Data = "", isManualChoiceAllowed = true });
//          RoastProfiles.Add(new RoastProfile() { Id =  X, Name = "Female Growers/Guatamala (x/y)",         Data = "", isManualChoiceAllowed = true });
//          RoastProfiles.Add(new RoastProfile() { Id =  X, Name = "Henry Hueck/Nicaragua (x/y)",            Data = "", isManualChoiceAllowed = true });
//          RoastProfiles.Add(new RoastProfile() { Id =  X, Name = "Roast : Puerto Escondido (x)",           Data = "", isManualChoiceAllowed = true });

            RoastProfiles.Add(new RoastProfile() { Id =  0, Name = "(select one)",                           Data = "", isManualChoiceAllowed = false });

            RoastProfiles.Add(new RoastProfile() { Id =  1, Name = "Ricardo Tavares/Brazil (50gr/6dl)",      Data = "AAA0504B 05AAAA41 4678AAB4 414B05AA B4414B96 375A5A2D 23011E00 0601891A 0000", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id =  2, Name = "Ricardo Tavares/Brazil (80gr/9dl)",      Data = "AAB84B4B 00AAB832 465AAAB6 4B5A00AA B6324664 3C465A2D 32011E00 05015DFD 0005", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id =  3, Name = "Flor de Corazon/Mexico (55gr/6dl)",      Data = "AAB4324B 00AAB432 4650AAAE 325A00AA AE32465A 3C465A2D 23010F00 0501ED37 0003", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id =  4, Name = "Flor de Corazon/Mexico (80gr/9dl)",      Data = "AAB44B4B 00AAB432 4655AAB4 4B5A00AA B432465F 3C465A2D 32011E00 05012C78 0001", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id =  5, Name = "Ocean College/Costa Rica (84gr/9dl)",    Data = "AAB84B4B 00AAB832 465AAAB6 4B5A00AA B6324664 3C465A2D 32011E00 0501F205 0005", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id =  6, Name = "Luis Alberto/Nicaragua (80gr/9dl)",      Data = "AAB84B4B 00AAB832 465AAAB6 4B5A00AA B6324664 3C465A2D 32011E00 050149C9 0005", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id =  7, Name = "Alfaro Family/El Salvador (80gr/9dl)",   Data = "AAB04B5A 05AAB041 5078AAB4 415A05AA B4415050 37465A50 32011E00 06013F40 0005", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id =  8, Name = "Surprise 1 (80gr/9dl)",                  Data = "AAB84B4B 00AAB832 465FAAB6 4B5A00AA B632465A 3C465A2D 32011E00 05019D29 0005", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id =  9, Name = "Wallace Family/Colombia (50gr/6dl)",     Data = "AAB0644B 05AAB041 4678AABA 5F5A05AA BA41463C 376E5A2D 23011E00 060171AC 0000", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 10, Name = "Wallace Family/Colombia (80gr/9dl)",     Data = "AAB64B4B 00AAB632 464BAAB4 4B5A00AA B4324664 3C465A2D 32011E00 0501F066 0005", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 11, Name = "Wallace Family/Colombia alt (80gr/9dl)", Data = "AAB64C4B 00AAB638 464EAAB6 4C5A00AA B6334664 3C465A2D 32011E00 0501B282 0005", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 12, Name = "Tavares Night roast/brew (10h/80gr/9dl)",Data = "AAB84B4B 00AAB832 465AAAB6 4B5A00AA B6324664 3C465A2D 32012D00 0501CCFE 0258", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 13, Name = "Coffee Changer Badge",                   Data = "AA96644B 05AA9641 3278AAA2 5F5005AA B6414696 376E5A2D 23010F00 0601AB13 0000", isManualChoiceAllowed = true });

            RoastProfiles.Add(new RoastProfile() { Id = 41, Name = "Roast : universal, light (50gr/6dl)",    Data = "AAB94B32 05AAB94B 325AAAB9 4B3205AA B94B3232 3C463250 32021E00 06015D03 000A", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 42, Name = "Roast : universal, medium (50gr/6dl)",   Data = "AA645A32 05AA9632 327DAAB4 463C05AA C0323C78 376E3C2D 23020F00 0601F501 000A", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 43, Name = "Roast : universal, dark (50gr/6dl)",     Data = "AA645A32 05AA9632 327DAAB4 463C05AA C0323C96 376E3C2D 23020F00 06018312 000A", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 44, Name = "Roast : Alfaro Family (80gr/9dl)",       Data = "AAAE4B5A 05AAAE41 5078AAB4 415A05AA B4415050 37465A50 23020F00 06016113 0000", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 44, Name = "Roast : universal (80gr/1L)",            Data = "AAB24B4B 00AAB232 4678AAB5 4B5A00AA B5324655 3C465A2D 32021E00 05017903 0005", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 45, Name = "Roast : postcard (50gr/6dl)",            Data = "AAA0644B 05AAA041 4658AAB2 5FF005AA B241468C 376E5A2D 23020F00 06019D11 0001", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 46, Name = "Roast : The Wave (50gr/6dl)",            Data = "AAA0644B 05AAA041 4655AAB2 5F5005AA B2414687 375A5A2D 23020F00 0601740F 0001", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 47, Name = "Roast : Flash Pump (80gr/9dl)",          Data = "AAAA644B 00AAB841 4664AAB4 414600AA B4414655 3C463C50 3C023C00 05011E10 0007", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 48, Name = "Roast : Ojo de Aqua (80gr/9dl)",         Data = "AAB44B4B 05AAB44B 465FAAB2 4B4B05AA B24B4687 373C4650 32023C00 06016A13 0007", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 49, Name = "Roast : Monpaka (80gr/9dl)",             Data = "AAAA644B 00AAB441 4696AAB2 415A00AA B041465F 3C5A5A2D 23020F00 0501EB10 0000", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 50, Name = "Roast : Flor de Corazon (80gr/9dl)",     Data = "AA92644B 05AA9241 4678AAAC 5F5005AA B23C4696 37465A2D 23020F00 0601E401 0001", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 51, Name = "Roast : Ricardo Tavares (80gr/9dl)",     Data = "AAA0644B 05AAA041 4655AAB2 5F5005AA B2414687 37465A2D 23030F00 06011E12 0000", isManualChoiceAllowed = true });
 
            RoastProfiles.Add(new RoastProfile() { Id = 81, Name = "Grind & Brew (20/50gr)",                 Data = "AAB44B4B 00AAB432 464BAAAE 4B5A00AA AE32465A 3C6E5A14 14052D00 05014A01 0000", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 82, Name = "Grind & Brew (80gr)",                    Data = "AAB0644B 05AAB041 4678AABA 5F5A05AA BA41463C 376E5A2D 23051E00 0601AB08 0000", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 83, Name = "Brew only (70C)",                        Data = "AAAA644B 00AAB841 4664AAB2 415A00AA B0414655 3C465A2D 23060F00 0501A602 0000", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 84, Name = "Brew only (90C)",                        Data = "AA41644B 05AA4B41 3278AA56 5F5005AA 5D414696 37375A2D 23060F00 06012BF2 0000", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 85, Name = "Grind only",                             Data = "AAB24B4B 00AAB232 4678AAB5 4B5A00AA B5324655 3C465A2D 32041E00 05011A02 0005", isManualChoiceAllowed = true });
            
            RoastProfiles.Add(new RoastProfile() { Id = 97, Name = "Descale brewsystem",                     Data = "AAB44B4B 05AAB44B 465FAAB2 4B5005AA B24B4687 37465A2D 23132D00 06017F56 0007", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 98, Name = "Clean grinder",                          Data = "AAB44B4B 00AAB432 464EAAB4 4B5A99AA B4324666 3C465A2D 32121E00 05018D5E 0005", isManualChoiceAllowed = true });
            RoastProfiles.Add(new RoastProfile() { Id = 99, Name = "Reset air filter counter",               Data = "AA96644B 05AA9641 3278AAAC 5F5005AA B6414696 376E5A2D 230F0F00 06018D7C 0000", isManualChoiceAllowed = true });

            RoastProfileSel = RoastProfiles[RoastProfiles.Count - 1];
            lastSelectedRoastProfile = RoastProfiles[RoastProfiles.Count - 1];

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

                nPause = PauseDuration.GetSliderTicks(256 * mem[48] + mem[49]);
                SetControlsVisibility(mem[41]);

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

        public void SetControlsVisibility(byte Type, bool externalData=true)
        {
            RoastProfile r;
            switch (Type)
            {
                case 1:
                    r = new RoastProfile() { Id = 101, Name = "(RGB from NFC or File)", Data = "", isManualChoiceAllowed = false };
                    nPauseEnabled = true;
                    cbMultiUseEnabled = true;
                    break;
                case 2:
                    r = new RoastProfile() { Id = 102, Name = "(Roast from NFC or File)", Data = "", isManualChoiceAllowed = false };
                    nPauseEnabled = false;
                    cbMultiUseEnabled = true;
                    break;
                case 4:
                    r = new RoastProfile() { Id = 104, Name = "(Grind from NFC or File)", Data = "", isManualChoiceAllowed = false };
                    nPauseEnabled = false;
                    cbMultiUseEnabled = true;
                    break;
                case 5:
                    r = new RoastProfile() { Id = 103, Name = "(Grind/Brew from NFC or File)", Data = "", isManualChoiceAllowed = false };
                    nPauseEnabled = false;
                    cbMultiUseEnabled = true;
                    break;
                case 6:
                    r = new RoastProfile() { Id = 105, Name = "(Brew from NFC or File)", Data = "", isManualChoiceAllowed = false };
                    nPauseEnabled = false;
                    cbMultiUseEnabled = true;
                    break;
                case 0x0f:
                    r = new RoastProfile() { Id = 99, Name = "Reset air filter counter", Data = "AA96644B 05AA9641 3278AAAC 5F5005AA B6414696 376E5A2D 230F0F00 06018D7C 0000", isManualChoiceAllowed = true };
                    nPauseEnabled = false;
                    cbMultiUseEnabled = true;
                    break;
                case 0x12:
                    r = new RoastProfile() { Id = 98, Name = "Clean grinder", Data = "AAB44B4B00 AAB432464E  AAB44B5A99 AAB4324666 3C465A2D 32 12  1E  000501 8D5E 0005", isManualChoiceAllowed = true };
                    nPauseEnabled = false;
                    cbMultiUseEnabled = true;
                    break;
                case 0x13:
                    r = new RoastProfile() { Id = 97, Name = "Descale brewsystem", Data = "AAB44B4B05 AAB44B465F  AAB24B5005 AAB24B4687 37465A2D 23 13  2D  000601 7F56 0007", isManualChoiceAllowed = true };
                    nPauseEnabled = false;
                    cbMultiUseEnabled = true;
                    break;

                // RoastProfiles.Add(new RoastProfile() { Id = 101, Name = "(RGB from NFC or File)",        Data = "", isManualChoiceAllowed = true});
                // RoastProfiles.Add(new RoastProfile() { Id = 102, Name = "(Roast from NFC or File)",      Data = "", isManualChoiceAllowed = true});
                // RoastProfiles.Add(new RoastProfile() { Id = 103, Name = "(Grind/Brew from NFC or File)", Data = "", isManualChoiceAllowed = false});
                // RoastProfiles.Add(new RoastProfile() { Id = 104, Name = "(Grind from NFC or File)",      Data = "", isManualChoiceAllowed = false});
                // RoastProfiles.Add(new RoastProfile() { Id = 105, Name = "(Brew from NFC or File)",       Data = "", isManualChoiceAllowed = false});

                default:
                    r = new RoastProfile() { Id = 92, Name = "(unknown)", Data = "", isManualChoiceAllowed = false };
                    nPauseEnabled = false;
                    cbMultiUseEnabled = true;
                    break;
            }
            
            if (externalData)
            {
                if (RoastProfiles.Where(p => p.Id == r.Id).FirstOrDefault() == null)
                    RoastProfiles.Add(r);

                // workaround for throwing OutOfRangeException in OnRoastProfileSelChanged on ios
                if ((lastSelectedRoastProfile?.Id > 100) || (lastSelectedRoastProfile?.Id == 0))
                    RoastProfiles.Remove(lastSelectedRoastProfile);

                RoastProfileSel = RoastProfiles.Where(p => p.Id == r.Id).First();
            }
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

        public ICommand OpenFilePickerSrc { get; }
        public ICommand OpenFilePickerDest { get; }
    }
}
