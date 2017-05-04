using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Command> cl;
        List<string> cls=new List<string>();
        List<int> comboList = new List<int>();
        public MainWindow()
        {
            InitializeComponent();
            cl = new List<Command>();
            cls = new List<string>();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBox.Text = Math.Round((slider.Value * 10), 3).ToString() + "ms";
        }

        private void textBox_Initialized(object sender, EventArgs e)
        {
            textBox.Text = Math.Round((slider.Value * 10),3).ToString()+"ms";
        }

        private void comboBox_Initialized(object sender, EventArgs e)
        {
            for(int i = 0; i < 45; i++)
            {
                comboList.Add(i);
            }
            comboBox.ItemsSource = comboList;
        }

       

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            Command cmd;
            if (IRButton.IsChecked==true)
            {
                cmd = new Command(Command.cmdtype.INFRARED, (int)comboBox.SelectedItem);
                cl.Add(cmd);
                cls.Add(cmd.toString());
            }

            if (VISButton.IsChecked == true)
            {
                cmd = new Command(Command.cmdtype.VISIBLE, (int)comboBox.SelectedItem+1);
                cl.Add(cmd);
                cls.Add(cmd.toString());
            }
            if (UVButton.IsChecked == true)
            {
                cmd = new Command(Command.cmdtype.ULTRAVIOLET, (int)comboBox.SelectedItem+1);
                cl.Add(cmd);
                cls.Add(cmd.toString());
            }

            if (slider.Value>0)
            {
                cmd = new Command(Command.cmdtype.TIME, (int)slider.Value+1);
                cl.Add(cmd);
                cls.Add(cmd.toString());
            }
            cmdBox.ItemsSource = cls;
            cmdBox.Items.Refresh();
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            cls = new List<string>();
            cl = new List<Command>();
            cmdBox.ItemsSource = cls;
            cmdBox.Items.Refresh();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            CommandList c=new CommandList(cl);
            c.send();
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
            Command cmd = new Command(Command.cmdtype.PHOTO);
            cl.Add(cmd);
            cls.Add(cmd.toString());
            cmdBox.ItemsSource = cls;
            cmdBox.Items.Refresh();
        }

        private void cmdBox_Initialized(object sender, EventArgs e)
        {
            cmdBox.ItemsSource = cls;
        }
    }
}
