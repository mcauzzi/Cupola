using System;
using System.Collections.Generic;
using System.IO;
using Nikon;

namespace Cupola
{
    public class NikonController
    {
        private readonly NikonManager man;
        private NikonDevice dev;
        public bool SaveToPc { set; get; }

        public bool IsConnected { private set; get; }
        public bool IsReady { private set; get; }
        public bool IsSaved { private set; get; }

        public delegate void PhotoReceived(NikonImage img);
        public delegate void CameraConnected();
        public event PhotoReceived Received;
        public event CameraConnected Connected;

        protected virtual void OnReceived(NikonImage img)
        {
            Received?.Invoke(img);
        }

        protected virtual void OnConnected()
        {
            Connected?.Invoke();
        }

        public enum Capability
        {
            Iso,
            ShutterSpeed,
            Aperture,
            WhiteBalance,
            Compression
        }

        private readonly Dictionary<Capability, eNkMAIDCapability> capability2Nikon = new Dictionary<Capability, eNkMAIDCapability>()
        {
            { Capability.Iso, eNkMAIDCapability.kNkMAIDCapability_Sensitivity },
            { Capability.ShutterSpeed, eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed },
            { Capability.Aperture, eNkMAIDCapability.kNkMAIDCapability_Aperture },
            { Capability.WhiteBalance, eNkMAIDCapability.kNkMAIDCapability_WBMode },
            { Capability.Compression, eNkMAIDCapability.kNkMAIDCapability_CompressionLevel }
        };

        public NikonController(string md3File)
        {
            SaveToPc = true;
            IsConnected = false;
            IsReady = false;
            IsSaved = false;
            man = new NikonManager(md3File);
            man.DeviceAdded += DeviceAdded;
        }

        #region Setter and Getter
        public int GetCapabilityIndex(Capability capability)
        {
            return IsConnected ? dev.GetEnum(capability2Nikon[capability]).Index : throw new Exception("Camera isn't connected");
        }

        public void SetCapabilityIndex(Capability capability, int index)
        {
            if (IsConnected)
            {
                var capList = dev.GetEnum(capability2Nikon[capability]);
                capList.Index = index;
                if (capList.Index != -1)
                {
                    dev.SetEnum(capability2Nikon[capability], capList);
                }
                else
                {
                    throw new ArgumentException("Wrong " + capability, nameof(index));
                }
            }
            else
            {
                throw new Exception("Camera isn't connected");
            }
        }

        public List<string> GetCapabilityStringList(Capability capability)
        {
            if (IsConnected)
            {
                List<string> res = new List<string>();
                var list = dev.GetEnum(capability2Nikon[capability]);
                for (int i = 0; i < list.Length; i++)
                {
                    res.Add((string) list.GetEnumValueByIndex(i));
                }
                return res;
            }
            throw new Exception("Camera isn't connected");
        }
        #endregion

        public void Capture()
        {
            if (IsConnected && IsReady && IsSaved)
            {
                IsReady = false;
                if (SaveToPc)
                {
                    IsSaved = false;
                }
                dev.Capture();
            }
        }

        public void SetLiveView(bool liveView)
        {
            dev.LiveViewEnabled = liveView;
        }

        public void WaitForConnection()
        {
            while (!IsConnected)
            {
                System.Threading.Thread.Yield();
            }
        }
   
        public void WaitForReady()
        {
            while (!IsSaved)
            {
                System.Threading.Thread.Yield();
            }
            while (!IsReady)
            {
                System.Threading.Thread.Yield();
            }
        }

        private void SaveToFile(byte[] imageBuffer)
        {
            using (var saveFile = new BinaryWriter(File.Open(DateTime.Now.ToString("yyMMddhhmmss") + ".nef", FileMode.Create)))
            {
                foreach (byte b in imageBuffer)
                {
                    saveFile.Write(b);
                }
            }
        
            IsSaved = true;
        }

        public NikonLiveViewImage GetLiveView( )
        {
            NikonLiveViewImage image = null;

            try
            {
                image = dev.GetLiveViewImage();
            }
            catch (NikonException)
            {
                Console.WriteLine("Errore LiveView");
            }

            return image;
        }

        private void DeviceAdded(NikonManager sender, NikonDevice device)
        {
            dev = device;
            if (SaveToPc)
            {
                dev.SetUnsigned(eNkMAIDCapability.kNkMAIDCapability_SaveMedia, (uint) eNkMAIDSaveMedia.kNkMAIDSaveMedia_Card_SDRAM);
            }
            else
            {
                dev.SetUnsigned(eNkMAIDCapability.kNkMAIDCapability_SaveMedia, (uint) eNkMAIDSaveMedia.kNkMAIDSaveMedia_Card);
            }
            dev.ImageReady += DeviceImageReady;
            dev.CaptureComplete += OnCaptureComplete;
            Console.WriteLine("Fotocamera collegata!");
            dev.SetBoolean(eNkMAIDCapability.kNkMAIDCapability_LockCamera, false);
            dev.SetUnsigned(eNkMAIDCapability.kNkMAIDCapability_ApertureLockSetting, 0);

            IsConnected = true;
            IsReady = true;
            IsSaved = true;

            OnConnected();
        }

        private void DeviceImageReady(NikonDevice sender, NikonImage image)
        {
            Console.WriteLine("Immagine Ricevuta!");
            if(SaveToPc)
                SaveToFile(image.Buffer);
            OnReceived(image);
        }

        private void OnCaptureComplete(NikonDevice sender, int data)
        {
            IsReady = true;
        }

        public void Close()
        {
            dev?.SetBoolean(eNkMAIDCapability.kNkMAIDCapability_LockCamera, false);
            man.Shutdown();
        }
    }
}