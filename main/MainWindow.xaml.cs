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
        }
        public static List<DropzWindow> items = new List<DropzWindow>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for(int a=1; ; a++)
            {
                if (!items.Any(r => r.Id == a))
                {
                    items.Add(new DropzWindow() { Id = a, Description = a.ToString(), Start = false, Status = "---",UserAgent= "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36",Visible=false,AutoClaimRunning=false,Proxytype=ProxyType.none,Host=null,Port=null });
                    lvUsers.Items.Refresh();
                    break;
                }
            }
        }
        private void StartAndStop(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            string Uid = s.Uid;
            int IndexDropzWindow = items.FindIndex(x => x.Id == Convert.ToInt32(Uid));
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
                        RefreshListView();
                        while (true)
                        {
                            Thread.Sleep(250);

                            if (items[IndexDropzWindow].CommandSend == null && items[IndexDropzWindow].AutoClaimRunning == true)
                            {
                                items[IndexDropzWindow].CommandSend = "CheckBalance";
                            }


                            if (items[IndexDropzWindow].CommandSend != null)
                            {

                                items[IndexDropzWindow].DataReceived= new Pipe().SendAndReceive(ppsv, items[IndexDropzWindow].CommandSend);

                                if (items[IndexDropzWindow].CommandSend == "AutoClaim")
                                {
                                    if (items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        items[IndexDropzWindow].AutoClaimRunning = true;
                                        RefreshListView();
                                    }
                                }
                                else if (items[IndexDropzWindow].CommandSend == "StopAutoClaim")
                                {
                                    if (items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        items[IndexDropzWindow].AutoClaimRunning = false;
                                        RefreshListView();
                                    }
                                }
                                else if (items[IndexDropzWindow].CommandSend == "CheckBalance")
                                {
                                    string[] result = items[IndexDropzWindow].DataReceived.Split('|');
                                    if (result[0] == "True")
                                    {
                                        items[IndexDropzWindow].Balance = result[1];
                                        if (result[2] == "True")
                                            items[IndexDropzWindow].Hcaptcha = true;
                                        else { items[IndexDropzWindow].Hcaptcha = false; }
                                        RefreshListView();
                                    }
                                }else if(items[IndexDropzWindow].CommandSend == "Stop")
                                {
                                    //MessageBox.Show("stop");
                                    if (items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        new Pipe().Disconnect(ppsv);
                                        items[IndexDropzWindow].Start = false;
                                        items[IndexDropzWindow].AutoClaimRunning = false;
                                        items[IndexDropzWindow].Visible = false;
                                        RefreshListView();
                                    }
                                }
                                else if (items[IndexDropzWindow].CommandSend == "Hide")
                                {
                                    //MessageBox.Show("stop");
                                    if (items[IndexDropzWindow].DataReceived == "OK")
                                    {
                                        items[IndexDropzWindow].Visible = false;
                                        RefreshListView();
                                    }
                                }
                                else if (items[IndexDropzWindow].CommandSend == "Show")
                                {
                                    //MessageBox.Show("stop");
                                    if (items[IndexDropzWindow].DataReceived == "OK")
                                    {
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
        private void ShowClose(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            string Uid = s.Uid;
            int IndexDropzWindow = items.FindIndex(x => x.Id == Convert.ToInt32(Uid));
            if (items[IndexDropzWindow].Visible)
            {
                items[IndexDropzWindow].CommandSend = "Hide";
            }
            else
            {
                items[IndexDropzWindow].CommandSend = "Show";
            }
        }

        private void AutoClaim(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            StackPanel s = b.Parent as StackPanel;
            string Uid = s.Uid;
            int IndexDropzWindow = items.FindIndex(x => x.Id == Convert.ToInt32(Uid));
            if (!items[IndexDropzWindow].AutoClaimRunning)
            {
                items[IndexDropzWindow].CommandSend = "AutoClaim";
            }
            else
            {
                items[IndexDropzWindow].CommandSend = "StopAutoClaim";
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
                    items.Add(JsonConvert.DeserializeObject<DropzWindow>(itemList[a]));
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
            try
            {
                Directory.Delete(System.AppDomain.CurrentDomain.BaseDirectory + "//CacheCef//" + Uid.ToString(), true);
            }
            catch { }
            items.Remove(items[IndexDropzWindow]);
            RefreshListView();
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
    }
    public class Pipe
    {
        //public void Send(NamedPipeServerStream sender, string content)
        //{
        //    var data = GetBytes(content);
        //    sender.Write(data, 0, data.Length);
        //}
        public string SendAndReceive(NamedPipeServerStream sender,string content)
        {
            var data = GetBytes(content);
            sender.Write(data, 0, data.Length);
            return Read(sender);
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
        public string Status { get; set; }
        public bool AutoClaimRunning { get; set; }
        public string Balance { get; set; }
        public string Action { get; set; }
        public bool Start { get; set; }
        //public Thread Thread { get; set; }
        public int IdProcess { get; set; }
        //public NamedPipeServerStream ServerSender { get; set; }
        public string CommandSend { get; set; }
        public string DataReceived { get; set; }
        public string UserAgent { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public bool Visible { get; set; }
        public ProxyType Proxytype { get; set; }
        public bool Hcaptcha { get; set; }
    }
    public enum ProxyType
    {
        none,
        socks5
    }
}
