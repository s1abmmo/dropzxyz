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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using main.Config;

namespace main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            captchasolverstatus = new CaptchaSolverStatus();
            main.Config.Config.LoadConfigs();
            lvUsers.ItemsSource =ListItem.ListItem.items;
            lvUsers.Items.Refresh();

            main.Config.Config.LoadListScript();
            this.DataContext = null;
            this.DataContext = main.Config.Config.config;
        }
        public static CaptchaSolverStatus captchasolverstatus;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int a = 1; ; a++)
            {
                if (ListItem.ListItem.items.Find(r => r.Id == a)==null)
                {
                    if (main.Config.Config.config.ScriptNameSelected != null && main.Config.Config.config.ScriptNameSelected != "")
                    {
                        DropzWindow newprofile = new DropzWindow();
                        newprofile.Id = a;
                        newprofile.Description = a.ToString();
                        newprofile.Start = false;
                        newprofile.Status = "---";
                        newprofile.AutoRunning = false;
                        newprofile.ButtonReady = true;
                        newprofile.ScriptParameters = new object();

                        Setting newsetting = new Setting();
                        newsetting.Proxytype = ProxyType.none;
                        newsetting.Host = "";
                        newsetting.Port = 0;
                        newsetting.HidePopup = true;
                        newsetting.Width = 1000;
                        newsetting.Height = 500;
                        newsetting.UserAgent = "";
                        newsetting.Visible = false;
                        newsetting.DelayClosePopup = 5;
                        newprofile.Setting = newsetting;

                        AutoScript autoscript = new AutoScript();
                        autoscript.Script = main.Config.Config.config.ListScript[main.Config.Config.config.ScriptNameSelected];
                        autoscript.ScriptName = main.Config.Config.config.ScriptNameSelected;
                        autoscript.ScriptType = ScriptType.claimtoolcommand;
                        newprofile.Script = autoscript;

                        ListItem.ListItem.items.Add(newprofile);
                        lvUsers.Items.Refresh();
                    }
                    break;
                }
            }
        }

        private void StartAndStop(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            string Uid = s.Uid;
            int IndexDropzWindow = ListItem.ListItem.items.FindIndex(x => x.Id == Convert.ToInt32(Uid));
            ListItem.ListItem.items[IndexDropzWindow].ButtonReady = false;
            RefreshListView();

            if (!ListItem.ListItem.items[IndexDropzWindow].Start)
            {
                Thread mainThread = new Thread(() =>
                {
                    Process p = new Process();
                    p.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + "/dropzwindow.exe";
                    string Arguments = Uid + " ";
                    Arguments += Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonConverting.JsonConverting.EncodeJson(ListItem.ListItem.items[IndexDropzWindow].ScriptParameters))) + " ";
                    Arguments += Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonConverting.JsonConverting.EncodeJson(ListItem.ListItem.items[IndexDropzWindow].Script))) + " ";
                    Arguments += Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonConverting.JsonConverting.EncodeJson(ListItem.ListItem.items[IndexDropzWindow].Setting))) + " ";
                    p.StartInfo.Arguments = Arguments;
                    if (!ListItem.ListItem.items[IndexDropzWindow].Setting.Visible)
                        p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.Start();
                    ListItem.ListItem.items[IndexDropzWindow].IdProcess = p.Id;
                    NamedPipeServerStream ppsv = new NamedPipeServerStream("dropzwindow" + Uid, PipeDirection.InOut);
                    ppsv.WaitForConnection();

                    ListItem.ListItem.items[IndexDropzWindow].Hwnd= new IntPtr((int)Convert.ToUInt64(new Pipe.Pipe().SendAndReceive(ppsv, "Start")));
                    if (ListItem.ListItem.items[IndexDropzWindow].Hwnd!= IntPtr.Zero)
                    {
                        ListItem.ListItem.items[IndexDropzWindow].Start = true;
                        ListItem.ListItem.items[IndexDropzWindow].ButtonReady = true;
                        RefreshListView();
                        while (ListItem.ListItem.items[IndexDropzWindow].Start)
                        {
                            Thread.Sleep(250);

                            if (ListItem.ListItem.items[IndexDropzWindow].CommandSend == null && ListItem.ListItem.items[IndexDropzWindow].AutoRunning == true)
                            {
                                ListItem.ListItem.items[IndexDropzWindow].CommandSend = "CheckData";
                            }


                            if (ListItem.ListItem.items[IndexDropzWindow].CommandSend != null)
                            {

                                ListItem.ListItem.items[IndexDropzWindow].DataReceived= new Pipe.Pipe().SendAndReceive(ppsv, ListItem.ListItem.items[IndexDropzWindow].CommandSend);

                                if (ListItem.ListItem.items[IndexDropzWindow].CommandSend == "Auto")
                                {
                                    if (ListItem.ListItem.items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        ListItem.ListItem.items[IndexDropzWindow].ButtonReady = true;
                                        ListItem.ListItem.items[IndexDropzWindow].AutoRunning = true;
                                        RefreshListView();
                                    }
                                }
                                else if (ListItem.ListItem.items[IndexDropzWindow].CommandSend == "StopAuto")
                                {
                                    if (ListItem.ListItem.items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        ListItem.ListItem.items[IndexDropzWindow].ButtonReady = true;
                                        ListItem.ListItem.items[IndexDropzWindow].AutoRunning = false;
                                        RefreshListView();
                                    }
                                }
                                else if (ListItem.ListItem.items[IndexDropzWindow].CommandSend == "CheckData")
                                {
                                    try
                                    {
                                        string[] result = ListItem.ListItem.items[IndexDropzWindow].DataReceived.Split('|');
                                        ListItem.ListItem.items[IndexDropzWindow].Response = result[0];
                                        if (Convert.ToBoolean(result[1]) == true)
                                        {
                                            ListItem.ListItem.items[IndexDropzWindow].Captcha = true;
                                            ListItem.ListItem.items[IndexDropzWindow].CaptchaTime = DateTime.Now;
                                        }
                                        else { ListItem.ListItem.items[IndexDropzWindow].Captcha = false; }
                                        ListItem.ListItem.items[IndexDropzWindow].ButtonReady = true;
                                        RefreshListView();
                                    }
                                    catch { }
                                }else if(ListItem.ListItem.items[IndexDropzWindow].CommandSend == "Stop")
                                {
                                    //MessageBox.Show("stop");
                                    if (ListItem.ListItem.items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        ListItem.ListItem.items[IndexDropzWindow].ButtonReady = true;
                                        try
                                        {
                                            Process.GetProcessById(ListItem.ListItem.items[IndexDropzWindow].IdProcess).Kill();
                                            Process.GetProcessById(ListItem.ListItem.items[IndexDropzWindow].IdProcess).Dispose();
                                        }
                                        catch { }
                                        new Pipe.Pipe().Disconnect(ppsv);
                                        ListItem.ListItem.items[IndexDropzWindow].Start = false;
                                        ListItem.ListItem.items[IndexDropzWindow].AutoRunning = false;
                                        ListItem.ListItem.items[IndexDropzWindow].Setting.Visible = false;
                                        ListItem.ListItem.items[IndexDropzWindow].ButtonReady = true;
                                        RefreshListView();
                                    }
                                }
                                else if (ListItem.ListItem.items[IndexDropzWindow].CommandSend == "Hide")
                                {
                                    //MessageBox.Show("stop");
                                    if (ListItem.ListItem.items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        ListItem.ListItem.items[IndexDropzWindow].ButtonReady = true;
                                        ListItem.ListItem.items[IndexDropzWindow].Setting.Visible = false;
                                        RefreshListView();
                                    }
                                }
                                else if (ListItem.ListItem.items[IndexDropzWindow].CommandSend == "Show")
                                {
                                    //MessageBox.Show("stop");
                                    if (ListItem.ListItem.items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        ListItem.ListItem.items[IndexDropzWindow].ButtonReady = true;
                                        ListItem.ListItem.items[IndexDropzWindow].Setting.Visible = true;
                                        RefreshListView();
                                    }
                                }

                                ListItem.ListItem.items[IndexDropzWindow].CommandSend = null;

                            }
                        }
                    }

                });
                mainThread.Start();
            }
            else
            {
                ListItem.ListItem.items[IndexDropzWindow].CommandSend = "Stop";
            }
        }
        private void ShowClose(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            string Uid = s.Uid;
            int IndexDropzWindow = ListItem.ListItem.items.FindIndex(x => x.Id == Convert.ToInt32(Uid));
            ListItem.ListItem.items[IndexDropzWindow].ButtonReady = false;
            if (ListItem.ListItem.items[IndexDropzWindow].Setting.Visible)
            {
                ListItem.ListItem.items[IndexDropzWindow].CommandSend = "Hide";
            }
            else
            {
                ListItem.ListItem.items[IndexDropzWindow].CommandSend = "Show";
            }
        }
        private void Auto(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            string Uid = s.Uid;
            int IndexDropzWindow = ListItem.ListItem.items.FindIndex(x => x.Id == Convert.ToInt32(Uid));
            ListItem.ListItem.items[IndexDropzWindow].ButtonReady = false;
            if (!ListItem.ListItem.items[IndexDropzWindow].AutoRunning)
            {
                ListItem.ListItem.items[IndexDropzWindow].CommandSend = "Auto";
            }
            else
            {
                ListItem.ListItem.items[IndexDropzWindow].CommandSend = "StopAuto";
            }
        }
        private void RefreshListView()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lvUsers.Items.Refresh();
            }));
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            main.Config.Config.SaveConfigs();
            Environment.Exit(0);
        }
        private void Delete(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            string Uid = s.Uid;
            int IndexDropzWindow = ListItem.ListItem.items.FindIndex(x => x.Id == Convert.ToInt32(Uid));
            if (!ListItem.ListItem.items[IndexDropzWindow].Start)
            {
                try
                {
                    Directory.Delete(System.AppDomain.CurrentDomain.BaseDirectory + "//CacheCef//" + Uid.ToString(), true);
                }
                catch { }
                ListItem.ListItem.items.Remove(ListItem.ListItem.items[IndexDropzWindow]);
                RefreshListView();
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Tính năng chưa có");
            CaptchaSolver window = new CaptchaSolver();
            window.Show();
        }
        private void Config(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            string Uid = s.Uid;
            int IndexDropzWindow = ListItem.ListItem.items.FindIndex(x => x.Id == Convert.ToInt32(Uid));
            WindowConfigBrowser window = new WindowConfigBrowser(IndexDropzWindow);
            window.Show();
        }
        private void About(object sender, RoutedEventArgs e)
        {
            About window = new About();
            window.Show();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();

        }
        private void Exit(object sender, RoutedEventArgs e)
        {
            main.Config.Config.SaveConfigs();
            Environment.Exit(0);
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string str = JsonConverting.JsonConverting.PrettyJson(tb.Text);
            //MessageBox.Show(str);
            if (str == null || tb.Text == null || tb.Text == "")
                return;
            tb.Text = str;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }
    }
    public class DropzWindow
    {
        public int Id { get; set; }
        public string Description { get; set; }
        //public string Script { get; set; }
        public string Status { get; set; }
        public bool AutoRunning { get; set; }
        public string Response { get; set; }
        public string Action { get; set; }
        public bool Start { get; set; }
        public int IdProcess { get; set; }
        public string CommandSend { get; set; }
        public string DataReceived { get; set; }
        public bool ButtonReady { get; set; }
        public IntPtr Hwnd { get; set; }
        //public dynamic ProfileConfig { get; set; }
        public bool Captcha { get; set; }
        public DateTime CaptchaTime { get; set; }
        public Setting Setting { get; set; }
        public AutoScript Script { get; set; }
        public object ScriptParameters { get; set; }
    }
}
