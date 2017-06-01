using System;
using System.Threading;
using System.Windows;
using Cupola;

namespace AiutoDebugUI
{
    public partial class MainWindow
    {
        private CommandList cl;
        private NikonController camCon;
        private USBConnection usbCon;
        private Thread thread;
        public MainWindow()
        {
            InitializeComponent();

            usbCon = new USBConnection("COM3");
            camCon = new NikonController("Type0014.md3");
            camCon.SaveToPc = true;
            cl = new CommandList();
            cl.Add(new Command(Command.Cmdtype.PHOTO));
        }

        private async void SendList_Click(object sender, RoutedEventArgs e)
        {
            thread = new Thread(() => cl.Send(usbCon, camCon));
            if (!thread.IsAlive)
                thread.Start();
        }
    }
}
