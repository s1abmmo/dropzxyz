using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace main
{
    /// <summary>
    /// Interaction logic for WindowConfigBrowser.xaml
    /// </summary>
    public partial class WindowConfigBrowser : Window
    {
        private int IndexDropzWindow;
        public WindowConfigBrowser(int IndexDropzWindow1)
        {
            IndexDropzWindow = IndexDropzWindow1;
            InitializeComponent();
            this.DataContext = ListItem.ListItem.items[IndexDropzWindow].Setting;
            //MainWindow.items[IndexDropzWindow].Proxytype = ProxyType.None;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void Host_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Port_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Socks5_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListItem.ListItem.items[IndexDropzWindow].Setting.UserAgent = TextBoxUserAgent.Text;
            ListItem.ListItem.items[IndexDropzWindow].Setting.Host = Host.Text;
            ListItem.ListItem.items[IndexDropzWindow].Setting.Port =Convert.ToInt32(Port.Text);
            ListItem.ListItem.items[IndexDropzWindow].Setting.Width = Convert.ToInt32(width.Text);
            ListItem.ListItem.items[IndexDropzWindow].Setting.Height = Convert.ToInt32(height.Text);
            if (None.IsChecked.Value)
            {
                ListItem.ListItem.items[IndexDropzWindow].Setting.Proxytype = ProxyType.none;
            }
            else if (Socks5.IsChecked.Value)
            {
                ListItem.ListItem.items[IndexDropzWindow].Setting.Proxytype = ProxyType.socks5;
            }
            if (hidepopup.IsChecked.Value)
            {
                ListItem.ListItem.items[IndexDropzWindow].Setting.HidePopup = true;
            }
            else
            {
                ListItem.ListItem.items[IndexDropzWindow].Setting.HidePopup = false;
            }
            ListItem.ListItem.items[IndexDropzWindow].Setting.DelayClosePopup = Convert.ToInt32(delayclosepopup.Text);
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Config_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
    public class ComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }
}
