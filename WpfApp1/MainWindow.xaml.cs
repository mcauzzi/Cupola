using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Windows.Forms.Timer;

namespace WpfApp1
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private NikonController camCon;
        private CommandList cl;
        private int time = -1;
        private USBConnection usbCon;
        private Thread sendThread;
        private Timer liveViewTimer;

        public MainWindow()
        {
            InitializeComponent();

            usbCon = new USBConnection("COM3");
            camCon = new NikonController("Type0014.md3");
            cl = new CommandList();
            
            LedTimeSlider.Value = CommandList.DEFAULT_TIME;
            VisButton.IsChecked = true;
        }

        private void LedTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {   
            if(LedTimeBox != null)
               LedTimeBox.Text = Math.Round((LedTimeSlider.Value), 0).ToString();
        }

        private void InitCameraBoxes()
        {
            if (camCon.IsConnected)
            {
                IsoBox.ItemsSource = camCon.IsoList;
                ShutterBox.ItemsSource = camCon.ShutterList;
                //ApertureBox.ItemsSource = camCon.ApertureList;
                //WhiteBox.ItemsSource = camCon.ApertureList;
                CompressionBox.ItemsSource = camCon.CompressionList;
            }
        }

        private void LedTimeBox_Initialized(object sender, EventArgs e)
        {
            LedTimeBox.Text = Math.Round((LedTimeSlider.Value),0).ToString();
        }

        private void LedNumberBox_Initialized(object sender, EventArgs e)
        {
            List<int> comboList = new List<int>();
            for(int i = 1; i <= 45; i++)
            {
                comboList.Add(i);
            }
            LedNumberBox.ItemsSource = comboList;
            LedNumberBox.SelectedIndex = 0;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (time != (int.Parse(LedTimeBox.Text)))
            {
                cl.Add(new Command(Command.Cmdtype.TIME, int.Parse(LedTimeBox.Text) / 10));
                time = int.Parse(LedTimeBox.Text);
            }
            if (IrButton.IsChecked==true)
                cl.Add(new Command(Command.Cmdtype.INFRARED, (int)LedNumberBox.SelectedItem));
            if (VisButton.IsChecked == true)
                cl.Add(new Command(Command.Cmdtype.VISIBLE, (int)LedNumberBox.SelectedItem+1));
            if (UvButton.IsChecked == true)
                cl.Add(new Command(Command.Cmdtype.ULTRAVIOLET, (int)LedNumberBox.SelectedItem+1));

            CmdBox.ItemsSource = cl.ToStringList();
            CmdBox.Items.Refresh();
            LedNumberBox.SelectedIndex = 0;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            time = -1;
            cl = new CommandList();
            CmdBox.ItemsSource = cl.ToStringList();
            CmdBox.Items.Refresh();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (sendThread==null || !sendThread.IsAlive)
            {
                sendThread = new Thread(() => cl.Send(usbCon, camCon));
                sendThread.Start();
            }
        }

        #region LightButtons
        private void VisButton_Unchecked(object sender, RoutedEventArgs e)
        {
            VisButton.IsChecked = false;
        }

        private void UvButton_Unchecked(object sender, RoutedEventArgs e)
        {
            UvButton.IsChecked = false;
        }

        private void IrButton_Unchecked(object sender, RoutedEventArgs e)
        {
            IrButton.IsChecked = false;
        }

        private void UvButton_Checked(object sender, RoutedEventArgs e)
        {
            IrButton.IsChecked = false;
            VisButton.IsChecked = false;
        }

        private void VisButton_Checked(object sender, RoutedEventArgs e)
        {
            UvButton.IsChecked = false;
            IrButton.IsChecked = false;
        }
        private void IrButton_Checked(object sender, RoutedEventArgs e)
        {
            VisButton.IsChecked = false;
            UvButton.IsChecked = false;
        }
        #endregion

        private void AddPhotoClick(object sender, RoutedEventArgs e)
        {
            cl.Add(new Command(Command.Cmdtype.PHOTO));

            CmdBox.ItemsSource = cl.ToStringList();
            CmdBox.Items.Refresh();
            CmdBox.SelectedIndex = 0;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            liveViewTimer?.Stop();
            camCon.SetLiveView(false);
            camCon.Close();
        }

        private void LedTimeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            LedTimeSlider.Value = (int.Parse(LedTimeBox.Text) - int.Parse(LedTimeBox.Text) % 10);
            LedTimeBox.Text = (int.Parse(LedTimeBox.Text) - int.Parse(LedTimeBox.Text) % 10).ToString();
        }

        private void LedTimeBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LedTimeSlider.Value = (int.Parse(LedTimeBox.Text) - int.Parse(LedTimeBox.Text) % 10);
                LedTimeBox.Text = (int.Parse(LedTimeBox.Text) - int.Parse(LedTimeBox.Text) % 10).ToString();
            }
        }

        private void SetCameraButton_Click(object sender, RoutedEventArgs e)
        {
            if (Process.GetProcessesByName("CameraControl").Length != 0)
            {
                MessageBox.Show("Chiudi Digicam! (crea un conflitto)");
                return;
            }
            if (!camCon.IsConnected)
            {
                MessageBox.Show("Collega La camera!");
                return;
            }

            InitCameraBoxes();
            liveViewTimer = new Timer();
            liveViewTimer.Tick += new EventHandler(liveViewTimer_Tick);
            liveViewTimer.Interval = 1000 / 30;
            camCon.SetLiveView(true);
            liveViewTimer.Start();
            CameraGroup.IsEnabled = true;
        }

        private void liveViewTimer_Tick(object sender, EventArgs e)
        {
            var image = camCon.getLiveView();

            if (image != null)
            {
                var stream = new MemoryStream(image.JpegBuffer);
                var img = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.Default);
                LiveViewImage.Source = img.Frames[0];
                /*
                LiveViewImage.Width = img.Frames[0].Width;
                LiveViewImage.Height = img.Frames[0].Height;
                LiveViewBorder.Height = img.Frames[0].Height;
                LiveViewBorder.Width = img.Frames[0].Width;
                LiveViewBorder.Margin = LiveViewImage.Margin;
                */
            }
        }

        private void DigiCam_Click(object sender, RoutedEventArgs e)
        {
            camCon.Close();
            var p = new Process {StartInfo = {FileName = @"C:\Program Files (x86)\digiCamControl\CameraControl.exe" } };
            p.EnableRaisingEvents = true;

            p.Exited += (senderP, a) =>
            {
                camCon = new NikonController("Type0014.md3");
               // cl = new CommandList(cl, camCon);
                camCon.WaitForConnection();
            };

            p.Start();
        }
    }
}
