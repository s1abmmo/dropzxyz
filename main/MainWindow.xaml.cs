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
            LoadConfigs();
            lvUsers.ItemsSource = items;
            lvUsers.Items.Refresh();
            DirectoryInfo di = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory + "\\Auto");
            bool add = false;
            foreach (FileInfo file in di.GetFiles("*"))
            {
                ComboBox1.Items.Add(file.Name);
                if (!add) {
                    ComboBox1.Text = file.Name;
                    add = true;
                }
            }
        }
        public static List<DropzWindow> items = new List<DropzWindow>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for(int a=1; ; a++)
            {
                if (!items.Any(r => r.Id == a))
                {
                    if (ComboBox1.Text != null && ComboBox1.Text != "")
                    {
                        items.Add(new DropzWindow() { Id = a, Description = a.ToString(), Script = ComboBox1.Text, Start = false, Status = "---", UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36", Visible = false, AutoRunning = false, Proxytype = ProxyType.none, Host = null, Port = null, Width = 800, Height = 600, ButtonReady = true ,HidePopup=true,DelayClosePopup=6000});
                        lvUsers.Items.Refresh();
                    }
                    break;
                }
            }
        }

        private List<Thread> ListThread;

        private void StartAndStop(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            string Uid = s.Uid;
            int IndexDropzWindow = items.FindIndex(x => x.Id == Convert.ToInt32(Uid));
            items[IndexDropzWindow].ButtonReady = false;
            RefreshListView();
            //MessageBox.Show(IndexDropzWindow+Uid + items[IndexDropzWindow].Status);
            //MessageBox.Show(items[IndexDropzWindow].Start.ToString());
            if (!items[IndexDropzWindow].Start)
            {
                Thread mainThread = new Thread(() =>
                {
                    Process p = new Process();
                    p.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + "/dropzwindow.exe";
                    string Arguments = Uid + " ";
                    Arguments += Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(items[IndexDropzWindow].UserAgent)) + " ";
                    Arguments += Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(items[IndexDropzWindow].Proxytype.ToString()+"|"+ items[IndexDropzWindow].Host + "|"+ items[IndexDropzWindow].Port)) + " ";
                    Arguments += Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Convert.ToString(items[IndexDropzWindow].Width))) + " ";
                    Arguments += Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Convert.ToString(items[IndexDropzWindow].Height))) + " ";
                    Arguments += Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Convert.ToString(items[IndexDropzWindow].Script))) + " ";
                    Arguments += Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Convert.ToString(items[IndexDropzWindow].HidePopup))) + " ";
                    Arguments += Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Convert.ToString(items[IndexDropzWindow].DelayClosePopup))) + " ";
                    p.StartInfo.Arguments = Arguments;
                    //MessageBox.Show(Arguments);
                    if (!items[IndexDropzWindow].Visible)
                        p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.Start();
                    items[IndexDropzWindow].IdProcess = p.Id;

                    NamedPipeServerStream ppsv = new NamedPipeServerStream("dropzwindow" + Uid, PipeDirection.InOut);
                    ppsv.WaitForConnection();

                    if (new Pipe().SendAndReceive(ppsv, "Start")=="OK")
                    {
                        items[IndexDropzWindow].Start = true;
                        items[IndexDropzWindow].ButtonReady = true;
                        RefreshListView();
                        while (items[IndexDropzWindow].Start)
                        {
                            Thread.Sleep(250);

                            if (items[IndexDropzWindow].CommandSend == null && items[IndexDropzWindow].AutoRunning == true)
                            {
                                items[IndexDropzWindow].CommandSend = "CheckData";
                            }


                            if (items[IndexDropzWindow].CommandSend != null)
                            {

                                items[IndexDropzWindow].DataReceived= new Pipe().SendAndReceive(ppsv, items[IndexDropzWindow].CommandSend);

                                if (items[IndexDropzWindow].CommandSend == "Auto")
                                {
                                    if (items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        items[IndexDropzWindow].ButtonReady = true;
                                        items[IndexDropzWindow].AutoRunning = true;
                                        RefreshListView();
                                    }
                                }
                                else if (items[IndexDropzWindow].CommandSend == "StopAuto")
                                {
                                    if (items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        items[IndexDropzWindow].ButtonReady = true;
                                        items[IndexDropzWindow].AutoRunning = false;
                                        RefreshListView();
                                    }
                                }
                                else if (items[IndexDropzWindow].CommandSend == "CheckData")
                                {
                                    string[] result = items[IndexDropzWindow].DataReceived.Split('|');
                                    items[IndexDropzWindow].Response = result[0];
                                    if (Convert.ToBoolean(result[1]) == true)
                                        items[IndexDropzWindow].Captcha = true;
                                    else { items[IndexDropzWindow].Captcha = false; }
                                    RefreshListView();
                                }else if(items[IndexDropzWindow].CommandSend == "Stop")
                                {
                                    //MessageBox.Show("stop");
                                    if (items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        items[IndexDropzWindow].ButtonReady = true;
                                        try
                                        {
                                            Process.GetProcessById(items[IndexDropzWindow].IdProcess).Kill();
                                            Process.GetProcessById(items[IndexDropzWindow].IdProcess).Dispose();
                                        }
                                        catch { }
                                        new Pipe().Disconnect(ppsv);
                                        items[IndexDropzWindow].Start = false;
                                        items[IndexDropzWindow].AutoRunning = false;
                                        items[IndexDropzWindow].Visible = false;
                                        RefreshListView();
                                    }
                                }
                                else if (items[IndexDropzWindow].CommandSend == "Hide")
                                {
                                    //MessageBox.Show("stop");
                                    if (items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        items[IndexDropzWindow].ButtonReady = true;
                                        items[IndexDropzWindow].Visible = false;
                                        RefreshListView();
                                    }
                                }
                                else if (items[IndexDropzWindow].CommandSend == "Show")
                                {
                                    //MessageBox.Show("stop");
                                    if (items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        items[IndexDropzWindow].ButtonReady = true;
                                        items[IndexDropzWindow].Visible = true;
                                        RefreshListView();
                                    }
                                }

                                items[IndexDropzWindow].CommandSend = null;

                            }
                        }
                    }

                });
                mainThread.Start();
            }
            else
            {
                items[IndexDropzWindow].CommandSend = "Stop";
            }
        }
        private void AbortMe(Thread thread)
        {
            thread.Abort();
        }
        private void ShowClose(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            string Uid = s.Uid;
            int IndexDropzWindow = items.FindIndex(x => x.Id == Convert.ToInt32(Uid));
            items[IndexDropzWindow].ButtonReady = false;
            if (items[IndexDropzWindow].Visible)
            {
                items[IndexDropzWindow].CommandSend = "Hide";
            }
            else
            {
                items[IndexDropzWindow].CommandSend = "Show";
            }
        }

        private void Auto(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            string Uid = s.Uid;
            int IndexDropzWindow = items.FindIndex(x => x.Id == Convert.ToInt32(Uid));
            items[IndexDropzWindow].ButtonReady = false;
            if (!items[IndexDropzWindow].AutoRunning)
            {
                items[IndexDropzWindow].CommandSend = "Auto";
            }
            else
            {
                items[IndexDropzWindow].CommandSend = "StopAuto";
            }
        }
        private void RefreshListView()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lvUsers.Items.Refresh();
            }));
        }
        private void Focus()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Focus();
            }));
        }

        private void SaveConfigs()
        {
            string[] itemList = new string[0];
            for(int a = 0; a < items.Count; a++)
            {
                Array.Resize(ref itemList, itemList.Length + 1);
                itemList[itemList.Length-1]= JsonConvert.SerializeObject(items[a]);
            }
            string configs = String.Join(Environment.NewLine, itemList);
            File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + "//configs", configs);
        }
        private void LoadConfigs()
        {
            try
            {
                string[] itemList = File.ReadAllLines(System.AppDomain.CurrentDomain.BaseDirectory + "//configs");
                for (int a = 0; a < itemList.Length; a++)
                {
                    DropzWindow currentItem = JsonConvert.DeserializeObject<DropzWindow>(itemList[a]);
                    currentItem.Start = false;
                    currentItem.AutoRunning = false;
                    items.Add(currentItem);
                }
            }
            catch
            {
                items = new List<DropzWindow>();
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            SaveConfigs();
            Environment.Exit(0);
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            string Uid = s.Uid;
            int IndexDropzWindow = items.FindIndex(x => x.Id == Convert.ToInt32(Uid));
            if (!items[IndexDropzWindow].Start)
            {
                try
                {
                    Directory.Delete(System.AppDomain.CurrentDomain.BaseDirectory + "//CacheCef//" + Uid.ToString(), true);
                }
                catch { }
                items.Remove(items[IndexDropzWindow]);
                RefreshListView();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng chưa có");
        }

        private void Config(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            string Uid = s.Uid;
            int IndexDropzWindow = items.FindIndex(x => x.Id == Convert.ToInt32(Uid));
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
            SaveConfigs();
            Environment.Exit(0);
        }
    }
    public class Pipe
    {
        public static bool PipeIsBusy;
        //public void Send(NamedPipeServerStream sender, string content)
        //{
        //    var data = GetBytes(content);
        //    sender.Write(data, 0, data.Length);
        //}
        public string SendAndReceive(NamedPipeServerStream sender,string content)
        {
            string result = null;
            if (sender.IsConnected)
            {
                try
                {
                    var data = GetBytes(content);
                    sender.Write(data, 0, data.Length);
                    result = Read(sender);
                }
                catch { }
            }
            return result;
        }
        public void Disconnect(NamedPipeServerStream sender)
        {
            try
            {
                sender.Disconnect();
                sender.Close();
                sender.Dispose();
            }
            catch { }
        }
        public static byte[] GetBytes(string str)
        {
            str = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        public string Read(NamedPipeServerStream sender)
        {
            var buffer = new byte[1000];
            if (sender.IsConnected)
                sender.Read(buffer, 0, 1000);
            return GetString(buffer);
        }
        public string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            string base64 = Regex.Replace(new string(chars), @"[^a-zA-Z0-9\+\/=]+", string.Empty);
            string result =Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            return result;
        }
    }
    public class ListItem
    {
        public static List<DropzWindow> items;
    }
    public class DropzWindow
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Script { get; set; }
        public string Status { get; set; }
        public bool AutoRunning { get; set; }
        public string Response { get; set; }
        public string Action { get; set; }
        public bool Start { get; set; }
        public int IdProcess { get; set; }
        public string CommandSend { get; set; }
        public string DataReceived { get; set; }
        public string UserAgent { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public bool Visible { get; set; }
        public ProxyType Proxytype { get; set; }
        public bool Captcha { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool ButtonReady { get; set; }
        public bool HidePopup { get; set; }
        public int DelayClosePopup { get; set; }
    }
    public enum ProxyType
    {
        none,
        socks5
    }
}
