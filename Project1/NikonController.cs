using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nikon;
using System.IO;

public class NikonController
{
    private NikonManager man = new NikonManager("Type0014.md3");
    public NikonDevice dev;
    private bool isConnected = false;
    private bool isReady = false;
    private bool isSaved = false;

    public NikonController()
    {
        man.DeviceAdded += new DeviceAddedDelegate(DeviceAdded);
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
    }

    public void Capture()
    {
        if (isConnected && isReady && isSaved)
        {
            isReady = false;
            isSaved = false;
            dev.Capture();
        }
    }

    public void WaitForConnection()
    {
        while (!isConnected) { }
    }

    public void WaitForSave()
    {
        while (!isSaved) { }
        while (!isReady) { }
    }

    private void SaveToFile(byte[] imageBuffer)
    {
        using (BinaryWriter saveFile = new BinaryWriter(File.Open(DateTime.Now.ToString("yymmddHmmss") + ".nef", FileMode.Create)))
        {
            foreach (byte b in imageBuffer)
            {
                saveFile.Write(b);
            }
        }

        isSaved = true;
    }

    public void getCapabilities(){
        if (isConnected)
        {
            NkMAIDCapInfo[] caps = dev.GetCapabilityInfo();

            foreach (NkMAIDCapInfo cap in caps)
            {
                if (cap.CanSet())
                {
                    Console.Write("Modifica del parametro {0}, valore attuale=");
                }
            }
        }
    }

    void DeviceAdded(NikonManager sender, NikonDevice device)
    {
        dev = device;
        dev.SetUnsigned(eNkMAIDCapability.kNkMAIDCapability_SaveMedia,(uint) eNkMAIDSaveMedia.kNkMAIDSaveMedia_Card_SDRAM);
        dev.ImageReady += new ImageReadyDelegate(DeviceImageReady);
        dev.CaptureComplete += new CaptureCompleteDelegate(OnCaptureComplete);
        isConnected = true;
        isReady = true;
        isSaved = true;
    }

    void DeviceImageReady(NikonDevice sender, NikonImage image)
    {
        Console.WriteLine("Immagine Ricevuta!");
        SaveToFile(image.Buffer);
    }

    void OnCaptureComplete(NikonDevice sender, int data)
    {
        Console.WriteLine(data+isReady.ToString());
        isReady = true;
    }

    void OnProcessExit(object sender,EventArgs e)
    {
        man.Shutdown();
    }
}