using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Cupola;
using Nikon;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;
using Timer = System.Windows.Forms.Timer;

//TODO: Add progressBar event in comlist

namespace WpfApp1
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly USBConnection usbCon;
        private readonly NikonController camCon;

        private Thread sendThread;
        private Timer liveViewTimer;
        private BitmapSource placeholder;
        private Point afCoord;

        public MainWindow()
        {
            usbCon = new USBConnection("COM3");
            camCon = new NikonController("Type0014.md3");

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
                MessageBox.Show("Close Digicam!");
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

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (sendThread == null || !sendThread.IsAlive)
            {
                var clTemp = new CommandList();

                clTemp.Add(new Command(Command.Cmdtype.TIME, GetMsFromShutterSpeed() / 10));
                clTemp.Add(new Command(Command.Cmdtype.PHOTO));
                if (AddLedBox.SelectedItem.Equals("All"))
                {
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
                    var type = GetSelectedType();
                    for (int i = 0; i < 45; i++)
                    {
                        clTemp.Add(new Command(type, i + 1));
                        clTemp.Add(new Command(Command.Cmdtype.PHOTO));
                    }
                }
                else
                {
                    try
                    {
                        clTemp = String2CommandList(LedNumberRangeBox.Text, GetSelectedType());
                    } catch(ArgumentException)
                    {
                        MessageBox.Show("Wrong string!");
                    }
                }

                sendThread = new Thread(() => clTemp.Send(usbCon, camCon));
                sendThread.Start();
            }
        }

        private CommandList String2CommandList(string str, Command.Cmdtype type)
        {
            var cl = new CommandList();
            str = str.Replace(" ", "");

            var strList = str.Split(';');
            foreach (var s in strList)
            {
                if (s.Contains("-"))
                {
                    var sRange = s.Split('-');
                    int left, right;
                    try
                    {
                        left = int.Parse(sRange[0]);
                        right = int.Parse(sRange[1]);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException(str + "is not a valid string", nameof(str));
                    }
                    if (left < 1 || left > Command.MAXLED || right < 1 || right > Command.MAXLED || left > right)
                    {
                        throw new ArgumentException(str + "is not a valid string", nameof(str));
                    }

                    for (int i = left; i <= right; i++)
                    {
                        cl.Add(new Command(type, i));
                        cl.Add(new Command(Command.Cmdtype.PHOTO));
                    }
                }
                else
                {
                    int num;
                    try
                    {
                        num = int.Parse(s);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException(str + "is not a valid string", nameof(str));
                    }
                    if (num < 1 || num > Command.MAXLED)
                    {
                        throw new ArgumentException(str + "is not a valid string", nameof(str));
                    }

                    cl.Add(new Command(type, num));
                    cl.Add(new Command(Command.Cmdtype.PHOTO));
                }
            }

            return cl;
        }

        private Command.Cmdtype GetSelectedType()
        {
            if (VisButton.IsChecked == true) return Command.Cmdtype.VISIBLE;
            if (IrButton.IsChecked == true) return Command.Cmdtype.INFRARED;
            return Command.Cmdtype.ULTRAVIOLET;
        }

        private int GetMs()
        {
            return AutoTimeCheck.IsChecked == true ? GetMsFromShutterSpeed() : int.Parse(LedTimeBox.Text);
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
                ms = (int)(double.Parse(spString.ToString(), CultureInfo.InvariantCulture) * 1000 + 200);
            }
            catch (Exception)
            {
                ms = 1000;
            }

            return Math.Max(0, Math.Min(Command.MAXTIME*10, ms));
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

        private void LedTimeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int time;
            try
            {
                time = int.Parse(LedTimeBox.Text);
            }
            catch (OverflowException)
            {
                time = Command.MAXTIME * 10;
            }
            LedTimeBox.Text = Math.Min(Command.MAXTIME*10, (time - time % 10)).ToString();
            LedTimeBox.Text = Math.Max(0, int.Parse(LedTimeBox.Text)).ToString();
        }

        private void LedTimeBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LedTimeBox_LostFocus(sender, e);
            }
        }

        private void LiveViewTimer_Tick(object sender, EventArgs e)
        {
            if (!camCon.IsConnected)
                return;

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
            LedTimeBox.Text = GetMsFromShutterSpeed().ToString();
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
                if (!camCon.IsConnected) return;
                ToggleLiveViewButton.Content = "Disable Live View";
                camCon.SetLiveView(true);
                liveViewTimer.Tick += LiveViewTimer_Tick ;
            }
            else
            {
                ToggleLiveViewButton.Content = "Enable Live View";
                camCon.SetLiveView(false);
                liveViewTimer.Tick -= LiveViewTimer_Tick;
                LiveViewImage.Source = placeholder;
            }
        }

        private void PhotoTestButton_Click(object sender, RoutedEventArgs e)
        {
            var clTest = new CommandList();
            clTest.Add(new Command(Command.Cmdtype.TIME, Math.Min(GetMsFromShutterSpeed()/10, Command.MAXTIME)));
            clTest.Add(new Command(GetSelectedType(), 45));
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

        private void AutoTimeCheck_Checked(object sender, RoutedEventArgs e)
        {
            LedTimeBox.IsEnabled = false;
            LedTimeBox.Text = GetMsFromShutterSpeed().ToString();
        }

        private void AutoTimeCheck_Initialized(object sender, EventArgs e)
        {
            AutoTimeCheck.IsChecked = true;
        }

        private void AutoTimeCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            LedTimeBox.IsEnabled = true;
        }

        private void AutoFocusButton_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleLiveViewButton.Content.Equals("Enable Live View"))
            {
                MessageBox.Show("Enable the Live View!");
                return;
            }

            camCon.SetContrastAfArea(afCoord);
            camCon.ContrastAf();
        }

        private void LiveViewImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ToggleLiveViewButton.Content.Equals("Enable Live View"))
            {
                MessageBox.Show("Enable the Live View!");
                return;
            }

            afCoord = e.GetPosition(LiveViewImage);
            const int radius = 30;
            afCoord.X = Math.Min(LiveViewImage.Width - radius - 1, Math.Max(1, afCoord.X - radius / 2));
            afCoord.Y = Math.Min(LiveViewImage.Height - radius - 1, Math.Max(1, afCoord.Y - radius / 2));

            LiveViewCanvas.Children.Clear();
            var ellipse = new Ellipse
            {
                Stroke = new SolidColorBrush(Colors.LawnGreen),
                Width = radius,
                Height = radius,
                StrokeThickness = 2,
                Margin = new Thickness(afCoord.X, afCoord.Y, 0, 0)
            };
            LiveViewCanvas.Children.Add(ellipse);
            MessageBox.Show(afCoord.ToString());
        }

        private void SaveLocationBox_Initialized(object sender, EventArgs e)
        {
            SaveLocationBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+"\\DomeDriver";
            SaveLocationBox.IsEnabled = false;
            camCon.SaveLocation = SaveLocationBox.Text;
        }

        private void SaveLocationButton_Initialized(object sender, EventArgs e)
        {
        }

        private void SaveLocationButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new FolderBrowserDialog();
            saveFileDialog.ShowDialog();
            SaveLocationBox.Text = saveFileDialog.SelectedPath;
            camCon.SaveLocation = saveFileDialog.SelectedPath;
        }
    }
}
