using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace WpfApp1
{
    public partial class CameraWindow : Window
    {
        private readonly Window caller;
        private NikonController con;
        Timer liveViewTimer = new Timer();

        public CameraWindow(Window caller, NikonController con)
        {
            InitializeComponent();

            this.caller = caller;
            this.con = con;

            liveViewTimer.Tick += new EventHandler(liveViewTimer_Tick);
            liveViewTimer.Interval = 1000 / 30;
            con.SetLiveView(true);

            liveViewTimer.Start();
        }

        private void liveViewTimer_Tick(object sender, EventArgs e)
        {
            var image = con.getLiveView();

            if (image != null)
            {
                var stream = new MemoryStream(image.JpegBuffer);
                var img = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.Default);
                LiveViewImage.Source = img.Frames[0];
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            liveViewTimer.Stop();
            con.SetLiveView(false);
            caller.IsEnabled = true;
        }
    }
}
