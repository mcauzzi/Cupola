using System;
using System.Collections.Generic;

using Nikon;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;


public class NikonController
{
    private  NikonManager man;
    private NikonDevice dev;
    public bool SaveToPc { set; get; } = false;

    public bool IsConnected { private set; get; }
    public bool IsReady { private set; get; }
    public bool IsSaved { private set; get; }

    private Dictionary<string, Nikon.eNkMAIDCapability> string2capability = new Dictionary<string, Nikon.eNkMAIDCapability>()
    {
        { "ISO", eNkMAIDCapability.kNkMAIDCapability_Sensitivity},
        { "ShutterSpeed", eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed},
        { "Compression", eNkMAIDCapability.kNkMAIDCapability_CompressionLevel }
    };

    public NikonController(string md3File)
    {
        IsConnected = false;
        IsReady = false;
        IsSaved = false;
        man = new NikonManager(md3File);
        man.DeviceAdded += new DeviceAddedDelegate(DeviceAdded);
    }

    public int GetCapabilityIndex(string capability)
    {
        return IsConnected ? dev.GetEnum(string2capability[capability]).Index : throw new Exception("Camera isn't connected");
    }

    public void SetCapabilityIndex(string capability, int index)
    {
        if (IsConnected)
        {
            var isoList = dev.GetEnum(string2capability[capability]);
            isoList.Index = index;
            if (isoList.Index != -1)
            {
                dev.SetEnum(string2capability[capability], isoList);
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

    public List<string> getCapabilityStringList(string capability)
    {
        if (IsConnected)
        {
            List<string> res = new List<string>();
            var list = dev.GetEnum(string2capability[capability]);
            for (int i = 0; i < list.Length; i++)
            {
                res.Add((string) list.GetEnumValueByIndex(i));
            }
            return res;
        }
        else
        {
            throw new Exception("Camera isn't connected");
        }
    }

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

    #region getParamStringList
    

    public List<string> getShutterStringList()
    {
        List<string> res = new List<string>();
        var shutterIndex = dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed);
        for (int i = 0; i < shutterIndex.Length; i++)
        {
            res.Add((string)shutterIndex.GetEnumValueByIndex(i));
        }
        return res;
    }

    public List<string> getCompressionStringList()
    {
        List<string> res = new List<string>();
        var compressionList = dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_CompressionLevel);
        for (int i = 0; i < compressionList.Length; i++)
        {
            res.Add((string)compressionList.GetEnumValueByIndex(i));
        }
        return res;
    }
    #endregion

    public void SetLiveView(bool liveView)
    {
        dev.LiveViewEnabled = liveView;
    }

    public void SetIso(string value)
    {
        var isoList = dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_Sensitivity);
        isoList.Index = IsoList.IndexOf(value);
        if(isoList.Index != -1)
            dev.SetEnum(eNkMAIDCapability.kNkMAIDCapability_Sensitivity, isoList);
        else
        {
            throw new ArgumentException("Wrong ISO level", nameof(value));
        }
    }

    public void SetShutterSpeed(string value)
    {
        var shutterList = dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed);
        shutterList.Index = ShutterList.IndexOf(value);
        if (shutterList.Index != -1)
            dev.SetEnum(eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed, shutterList);
        else
        {
            throw new ArgumentException("Wrong Shutter Speed", nameof(value));
        }
    }

    public void SetFormat(string value)
    {
        var formatList = dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_CompressionLevel);
        formatList.Index = CompressionList.IndexOf(value);
        if (formatList.Index != -1)
            dev.SetEnum(eNkMAIDCapability.kNkMAIDCapability_CompressionLevel, formatList);
        else
        {
            throw new ArgumentException("Wrong Format", nameof(value));
        }
    }

    public void SetFocus()
    {
        // Get the current manual focus 'drive step'
        NikonRange driveStep = dev.GetRange(eNkMAIDCapability.kNkMAIDCapability_MFDriveStep);

        // Set the drive step to max
        driveStep.Value = driveStep.Max;
        dev.SetRange(eNkMAIDCapability.kNkMAIDCapability_MFDriveStep, driveStep);

        // Drive all the way to 'closest'
        //DriveManualFocus(eNkMAIDMFDrive.kNkMAIDMFDrive_InfinityToClosest);

        // Set the drive step to something small
        driveStep.Value = 200.0;
        dev.SetRange(eNkMAIDCapability.kNkMAIDCapability_MFDriveStep, driveStep);

        // Drive manual focus towards infinity in small steps
        for (int i = 0; i < 10; i++)
        {
         //   DriveManualFocus(eNkMAIDMFDrive.kNkMAIDMFDrive_ClosestToInfinity);
        }
    }

    public void SetParams()
    {
        dev.SetBoolean(eNkMAIDCapability.kNkMAIDCapability_LockCamera, true);

        NikonEnum shootingMode = dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_ShootingMode);

        for (int i = 0; i < shootingMode.Length; i++)
        {
            Console.WriteLine(i + ") " + shootingMode.GetEnumValueByIndex(i));
        }

        //shootingMode.Index = 3;
        //dev.SetEnum(eNkMAIDCapability.kNkMAIDCapability_ShootingMode, shootingMode);
        //dev.SetUnsigned(eNkMAIDCapability.kNkMAIDCapability_ContinuousShootingNum, 5);
        //var shutterspeed = device.GetEnum(eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed).ToString()));

        SetIso("100");
        SetShutterSpeed("1/40");
        SetFormat("RAW");

        /*
        // Set White Balance to Custom
        var wbList = dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_WBMode);
        for (int i = 0; i < wbList.Length; i++)
        {
            Console.WriteLine(i + ") " + wbList.GetEnumValueByIndex(i));
        }
        wbList.Index = ?;
        dev.SetEnum(eNkMAIDCapability.kNkMAIDCapability_WBMode, wbList);
        */

        Console.WriteLine(dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_Sensitivity).ToString());
        Console.WriteLine(dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed).ToString());
        Console.WriteLine(dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_CompressionLevel).ToString());
        Console.WriteLine(dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_WBMode).ToString());

        //dev.SetBoolean(eNkMAIDCapability.kNkMAIDCapability_LockCamera, false);
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

    public NikonLiveViewImage getLiveView( )
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
        dev.ImageReady += new ImageReadyDelegate(DeviceImageReady);
        dev.CaptureComplete += new CaptureCompleteDelegate(OnCaptureComplete);
        Console.WriteLine("Fotocamera collegata!");
        InitParamList();

        IsConnected = true;
        IsReady = true;
        IsSaved = true;
    }

    private void DeviceImageReady(NikonDevice sender, NikonImage image)
    {
        Console.WriteLine("Immagine Ricevuta!");
        if(SaveToPc)
            SaveToFile(image.Buffer);
    }

    private void OnCaptureComplete(NikonDevice sender, int data)
    {
        IsReady = true;
    }

    public void Close()
    {
        man.Shutdown();
    }
}