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
                string ltext = "";

                ltext += string.Format("UUID  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2} \n", lmem[0], lmem[1], lmem[2], lmem[3], lmem[4], lmem[5], lmem[6], lmem[7], lmem[8]);
                ltext += string.Format("CFG   {0:X2}{1:X2}{2:X2} \n\n", lmem[9], lmem[10], lmem[11]);
                ltext += string.Format("OTP   {0:X2}{1:X2}{2:X2}{3:X2} \n", lmem[12], lmem[13], lmem[14], lmem[15]);

                ltext += string.Format("Rec1  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2} \n", lmem[16], lmem[17], lmem[18], lmem[19], lmem[20]);
                ltext += string.Format("Rec2  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2} \n", lmem[21], lmem[22], lmem[23], lmem[24], lmem[25]);
                ltext += string.Format("Rec3  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2} \n", lmem[26], lmem[27], lmem[28], lmem[29], lmem[30]);
                ltext += string.Format("Rec4  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2} \n\n", lmem[31], lmem[32], lmem[33], lmem[34], lmem[35]);

                ltext += string.Format("Cmd1  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2} \n", lmem[36], lmem[37], lmem[38], lmem[39], lmem[40]);
                ltext += string.Format("Cmd2  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2} \n\n", lmem[41], lmem[42], lmem[43], lmem[44], lmem[45]);
                ltext += string.Format("CRC16 {0:X2}{1:X2} \n", lmem[46], lmem[47]);

                for (int i = 12; i < 20; i++)
                    ltext += string.Format("{0:D2}  {1:X2}{2:X2}{3:X2}{4:X2} \n", i, lmem[4*i], lmem[4*i + 1], lmem[4*i + 2], lmem[4*i + 3]);

                DataText = ltext;
            });

            byte[] mem = DataBag.GetData();

            string text="";
            text += string.Format("UUID  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2} \n", mem[0], mem[1], mem[2], mem[3], mem[4], mem[5], mem[6], mem[7], mem[8]);
            text += string.Format("CFG   {0:X2}{1:X2}{2:X2} \n\n", mem[9], mem[10], mem[11]);
            text += string.Format("OTP   {0:X2}{1:X2}{2:X2}{3:X2} \n", mem[12], mem[13], mem[14], mem[15]);

            text += string.Format("Rec1  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2} \n", mem[16], mem[17], mem[18], mem[19], mem[20]);
            text += string.Format("Rec2  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2} \n", mem[21], mem[22], mem[23], mem[24], mem[25]);
            text += string.Format("Rec3  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2} \n", mem[26], mem[27], mem[28], mem[29], mem[30]);
            text += string.Format("Rec4  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2} \n\n", mem[31], mem[32], mem[33], mem[34], mem[35]);

            text += string.Format("Cmd1  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2} \n", mem[36], mem[37], mem[38], mem[39], mem[40]);
            text += string.Format("Cmd2  {0:X2}{1:X2}{2:X2}{3:X2}{4:X2} \n\n", mem[41], mem[42], mem[43], mem[44], mem[45]);
            text += string.Format("CRC16 {0:X2}{1:X2} \n", mem[46], mem[47]);

            for (int i = 12; i < 20; i++)
                text += string.Format("{0:D2}  {1:X2}{2:X2}{3:X2}{4:X2} \n", i, mem[4 * i], mem[4 * i + 1], mem[4 * i + 2], mem[4 * i + 3]);

            DataText = text; 
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