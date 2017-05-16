using System;
using System.Windows.Forms;
using Cupola;

//Led 21, 35, 42 IR non funzionano

//iso OK
//exposure time OK
//apertura NO
//white balance (custom)
//focus
//format row .nef OK


public class Program {
    [STAThread]
    public static void Main() {
        USBConnection.Init();
        var camCon = new NikonController(true);
        var cl = new CommandList(camCon);
        Application.EnableVisualStyles();
        camCon.WaitForConnection();
        camCon.SetParams();
        //Application.Run(new LiveView(camCon));

        
        cl.Add(new Command(Command.Cmdtype.TIME, 100));

        for (int i = 0; i < 5; i++)
        {
            cl.Add(new Command(Command.Cmdtype.VISIBLE, i + 1));
            cl.Add(new Command(Command.Cmdtype.PHOTO));
        }

        cl.Send();

        Console.ReadLine();
        Environment.Exit(0);
    }
}
