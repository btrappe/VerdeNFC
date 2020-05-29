using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.IO;
using Android.Content;
using VerdeNFC.ViewModels;

namespace VerdeNFC.Droid
{
    [Activity(Label = "VerdeNFC", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            NfcIo.SetCurrentActivityResolver(() => this);

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            if (!NfcIo.Current.IsAvailable())
            {
                Toast.MakeText(ApplicationContext, "NFC Reading not supported", ToastLength.Long).Show();
            }
            else if (!NfcIo.Current.IsEnabled())
            {
                Toast.MakeText(ApplicationContext, "NFC Reader not enabled. Please turn it on in the settings.", ToastLength.Long).Show();
            }

            // Toast.MakeText(ApplicationContext, "vor LoadApp", ToastLength.Long).Show();
            try
            {
                LoadApplication(new App(Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads)));
            }
            catch (Exception e)
            {
                Toast.MakeText(ApplicationContext, e.Message, ToastLength.Long).Show();
                Toast.MakeText(ApplicationContext, e.StackTrace, ToastLength.Long).Show();
            }
            MainTabViewModel.Current.NFCStartListening += NfcIo.Current.StartListening;
            MainTabViewModel.Current.NFCStopListening += NfcIo.Current.StopListening;

        }

        protected override void OnResume()
        {
            base.OnResume();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            NfcIo.OnNewIntent(intent);
        }

    }
    }