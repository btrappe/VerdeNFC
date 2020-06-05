using System.ComponentModel;
using System.Runtime.CompilerServices;
using VerdeNFC.Services;
using VerdeNFC.ViewModels;
using Xamarin.Forms;

namespace VerdeNFC.Views
{
    public class DataDetails2ViewModel : ContentPage
    {
        public IDataBag DataBag => DependencyService.Get<IDataBag>();
        private string _text;

        public DataDetails2ViewModel()
        {

            MessagingCenter.Subscribe<MainTabViewModel, byte []>(this, "DataChanged", async (obj, lmem) =>
            {
                DataText = ToString(lmem);
            });

            DataText = ToString(DataBag.GetData()); 
        }

        public static string ToString(byte [] mem)
        {
            string text = "";
            text += string.Format("UUID  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2} \n", mem[0], mem[1], mem[2], mem[3], mem[4], mem[5], mem[6], mem[7], mem[8]);
            text += string.Format("i/LCK {0:X2}{1:X2}{2:X2} \n\n", mem[9], mem[10], mem[11]);
            text += string.Format("OTP   {0:X2}{1:X2}{2:X2}{3:X2} \n", mem[12], mem[13], mem[14], mem[15]);

            text += string.Format("Rec1  Temp1:{0}° Temp2:{1}° Pmax{2:D}%\n      P={3:D}%\n", mem[16], mem[17], mem[18], mem[19]);
            text += string.Format("Rec2  Temp1:{0}° Temp2:{1}° Pmax{2:D}%\n       P={3:D}% {4:D2}s \n", mem[21], mem[22], mem[23], mem[24], mem[25]);
            text += string.Format("Rec3  Temp1:{0}° Temp2:{1}° Pmax{2:D}%\n       P={3:D}%\n", mem[26], mem[27], mem[28], mem[29]);
            text += string.Format("Rec4  Temp1:{0}° Temp2:{1}° Pmax{2:D}%\n       P={3:D}% {4:D2}s \n", mem[31], mem[32], mem[33], mem[34], 20*mem[35]);
            text += string.Format("Rec5  {0:X2}{1:X2} Grind {2:D}s \n\n", mem[37], mem[38], 5*mem[40]);

            text += string.Format("Type  {0:X2} Param {1:D} \n", mem[41], 600*mem[42]);

            text += "command ";

            switch (mem[41])
            {
                case 1:
                    text += "roast, grind and brew"; 
                    break;
                case 2:
                    text += "roast";
                    break;
                case 3:
                    text += "";
                    break;
                case 4:
                    text += "grind";
                    break;
                case 5:
                    text += "grind and brew";
                    break;
                case 6:
                    text += "brew";
                    break;
                case 7:
                    text += "";
                    break;
                case 8:
                    text += "";
                    break;
                case 9:
                    text += "";
                    break;
                case 0x0a:
                    text += "";
                    break;
                case 0x0b:
                    text += "";
                    break;
                case 0x0c:
                    text += "";
                    break;
                case 0x0d:
                    text += "";
                    break;
                case 0x0e:
                    text += "";
                    break;
                case 0x0f:
                    text += "filter usage reset";
                    break;
                case 0x10:
                    text += "";
                    break;
                case 0x11:
                    text += "filter usage set to 30 (request filter)";
                    break;
                case 0x12:
                    text += "grind cleaning with special cleaning beans";
                    break;
                case 0x13:
                    text += "maintenance  (descale brewing system) ";
                    break;
                default:
                    text += "unknown";
                    break;
            }

            text += "\n";
            text += string.Format("CRC16 {0:X2}{1:X2} \n", mem[46], mem[47]);
            text += string.Format("Pause {0} min\n", mem[48] * 0x100 + mem[49]);

            for (int i = 12; i < 20; i++)
                text += string.Format("{0:D2}  {1:X2}{2:X2}{3:X2}{4:X2} \n", i, mem[4 * i], mem[4 * i + 1], mem[4 * i + 2], mem[4 * i + 3]);

            return text;
        }

        public string DataText
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                OnPropertyChanged("DataText");
            }
        }

    }
}
