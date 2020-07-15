using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

using VerdeNFC.ViewModels;
using CoreNFC;
using CoreFoundation;

namespace VerdeNFC.iOS
{
    public class NFCDelegate : NFCTagReaderSessionDelegate
    {
        /// <summary>
		/// Event raised when NFC tags are detected
		/// </summary>
		/// <param name="session">iOS <see cref="NFCTagReaderSession"/></param>
		/// <param name="tags">Array of iOS <see cref="INFCTag"/></param>
		public override void DidDetectTags(NFCTagReaderSession session, INFCTag[] tags)
        {
            var _tag = tags.First();

            var connectionError = string.Empty;
            session.ConnectTo(_tag, (error) =>
            {
                if (error != null)
                {
                    connectionError = error.LocalizedDescription;
                    return;
                }
            });

            var nMifareTag = _tag.GetNFCMiFareTag();
            //nMifareTag.SendMiFareCommand();

        }

        public override void DidInvalidate(NFCTagReaderSession session, NSError error)
        {
          //  throw new NotImplementedException();
        }
    };

    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        NFCTagReaderSession NfcSession { get; set; }
        NFCDelegate nfc = new NFCDelegate();
            
        public override void OnActivated(UIApplication uiApplication)
        {
            base.OnActivated(uiApplication);
            if (!NfcIo.registered && (MainTabViewModel.Current != null))
            {
                MainTabViewModel.Current.NFCStartListening += NfcIo.Current.StartListening;
                MainTabViewModel.Current.NFCStopListening += NfcIo.Current.StopListening;
                NfcIo.registered = true;
            }
            
            //NfcIo.Current.StartListening(true);
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
