using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using VerdeNFC.Services;
using VerdeNFC.Views;

namespace VerdeNFC
{
    public partial class App : Application
    {

        public App(string FileSaveFolder)
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            DependencyService.Register<DataBag>();

            object value;
            if (!Properties.TryGetValue("FileSaveFolder", out value) || (((string) value) != FileSaveFolder))
            {
                Properties.Remove("FileSaveFolder");
                Properties.Add("FileSaveFolder", FileSaveFolder);
            }

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
