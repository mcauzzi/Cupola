using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CommandList
{
    private const int DELAY = 100;       //Minimum time to wait after a led command
    public const int DEFAULT_TIME = 1000;

    private List<Command> list;
    private int time;
    private NikonController camCon;

    public CommandList(NikonController control)
    {
        time = DEFAULT_TIME;
        this.list = new List<Command>();
        this.camCon = control;
    }

    public CommandList(List<Command> list, NikonController control)
    {
        time = DEFAULT_TIME;
        this.list = new List<Command>(list);
        this.camCon = control;
    }

    public void ReadFromFile(String fileName) {
        string[] lines = System.IO.File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"\config\" + fileName);
        foreach (string str in lines)
            Add(new Command(str));
    }

    public void WriteToFile(String fileName)
    {
        using (StreamWriter file = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\config\" + fileName))
        {
            foreach (Command c in list)
                file.WriteLine(c.ToString());
        }
    }

    public void Add(Command c)
    {
        list.Add(c);
    }

    public void Send()
    {
        USBConnection.open();
        camCon.WaitForConnection();
        Console.WriteLine("Inizio a mandare!");

        int i=0;
        foreach (Command c in list)
        {
            camCon.WaitForSave();
            if (c.type == Command.Cmdtype.PHOTO)
                camCon.Capture();
            else
                USBConnection.send(c.ToString());
            
            Console.WriteLine(c.ToString());

            if (c.type == Command.Cmdtype.TIME)
                time = c.Value*10;

            if ((c.type == Command.Cmdtype.VISIBLE) || (c.type == Command.Cmdtype.INFRARED) || (c.type == Command.Cmdtype.ULTRAVIOLET) || (c.type == Command.Cmdtype.PHOTO))
            {
                if (i < list.Count-1 && list.ElementAt(i+1).type != Command.Cmdtype.PHOTO)
                {
                    System.Threading.Thread.Sleep(time);
                }
            }
            
            System.Threading.Thread.Sleep(DELAY);

            i++;
        }
        camCon.WaitForSave();
        Console.WriteLine("Finito!");
        USBConnection.close();
    }

    public List<String> ToString()
    {
        List<String> str = new List<String>();
        foreach(Command c in list)
            str.Add(c.ToString());
        return str;
    }
}
