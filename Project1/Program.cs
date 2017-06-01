using System;

//Led 21, 35, 41 IR non funzionano

//iso OK
//exposure time OK
//apertura NO
//white balance (custom)
//focus
//format row .nef OK


public class Program {
    public static void Main()
    {
        var usbCon = new USBConnection("COM3");
        //var camCon = new NikonController("Type0014.md3");
        //camCon.SaveToPc = true;
        var cl = new CommandList();

        //camCon.WaitForConnection();
        /*
        cl.Add(new Command(Command.Cmdtype.TIME, 100));
        cl.Add(new Command(Command.Cmdtype.PHOTO));
        for (int i = 0; i < 45; i++)
        {
            cl.Add(new Command(Command.Cmdtype.INFRARED, i + 1));
            cl.Add(new Command(Command.Cmdtype.PHOTO));
        }
        */

        
        cl.Add(new Command(Command.Cmdtype.TIME, 999));
        cl.Add(new Command(Command.Cmdtype.INFRARED, 41));
        cl.Send(usbCon, null);
        
        cl.Send(usbCon, null);

        Console.ReadLine();
        Environment.Exit(0);
    }
}
