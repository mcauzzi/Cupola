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
        USBConnection.Open();
        camCon.WaitForConnection();
        Console.WriteLine("Inizio a mandare!");

        int i=0;
        foreach (Command c in list)
        {
            camCon.WaitForSave();
            if (c.Type == Command.Cmdtype.PHOTO)
                camCon.Capture();
            else
                USBConnection.Send(c.ToString());
            
            Console.WriteLine(c.ToString());

            if (c.Type == Command.Cmdtype.TIME)
                time = c.Value*10;

            if ((c.Type == Command.Cmdtype.VISIBLE) || (c.Type == Command.Cmdtype.INFRARED) || (c.Type == Command.Cmdtype.ULTRAVIOLET) || (c.Type == Command.Cmdtype.PHOTO))
            {
                if (i < list.Count-1 && list.ElementAt(i+1).Type != Command.Cmdtype.PHOTO)
                {
                    System.Threading.Thread.Sleep(time);
                }
            }
            
            System.Threading.Thread.Sleep(DELAY);

            i++;
        }
        camCon.WaitForSave();
        Console.WriteLine("Finito!");
        USBConnection.Close();
    }

    public List<String> ToStringList()
    {
        List<String> str = new List<String>();
        foreach(Command c in list)
            str.Add(c.ToString());
        return str;
    }
}
