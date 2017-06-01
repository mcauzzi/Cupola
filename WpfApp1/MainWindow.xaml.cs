using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Cupola;
using Nikon;
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
        private readonly USBConnection usbCon;
        private readonly NikonController camCon;
        private CommandList cl;

        private int time = -1;
        private Thread sendThread;
        private Timer liveViewTimer;
        private BitmapSource placeholder;

        public MainWindow()
        {
            usbCon = new USBConnection("COM3");
            camCon = new NikonController("Type0014.md3");
            cl = new CommandList();

            InitializeComponent();
            
            camCon.Received += OpenReceivedWindow;
            camCon.Connected += OnCameraConnected;
        }

        private void OpenReceivedWindow(NikonImage img)
        {
            
        }

        private void OnCameraConnected()
        {
            if (Process.GetProcessesByName("CameraControl").Length != 0)
            {
                MessageBox.Show("Chiudi Digicam! (crea un conflitto)");
                return;
            }

            InitCameraBoxes();
            liveViewTimer = new Timer {Interval = 1000 / 30};
            liveViewTimer.Start();
        }

        private void InitCameraBoxes()
        {
            if (camCon.IsConnected)
            {
                IsoBox.ItemsSource = camCon.GetCapabilityStringList(NikonController.Capability.Iso);
                IsoBox.SelectedIndex = camCon.GetCapabilityIndex(NikonController.Capability.Iso);
                ShutterBox.ItemsSource = camCon.GetCapabilityStringList(NikonController.Capability.ShutterSpeed);
                ShutterBox.SelectedIndex = camCon.GetCapabilityIndex(NikonController.Capability.ShutterSpeed);
                ApertureBox.ItemsSource = camCon.GetCapabilityStringList(NikonController.Capability.Aperture);
                ApertureBox.SelectedIndex = camCon.GetCapabilityIndex(NikonController.Capability.Aperture);
                WhiteBox.ItemsSource = camCon.GetCapabilityStringList(NikonController.Capability.WhiteBalance);
                WhiteBox.SelectedIndex = camCon.GetCapabilityIndex(NikonController.Capability.WhiteBalance);
                CompressionBox.ItemsSource = camCon.GetCapabilityStringList(NikonController.Capability.Compression);
                CompressionBox.SelectedIndex = camCon.GetCapabilityIndex(NikonController.Capability.Compression);
            }
        }

        private void LedTimeBox_Initialized(object sender, EventArgs e)
        {
            LedTimeBox.Text = GetMsFromShutterSpeed().ToString();
        }

        private void LedNumberBox_Initialized(object sender, EventArgs e)
        {
            var comboList = new List<int>();
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
                cl.Add(new Command(Command.Cmdtype.VISIBLE, (int)LedNumberBox.SelectedItem));
            if (UvButton.IsChecked == true)
                cl.Add(new Command(Command.Cmdtype.ULTRAVIOLET, (int)LedNumberBox.SelectedItem));

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
            if (sendThread == null || !sendThread.IsAlive)
            {
                var clTemp = new CommandList();
                if (AddLedBox.SelectedItem.Equals("All"))
                {
                    clTemp.Add(new Command(Command.Cmdtype.TIME, GetMsFromShutterSpeed()/10));
                    clTemp.Add(new Command(Command.Cmdtype.PHOTO));
                    for (int i = 0; i < 45; i++)
                    {
                        clTemp.Add(new Command(Command.Cmdtype.VISIBLE, i + 1));
                        clTemp.Add(new Command(Command.Cmdtype.PHOTO));
                    }
                    for (int i = 0; i < 45; i++)
                    {
                        clTemp.Add(new Command(Command.Cmdtype.INFRARED, i + 1));
                        clTemp.Add(new Command(Command.Cmdtype.PHOTO));
                    }
                    for (int i = 0; i < 45; i++)
                    {
                        clTemp.Add(new Command(Command.Cmdtype.ULTRAVIOLET, i + 1));
                        clTemp.Add(new Command(Command.Cmdtype.PHOTO));
                    }
                }
                else if (AddLedBox.SelectedItem.Equals("All type"))
                {
                    Command.Cmdtype type;

                    if (VisButton.IsChecked == true) { type = Command.Cmdtype.VISIBLE; }
                    else if (IrButton.IsChecked == true) { type = Command.Cmdtype.INFRARED; }
                    else { type = Command.Cmdtype.ULTRAVIOLET; }

                    clTemp.Add(new Command(Command.Cmdtype.TIME, GetMsFromShutterSpeed()/10));
                    clTemp.Add(new Command(Command.Cmdtype.PHOTO));
                    for (int i = 0; i < 45; i++)
                    {
                        clTemp.Add(new Command(type, i + 1));
                        clTemp.Add(new Command(Command.Cmdtype.PHOTO));
                    }
                }
                else
                {
                    clTemp = cl;
                }

                sendThread = new Thread(() => clTemp.Send(usbCon, camCon));
                sendThread.Start();
            }
        }

        private int GetMsFromShutterSpeed()
        {
            int spIndex;
            try
            {
                spIndex = camCon.GetCapabilityIndex(NikonController.Capability.ShutterSpeed);
            }
            catch (Exception)
            {
                return 1000;
            }

            var spString = ShutterBox.Items.GetItemAt(spIndex);
            int ms;

            try
            {
                ms = int.Parse(spString.ToString()) * 1000 + 500;
            }
            catch (Exception)
            {
                ms = 1000;
            }

            return ms;
        }

        #region LightButtons
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

        private void LedTimeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            LedTimeBox.Text = (int.Parse(LedTimeBox.Text) - int.Parse(LedTimeBox.Text) % 10).ToString();
        }

        private void LedTimeBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LedTimeBox.Text = (int.Parse(LedTimeBox.Text) - int.Parse(LedTimeBox.Text) % 10).ToString();
            }
        }

        private void liveViewTimer_Tick(object sender, EventArgs e)
        {
            var image = camCon.GetLiveView();

            if (image != null)
            {
                var stream = new MemoryStream(image.JpegBuffer);
                var img = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.Default);
                LiveViewImage.Source = img.Frames[0];
            }
        }

        private void IsoBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            camCon.SetCapabilityIndex(NikonController.Capability.Iso, IsoBox.SelectedIndex);
        }

        private void ShutterBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            camCon.SetCapabilityIndex(NikonController.Capability.ShutterSpeed, ShutterBox.SelectedIndex);
        }

        private void ApertureBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            camCon.SetCapabilityIndex(NikonController.Capability.Aperture, ApertureBox.SelectedIndex);
        }

        private void WhiteBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            camCon.SetCapabilityIndex(NikonController.Capability.WhiteBalance, WhiteBox.SelectedIndex);
        }

        private void CompressionBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            camCon.SetCapabilityIndex(NikonController.Capability.Compression, CompressionBox.SelectedIndex);
        }

        private void ToggleLiveViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleLiveViewButton.Content.Equals("Enable Live View"))
            {
                ToggleLiveViewButton.Content = "Disable Live View";
                camCon.SetLiveView(true);
                liveViewTimer.Tick += liveViewTimer_Tick ;
            }
            else
            {
                ToggleLiveViewButton.Content = "Enable Live View";
                camCon.SetLiveView(false);
                liveViewTimer.Tick -= liveViewTimer_Tick;
                LiveViewImage.Source = placeholder;
            }
        }

        private void PhotoTestButton_Click(object sender, RoutedEventArgs e)
        {
            var clTest = new CommandList();
            clTest.Add(new Command(Command.Cmdtype.TIME, 400));
            if (VisButton.IsChecked == true) { clTest.Add(new Command(Command.Cmdtype.VISIBLE, 45)); }
            else if (IrButton.IsChecked == true) { clTest.Add(new Command(Command.Cmdtype.INFRARED, 45)); }
            else { clTest.Add(new Command(Command.Cmdtype.ULTRAVIOLET, 45)); }
            clTest.Add(new Command(Command.Cmdtype.PHOTO));
            if (sendThread == null || !sendThread.IsAlive)
            {
                sendThread = new Thread(() => clTest.Send(usbCon, camCon));
                sendThread.Start();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            liveViewTimer?.Stop();
            if (camCon != null && camCon.IsConnected)
                camCon.SetLiveView(false);
            camCon?.Close();
        }

        private void AddLedBox_Initialized(object sender, EventArgs e)
        {
            AddLedBox.ItemsSource = new List<string>()
            {
                "All",
                "All type",
                "Custom"
            };
            AddLedBox.SelectedIndex = 0;
        }

        private void LiveViewImage_Initialized(object sender, EventArgs e)
        {
            var stream = new FileStream("placeholder.jpg", FileMode.Open);
            var img = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.Default);
            placeholder = img.Frames[0];

            LiveViewImage.Source = placeholder;
        }

        private void AddLedBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AddLedBox.SelectedItem.Equals("All"))
            {
                LightTypeGrid.Visibility = Visibility.Collapsed;
                CustomGrid.Visibility = Visibility.Collapsed;
            }
            else if (AddLedBox.SelectedItem.Equals("All type"))
            {
                LightTypeGrid.Visibility = Visibility.Visible;
                CustomGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                LightTypeGrid.Visibility = Visibility.Visible;
                CustomGrid.Visibility = Visibility.Visible;
            }
        }

        private void LightTypeGrid_Initialized(object sender, EventArgs e)
        {
            LightTypeGrid.Visibility = Visibility.Collapsed;
        }

        private void CustomGrid_Initialized(object sender, EventArgs e)
        {
            CustomGrid.Visibility = Visibility.Collapsed;
        }

        private void SaveCheck_Initialized(object sender, EventArgs e)
        {
            SaveCheck.IsChecked = camCon.SaveToPc;
        }

        private void SaveCheck_Checked(object sender, RoutedEventArgs e)
        {
            camCon.SaveToPc = SaveCheck.IsChecked ?? true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (sendThread != null && sendThread.IsAlive)
            {
                CommandList.ShouldClose = true;
            }
        }
    }
}
