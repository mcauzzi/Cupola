using System;
using System.Collections.Generic;

using Nikon;
using System.IO;
using System.Windows.Forms;

public class NikonController
{
    private readonly NikonManager man = new NikonManager("Type0014.md3");
    public readonly bool SaveToPc;
    private NikonDevice dev;

    private bool isConnected = false;
    private bool isReady = false;
    private bool isSaved = false;

    private List<string> IsoList = new List<string>();
    private List<string> ShutterList = new List<string>();
    private List<string> FormatList = new List<string>();

    public NikonController(bool saveToPc = false)
    {
        man.DeviceAdded += new DeviceAddedDelegate(DeviceAdded);
        this.SaveToPc = saveToPc;
    }

    public void Capture()
    {
        if (isConnected && isReady && isSaved)
        {
            isReady = false;
            if (SaveToPc)
            {
                isSaved = false;
            }
            dev.Capture();
        }
    }

    private void InitParamList()
    {
        var isoList = dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_Sensitivity);
        for (int i = 0; i < isoList.Length; i++)
        {
            IsoList.Add((string)isoList.GetEnumValueByIndex(i));
        }

        var shutterList = dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_ShutterSpeed);
        for (int i = 0; i < shutterList.Length; i++)
        {
            ShutterList.Add((string)shutterList.GetEnumValueByIndex(i));
        }

        var formatList = dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_CompressionLevel);
        for (int i = 0; i < formatList.Length; i++)
        {
            FormatList.Add((string)formatList.GetEnumValueByIndex(i));
        }

    }

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
        formatList.Index = FormatList.IndexOf(value);
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
        //dev.SetUnsigned(eNkMAIDCapability.kNkMAIDCapability_ISOControlSensitivity, 100);
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
        //Console.WriteLine(dev.GetUnsigned(eNkMAIDCapability.kNkMAIDCapability_ISOControlSensitivity));
        //dev.SetUnsigned(eNkMAIDCapability.kNkMAIDCapability_ISOControlSensitivity, (uint)eNkMAIDISOControlSensitivity3.kNkMAIDISOControlSensitivity3_ISO200);
        //Console.WriteLine(dev.GetFloat(eNkMAIDCapability.kNkMAIDCapability_FocalLength).ToString());
        //Console.WriteLine(dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_Aperture).ToString());
        //
        //Console.WriteLine(dev.GetEnum(eNkMAIDCapability.kNkMAIDCapability_ShootingSpeed).ToString());

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
        while (!isConnected) { }
    }

    public void WaitForReady()
    {
        while (!isSaved) { }
        while (!isReady) { }
    }

    private void SaveToFile(byte[] imageBuffer)
    {
        using (var saveFile = new BinaryWriter(File.Open(DateTime.Now.ToString("yymmddHmmss") + ".nef", FileMode.Create)))
        {
            foreach (byte b in imageBuffer)
            {
                saveFile.Write(b);
            }
        }
        
        isSaved = true;
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

        InitParamList();

        isConnected = true;
        isReady = true;
        isSaved = true;
    }

    private void DeviceImageReady(NikonDevice sender, NikonImage image)
    {
        Console.WriteLine("Immagine Ricevuta!");
        if(SaveToPc)
            SaveToFile(image.Buffer);
    }

    private void OnCaptureComplete(NikonDevice sender, int data)
    {
        Console.WriteLine(data+isReady.ToString());
        isReady = true;
    }

    public void Close()
    {
        man.Shutdown();
    }
}