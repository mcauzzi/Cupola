using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class CommandList
{
    private List<Command> list;

    public CommandList()
    {
        this.list = new List<Command>();
    }

    public CommandList(List<Command> list)
    {
        this.list = new List<Command>(list);
    }

    public void add(Command c)
    {
        list.Add(c);
    }

    public void send()
    {
        foreach (Command c in list)
        {
            USBConnection.send(c.toString());
            System.Threading.Thread.Sleep(1050);
        }
    }
}
