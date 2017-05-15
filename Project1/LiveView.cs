using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cupola
{
    public partial class LiveView : Form
    {
        private NikonController con;

        Timer liveViewTimer = new Timer();
        public LiveView(NikonController con)
        {
            InitializeComponent();
            pictureBox1.Image = Image.FromFile("lena.jpg");

            this.con = con;
  
            liveViewTimer.Tick += new EventHandler(liveViewTimer_Tick);
            liveViewTimer.Interval = 1000 / 30;
            con.SetLiveView(true);

            liveViewTimer.Start();
        }

        void liveViewTimer_Tick(object sender, EventArgs e)
        {
            var image = con.getLiveView();

            if (image != null)
            {
                MemoryStream stream = new MemoryStream(image.JpegBuffer);
                pictureBox1.Image = Image.FromStream(stream);
            }
        }

        private void LiveView_FormClosing(object sender, FormClosingEventArgs e)
        {
            liveViewTimer.Stop();
            con.SetLiveView(false);
        }
    }
}
