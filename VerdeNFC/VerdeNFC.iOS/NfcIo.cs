using System;
using System.Linq;
using Foundation;
using CoreNFC;
using CoreFoundation;
using VerdeNFC.ViewModels;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace VerdeNFC.iOS
{
    public class NfcIo : NFCTagReaderSessionDelegate
    {
        static NfcIo _current;
        public static bool registered = false;
        public static NfcIo Current { get { if (_current == null) _current = new NfcIo(); return _current; } }
        private bool Enabled;
        NFCTagReaderSession NfcSession { get; set; }
        private bool WriteMode;

        NfcIo()
        {
            WriteMode = false;
        }

        public Task<byte[]> Read4Pages(INFCMiFareTag tag, byte page)
        {
            var tcs = new TaskCompletionSource<byte[]>();
            tag.SendMiFareCommand(NSData.FromArray(new byte[] { 0x30, page }), (data, error) =>
            {
                if ((data != null) && (error == null) && (data.Count() == 16))
                    tcs.SetResult(data.ToArray());
                else
                    tcs.SetResult(null);
            });
            return tcs.Task;
        }

        public Task<bool> WritePage(INFCMiFareTag tag, byte page, byte [] data)
        {
            var tcs = new TaskCompletionSource<bool>();
            tag.SendMiFareCommand(NSData.FromArray(new byte[] { 0xA2, page, data[0], data[1], data[2], data[3] }),
                (tagdata, error) => tcs.SetResult((tagdata != null) && (error == null) && (tagdata.Count() == 1)));
            return tcs.Task;
        }

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

            var t = WriteMode ? Task.Run(() => HandleWiteTag(nMifareTag)):
                                Task.Run(() => HandleReadTag(nMifareTag));
        }

        public override void DidInvalidate(NFCTagReaderSession session, NSError error)
        {
            MainTabViewModel.Current.cbNFCRead = false;
            MainTabViewModel.Current.cbNFCWrite = false;
        }

        public bool IsAvailable()
        {
            NfcSession = new NFCTagReaderSession(NFCPollingOption.Iso14443, this, null)
            {
                AlertMessage = "NFC not available."
            };

            if (NfcSession != null)
            {
                NfcSession.InvalidateSession();
                return true;
            }
            return false;
        }

        public bool IsEnabled()
        {
            return true;
        }

        public void StartListening(bool Write)
        {
            if (!IsAvailable())
                throw new InvalidOperationException("NFC not available");

            if (!IsEnabled()) // todo: offer possibility to open dialog
                throw new InvalidOperationException("NFC is not enabled");

            WriteMode = Write;

            NfcSession = new NFCTagReaderSession(NFCPollingOption.Iso14443, this, null)
            {
                AlertMessage = "Present your NFC tag"
            };

            NfcSession?.BeginSession();
            Enabled = true;
        }

        private async void HandleReadTag(INFCMiFareTag nMifareTag)
        {
            byte[] FirstTag = MainTabViewModel.Current?.DataBag.GetData();
            byte[] data = await Read4Pages(nMifareTag, 4);

            if (data != null)
            {
                data.CopyTo(FirstTag, 16);
                data = await Read4Pages(nMifareTag, 8);

                if (data != null)
                {
                    data.CopyTo(FirstTag, 32);
                    data = await Read4Pages(nMifareTag, 12);

                    if (data != null)
                    {
                        data.CopyTo(FirstTag, 48);
                        MainThread.BeginInvokeOnMainThread(() => MainTabViewModel.Current?.SetControlsVisibility(FirstTag[41]));
                        MainTabViewModel.Current?.DataBag.SetData(FirstTag);
                    }
                }
            }

            MainTabViewModel.Current.cbNFCRead = false;
            MainTabViewModel.Current.cbNFCWrite = false;
        }

        private async void HandleWiteTag(INFCMiFareTag nMifareTag)
        {
            byte[] data = await Read4Pages(nMifareTag, 0);

            // write
            if (data != null)
            {
                byte[] FirstTag = MainTabViewModel.Current?.DataBag.GetData();
                byte[] mem = new byte[80];
                data.CopyTo(mem, 0);
                byte[] dstData = MainTabViewModel.MergeTagData(FirstTag, mem);

                bool result = true;
                for (byte i = 4; result && (i < 16); i++)
                    result = await WritePage(nMifareTag, i, dstData.Skip(4 * i).Take(4).ToArray());

                if (result)
                    MainTabViewModel.Current?.DataBag.SetData(dstData);
            }

            MainTabViewModel.Current.cbNFCRead = false;
            MainTabViewModel.Current.cbNFCWrite = false;
        }

        public void StopListening(bool Dummy)
        {
            WriteMode = false;
            Enabled = false;
            NfcSession?.InvalidateSession();
            NfcSession = null;
        }
    }
}

