using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class CommandList
{
    private const int DELAY = 100; //Minimum time to wait after a led command
    public const int DEFAULT_TIME = 1000;

    private readonly List<Command> list;
    private int time;

    public CommandList()
    {
        time = DEFAULT_TIME;
        list = new List<Command>();
    }

    public CommandList(List<Command> list)
    {
        time = DEFAULT_TIME;
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
        usbCon.Open();
        camCon.WaitForConnection();
        Console.WriteLine("Inizio a mandare i comandi!");

        var i = 0;
        foreach (var c in list)
        {
            camCon.WaitForReady();
            if (c.Type == Command.Cmdtype.PHOTO)
                camCon.Capture();
            else
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
        camCon.WaitForReady();
        Console.WriteLine("Finito!");
        usbCon.Close();
    }

    public List<string> ToStringList()
    {
        var str = new List<string>();
        foreach (var c in list)
            str.Add(c.ToString());
        return str;
    }
}