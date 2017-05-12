using System;
//Led 21, 35, 42 IR non funzionano

public class Cupola{
    public static void Main() {
        USBConnection.init();
        NikonController camCon = new NikonController();
        CommandList cl = new CommandList(camCon);

        cl.Add(new Command(Command.Cmdtype.TIME, 50));

        for (int i = 0; i < 5; i++)
        {
            cl.Add(new Command(Command.Cmdtype.VISIBLE, i + 1));
            cl.Add(new Command(Command.Cmdtype.PHOTO));
        }

        cl.Send();
        //camCon.WaitForConnection();
        //camCon.getCapabilities();
        Console.ReadLine();
        Environment.Exit(0);
    }
}