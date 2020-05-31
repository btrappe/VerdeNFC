using System.ComponentModel;
using System.Runtime.CompilerServices;
using VerdeNFC.Services;
using VerdeNFC.ViewModels;
using Xamarin.Forms;

namespace VerdeNFC.Views
{
    public class DataDetailsViewModel : ContentPage
    {
        public IDataBag DataBag => DependencyService.Get<IDataBag>();
        private string _text;

        public DataDetailsViewModel()
        {

            MessagingCenter.Subscribe<MainTabViewModel, byte []>(this, "DataChanged", async (obj, lmem) =>
            {
                DataText = ToString(lmem);
            });

            DataText = ToString(DataBag.GetData()); 
        }

        protected static string ToString(byte [] mem)
        {
            string text = "";
            int index = 0;
            for (int i = 0; i < 20; i++)
            {
                text += string.Format("{0:D2}  {1:X2}{2:X2}{3:X2}{4:X2} \n", i, mem[index], mem[index + 1], mem[index + 2], mem[index + 3]);
                index += 4;
            }
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