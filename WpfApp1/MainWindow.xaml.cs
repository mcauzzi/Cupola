using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CommandList cl;
        private int time=-1;
        private NikonController camCon = new NikonController(true);

        public MainWindow()
        {
            InitializeComponent();

            cl = new CommandList(camCon);
            USBConnection.Init();
            slider.Value = CommandList.DEFAULT_TIME;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {   
            if(textBox!=null)
               textBox.Text = Math.Round((slider.Value), 0).ToString();
        }

        private void textBox_Initialized(object sender, EventArgs e)
        {
            textBox.Text = Math.Round((slider.Value),0).ToString();
        }

        private void comboBox_Initialized(object sender, EventArgs e)
        {
            List<int> comboList = new List<int>();
            for(int i = 1; i <= 45; i++)
            {
                comboList.Add(i);
            }
            comboBox.ItemsSource = comboList;
            comboBox.SelectedIndex = 0;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (time != (int.Parse(textBox.Text)))
            {
                cl.Add(new Command(Command.Cmdtype.TIME, int.Parse(textBox.Text) / 10));
                time = int.Parse(textBox.Text);
            }
            if (IRButton.IsChecked==true)
                cl.Add(new Command(Command.Cmdtype.INFRARED, (int)comboBox.SelectedItem));
            if (VISButton.IsChecked == true)
                cl.Add(new Command(Command.Cmdtype.VISIBLE, (int)comboBox.SelectedItem+1));
            if (UVButton.IsChecked == true)
                cl.Add(new Command(Command.Cmdtype.ULTRAVIOLET, (int)comboBox.SelectedItem+1));

            cmdBox.ItemsSource = cl.ToStringList();
            cmdBox.Items.Refresh();
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            cl = new CommandList(camCon);
            cmdBox.ItemsSource = cl.ToStringList();
            cmdBox.Items.Refresh();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            cl.Send();
        }
        #region LightButtons
        private void VISButton_Unchecked(object sender, RoutedEventArgs e)
        {
            VISButton.IsChecked = false;
        }

        private void UVButton_Unchecked(object sender, RoutedEventArgs e)
        {
            UVButton.IsChecked = false;
        }

        private void IRButton_Unchecked(object sender, RoutedEventArgs e)
        {
            IRButton.IsChecked = false;
        }

        private void UVButton_Checked(object sender, RoutedEventArgs e)
        {
            IRButton.IsChecked = false;
            VISButton.IsChecked = false;
        }

        private void VISButton_Checked(object sender, RoutedEventArgs e)
        {
            UVButton.IsChecked = false;
            IRButton.IsChecked = false;
        }
        private void IRButton_Checked(object sender, RoutedEventArgs e)
        {
            VISButton.IsChecked = false;
            UVButton.IsChecked = false;
        }
        #endregion

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Command cmd = new Command(Command.Cmdtype.PHOTO);
            cl.Add(cmd);
            cmdBox.ItemsSource = cl.ToStringList();
            cmdBox.Items.Refresh();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            camCon.Close();
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            slider.Value = (int.Parse(textBox.Text) - int.Parse(textBox.Text) % 10);
            textBox.Text = (int.Parse(textBox.Text) - int.Parse(textBox.Text) % 10).ToString();
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                slider.Value = (int.Parse(textBox.Text) - int.Parse(textBox.Text) % 10);
                textBox.Text = (int.Parse(textBox.Text) - int.Parse(textBox.Text) % 10).ToString();
            }
        }
    }
}
