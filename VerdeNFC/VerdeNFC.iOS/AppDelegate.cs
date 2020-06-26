using System;
using System.Collections.Generic;
using System.Linq;
using Plugin.NFC;
using Plugin.Toast;

using Foundation;
using UIKit;

namespace VerdeNFC.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override void OnActivated(UIApplication uiApplication)
        {
            base.OnActivated(uiApplication);
            if (CrossNFC.IsSupported)
            {
                if (!CrossNFC.Current.IsAvailable)
                    CrossToastPopUp.Current.ShowToastMessage("NFC not supported");
                else if (!CrossNFC.Current.IsEnabled)
                    CrossToastPopUp.Current.ShowToastMessage("NFC not enabled");
                else
                    CrossToastPopUp.Current.ShowToastMessage("NFC ready");


            }

        }

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App(""));

            return base.FinishedLaunching(app, options);
        }
    }
}
