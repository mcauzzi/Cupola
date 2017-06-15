using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cupola;

public class CommandList
{
    private const int DELAY = 100; //Minimum time to wait after a led command
    public const int DEFAULT_TIME = 100;

    private readonly List<Command> list;
    private int time = DEFAULT_TIME;

    public static bool ShouldClose = false;

    public CommandList()
    {
        list = new List<Command>();
    }

    public CommandList(List<Command> list)
    {
        this.list = new List<Command>(list);
    }

    public void ReadFromFile(string fileName)
    {
        var lines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"\config\" + fileName);
        foreach (var str in lines)
            Add(new Command(str));
    }

    public void WriteToFile(string fileName)
    {
        using (var file = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\config\" + fileName))
        {
            foreach (var c in list)
                file.WriteLine(c.ToString());
        }
    }

    public void Add(Command c)
    {
        list.Add(c);
    }

    public void Send(USBConnection usbCon, NikonController camCon)
    {
        bool camConStatus = (camCon != null && camCon.IsConnected);
        bool usbConStatus = (usbCon != null && usbCon.IsConnected);

        if (usbConStatus) usbCon.Open();
        if (camConStatus) camCon.WaitForConnection();
        Console.WriteLine("Inizio a mandare i comandi!");

        var i = 0;
        foreach (var c in list)
        {
            if (ShouldClose)
            {
                ShouldClose = false;
                if (usbConStatus) usbCon.Close();
                if (camConStatus) camCon.WaitForReady();
                return;
            }
            if (camConStatus) camCon.WaitForReady();
            if (c.Type == Command.Cmdtype.PHOTO && camConStatus)
                camCon.Capture();
            else if(usbConStatus)
                c.Send(usbCon);

            Console.WriteLine(c.ToString());

            if (c.Type == Command.Cmdtype.TIME)
                time = c.Value * 10;

            if (c.Type == Command.Cmdtype.VISIBLE || c.Type == Command.Cmdtype.INFRARED ||
                c.Type == Command.Cmdtype.ULTRAVIOLET || c.Type == Command.Cmdtype.PHOTO)
                if (i < list.Count - 1 && list.ElementAt(i + 1).Type != Command.Cmdtype.PHOTO)
                    Thread.Sleep(time);

            Thread.Sleep(DELAY);
            
            i++;
        }
        if (camConStatus) camCon.WaitForReady();
        Console.WriteLine("Finito!");
        if (usbConStatus) usbCon.Close();
    }

    public List<string> ToStringList()
    {
        var str = new List<string>();
        foreach (var c in list)
            str.Add(c.ToString());
        return str;
    }
}