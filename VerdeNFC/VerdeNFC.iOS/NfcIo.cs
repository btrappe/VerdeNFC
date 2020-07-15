using System;
using System.Linq;
using Foundation;
using CoreNFC;
using CoreFoundation;
using VerdeNFC.ViewModels;

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

            if (!WriteMode)
            {
                // read
                nMifareTag.SendMiFareCommand(NSData.FromArray(new byte[] { 0x30, 0x04 }), (data, error) =>
                {
                    if ((data != null) && (error == null) && (data.Count() == 16))
                    {
                        byte[] FirstTag = MainTabViewModel.Current?.DataBag.GetData();
                        data.ToArray().CopyTo(FirstTag, 16);
                        nMifareTag.SendMiFareCommand(NSData.FromArray(new byte[] { 0x30, 0x08 }), (data2, error2) =>
                        {
                            if ((data2 != null) && (error2 == null) && (data2.Count() == 16))
                            {
                                data2.ToArray().CopyTo(FirstTag, 32);
                                nMifareTag.SendMiFareCommand(NSData.FromArray(new byte[] { 0x30, 0x0C }), (data3, error3) =>
                                {
                                    if ((data3 != null) && (error3 == null) && (data3.Count() == 16))
                                    {
                                        data3.ToArray().CopyTo(FirstTag, 48);
                                        MainTabViewModel.Current?.SetControlsVisibility(FirstTag[41]);
                                        MainTabViewModel.Current?.DataBag.SetData(FirstTag);
                                    }
                                    else
                                    {
                                        if (MainTabViewModel.Current != null)
                                        {
                                            MainTabViewModel.Current.cbNFCRead = false;
                                            MainTabViewModel.Current.cbNFCWrite = false;
                                        }
                                    }
                                });
                            }
                            else
                            {
                                if (MainTabViewModel.Current != null)
                                {
                                    MainTabViewModel.Current.cbNFCRead = false;
                                    MainTabViewModel.Current.cbNFCWrite = false;
                                }
                            }
                        });
                    }
                    else
                    {
                        if (MainTabViewModel.Current != null)
                        {
                            MainTabViewModel.Current.cbNFCRead = false;
                            MainTabViewModel.Current.cbNFCWrite = false;
                        }
                    }
                });
            }
            else
            {
                // write
            }
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

        public void StopListening(bool Dummy)
        {
            WriteMode = false;
            Enabled = false;
            NfcSession?.InvalidateSession();
            NfcSession = null;
        }

        /*
    private NFC;
    private bool WriteMode;
    private bool Enabled;

    public delegate void TagDetectedDelegate(Tag tag);
    public event TagDetectedDelegate TagDetected;

    public NfcIo()
    {
        _nfcAdapter = NfcAdapter.GetDefaultAdapter(CurrentActivity);
        WriteMode = false;
        Enabled = false;
    }

    public bool IsAvailable()
    {
        var context = Application.Context;
        if (context.CheckCallingOrSelfPermission(Manifest.Permission.Nfc) != Permission.Granted)
            return false;

        return _nfcAdapter != null;
    }

    public bool IsEnabled()
    {
        return _nfcAdapter?.IsEnabled ?? false;
    }

    public void StartListening(bool Write)
    {
        if (!IsAvailable())
            throw new InvalidOperationException("NFC not available");

        if (!IsEnabled()) // todo: offer possibility to open dialog
            throw new InvalidOperationException("NFC is not enabled");

        WriteMode = Write;

        var ndefDetected = new IntentFilter(NfcAdapter.ActionNdefDiscovered);
        ndefDetected.AddDataType("* /*");
        var tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
        tagDetected.AddDataType("* /*");
        var filters = new[] { tagDetected };
        var intent = new Intent(CurrentActivity, CurrentActivity.GetType()).AddFlags(ActivityFlags.SingleTop);
        var pendingIntent = PendingIntent.GetActivity(CurrentActivity, 0, intent, 0);
        _nfcAdapter.EnableForegroundDispatch(CurrentActivity, pendingIntent, filters, new[] { new[] { Java.Lang.Class.FromType(typeof(MifareUltralight)).Name } });
        Enabled = true;
        //_nfcAdapter.EnableReaderMode(activity, this, NfcReaderFlags.NfcA | NfcReaderFlags.NoPlatformSounds, null);
    }

    public void StopListening(bool Dummy)
    {
        Enabled = false;
        //_nfcAdapter?.DisableReaderMode(CrossNfc.CurrentActivity);
        //_nfcAdapter?.DisableForegroundDispatch(CurrentActivity); // can be called from OnResume only
    }

    internal void CheckForNfcMessage(Intent intent)
    {
        if (!Enabled)
            return;

        if (intent.Action != NfcAdapter.ActionTechDiscovered)
            return;

        if (!(intent.GetParcelableExtra(NfcAdapter.ExtraTag) is Tag tag))
            return;

        try
        {
            var ev1 = MifareUltralight.Get(tag);

            //TagDetected?.Invoke(tag);

            ev1.Connect();
            byte[] FirstTag = MainTabViewModel.Current?.DataBag.GetData();
            byte[] mem = new byte[80];

            for (int i = 0; i < 20; i += 4)
            {
                byte[] payload = ev1.ReadPages(i);
                Buffer.BlockCopy(payload, 0, mem, 4 * i, 16);
            }

            if (WriteMode)
            {
                byte[] dstData = MainTabViewModel.MergeTagData(FirstTag, mem);


                // password auth
                // var response = ev1.Transceive(new byte[]{
                //            (byte) 0x1B, // PWD_AUTH
                //            0,0,0,0 });

    E            // Check if PACK is matching expected PACK
                // This is a (not that) secure method to check if tag is genuine
                //if ((response != null) && (response.Length >= 2))
                //{
                //}

                for (int i = 4; i < 16; i++)
                    ev1.WritePage(i, dstData.Skip(4 * i).Take(4).ToArray());

                MainTabViewModel.Current?.DataBag.SetData(dstData);
                WriteMode = false;
            }
            else
            {
                MainTabViewModel.Current?.SetControlsVisibility(mem[41]);
                MainTabViewModel.Current?.DataBag.SetData(mem);
            }
            ev1.Close();
            MainTabViewModel.Current.cbNFCRead = false;
            MainTabViewModel.Current.cbNFCWrite = false;

            try
            {
                // Use default vibration length
                Vibration.Vibrate();
            }
            catch (FeatureNotSupportedException ex)
            {
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                // Other error has occurred.
            }
        }
        catch (Exception e)
        {
            try
            {
                Vibration.Vibrate();
                Thread.Sleep(1000);
                Vibration.Vibrate();
            }
            catch (Exception ex)
            {
                // Other error has occurred.
            }

        }
    }

    public void OnTagDiscovered(Tag tag)
    {
        try
        {
            var techs = tag.GetTechList();
            if (!techs.Contains(Java.Lang.Class.FromType(typeof(MifareUltralight)).Name))
                return;

            //   var ndef = Ndef.Get(tag);
            //   ndef.Connect();
            //   var ndefMessage = ndef.NdefMessage;
            //   var records = ndefMessage.GetRecords();
            //   ndef.Close();

        }
        catch
        {
            // handle errors
        }
    }
    */
    }
}

