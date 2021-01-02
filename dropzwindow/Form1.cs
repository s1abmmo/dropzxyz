using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace dropzwindow
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeChromium();
            Thread t = new Thread(() =>
            {
                InfomationStartup.client = new NamedPipeClientStream("dropzwindow" + InfomationStartup.IdDropzWindow);
                InfomationStartup.client.Connect();
                while (true)
                {
                    Thread.Sleep(250);
                    string data = new Pipe().Read(InfomationStartup.client);
                    string response = HandleData(data);
                    //MessageBox.Show(data+response);
                    new Pipe().Response(InfomationStartup.client, response);

                    if (!InfomationStartup.client.IsConnected)
                        Application.Exit();

                    this.Invoke((MethodInvoker)delegate ()
                    {
                        this.Text = InfomationStartup.client.IsConnected.ToString() + " " + data + " "+InfomationStartup.ProxyType+ InfomationStartup.Host+ InfomationStartup.Port;
                        Application.DoEvents();
                    });
                }
            });
            t.Start();
        }

        public ChromiumWebBrowser chromeBrowser;

        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            settings.CachePath = Application.StartupPath + "/CacheCef/" + InfomationStartup.IdDropzWindow;
            settings.UserAgent = InfomationStartup.UserAgent;
            if(InfomationStartup.ProxyType=="socks5")
            settings.CefCommandLineArgs.Add("proxy-server", "socks5://"+InfomationStartup.Host+":"+ InfomationStartup.Port);
            Cef.Initialize(settings);
            // Create a browser component
            chromeBrowser = new ChromiumWebBrowser("http://my.dropz.xyz");
            chromeBrowser.LifeSpanHandler = new LifespanHandler();
            // Add it to the form and fill it to the form window.
            this.Controls.Add(chromeBrowser);
            //this.Controls.Add();
            chromeBrowser.Dock = DockStyle.Fill;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            chromeBrowser.Load("http://my.dropz.xyz");
            //this.Visible = false;
        }

        private string HandleData(string data)
        {
            if (data == "Start")
            {
                return "OK";
            } else if (data == "Stop")
            {
                return "OK";
            }
            else if (data == "AutoClaim")
            {
                bool threadalive = false;
                try
                {
                    if (InfomationStartup.AutoClaimThread.IsAlive)
                        threadalive = true;
                }
                catch { }
                if (!threadalive)
                {
                    InfomationStartup.AutoClaimThread = new Thread(() =>
                        {
                            AutoClaim();
                        });
                    InfomationStartup.AutoClaimThread.Start();
                    return "OK";
                }
            }
            else if (data == "CheckBalance")
            {
                return InfomationStartup.AccountLive.ToString() + "|" + InfomationStartup.Balance + "|" + InfomationStartup.Hcaptcha.ToString();
            } else if (data == "StopAutoClaim")
            {
                try
                {
                    InfomationStartup.AutoClaimThread.Abort();
                }
                catch { }
                return "OK";
            }
            else if(data == "Hide")
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    this.Visible = false;
                    Application.DoEvents();
                });
                return "OK";
            }
            else if(data == "Show")
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    this.Visible = true;
                    Application.DoEvents();
                });
                return "OK";
            }
            return "null";
        }
        private void AutoClaim()
        {
            chromeBrowser.Load("https://my.dropz.xyz/site-friends/");
            while (true)
            {
                Thread.Sleep(500);
                if (!chromeBrowser.IsLoading)
                    break;
            }
            while (true)
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    if (!chromeBrowser.IsLoading)
                        break;
                }
                string balance = EvaluateScript(chromeBrowser, "document.querySelector('b#display_pending_drops').innerHTML;");
                if (balance == null)
                {
                    InfomationStartup.AccountLive = false;
                }
                else
                {
                    InfomationStartup.AccountLive = true;
                    InfomationStartup.Balance = balance;
                }
                if (CheckVisibleElement(chromeBrowser, "a.btn.btn-app>i.fa.fa-play") || CheckVisibleElement(chromeBrowser, "a.btn.btn-app>i.fa.fa-repeat"))
                {
                    InfomationStartup.Hcaptcha = false;
                    chromeBrowser.EvaluateScriptAsync("document.querySelector('a.btn.btn-app').scrollIntoView();");
                    Thread.Sleep(500);
                    int x = Convert.ToInt32(Convert.ToDecimal(EvaluateScript(chromeBrowser, "document.querySelector('a.btn.btn-app').getBoundingClientRect().x;")));
                    int y = Convert.ToInt32(Convert.ToDecimal(EvaluateScript(chromeBrowser, "document.querySelector('a.btn.btn-app').getBoundingClientRect().y;")));
                    chromeBrowser.GetBrowser().GetHost().SendMouseClickEvent(x + 5, y + 5, MouseButtonType.Left, false, 1, CefEventFlags.None);
                    Thread.Sleep(100);
                    chromeBrowser.GetBrowser().GetHost().SendMouseClickEvent(x + 5, y + 5, MouseButtonType.Left, true, 1, CefEventFlags.None);
                }
                else if (CheckVisibleElement(chromeBrowser, "div.h-captcha"))
                {
                    InfomationStartup.Hcaptcha = true;
                } else if (!Regex.IsMatch(chromeBrowser.Address, "site-friends"))
                {
                    chromeBrowser.Load("https://my.dropz.xyz/site-friends/");
                }
            }
        }
        private bool CheckVisibleElement(ChromiumWebBrowser browser, string element)
        {
            bool Visible = false;
            int width = Convert.ToInt32(Convert.ToDecimal(EvaluateScript(browser, "document.querySelector('" + element + "').getBoundingClientRect().width;")));
            int height = Convert.ToInt32(Convert.ToDecimal(EvaluateScript(browser, "document.querySelector('" + element + "').getBoundingClientRect().height;")));
            if (width != 0 && height != 0) {
                Visible = true;
            }
            return Visible;
        }
        public string EvaluateScript(ChromiumWebBrowser browser, string script)
        {
            string result = null;
            browser.EvaluateScriptAsync(script).ContinueWith(x =>
            {
                var response = x.Result;

                if (response.Success && response.Result != null)
                {
                    var startDate = response.Result;
                    result = startDate.ToString();
                }
            }).Wait();
            return result;
        }
    }
    public class LifespanHandler : ILifeSpanHandler
    {
        //event that receive url popup
        public event Action popup_request;
        public Form form;
        bool ILifeSpanHandler.OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            form = new Form();
            form.Activate();
            windowInfo.SetAsChild(form.Handle, 0,0,0,0);
            windowInfo.Width = 500;
            windowInfo.Height = 500;
            newBrowser = null;
            return false;
        }
        bool ILifeSpanHandler.DoClose(IWebBrowser browserControl, IBrowser browser)
        { return false; }

        void ILifeSpanHandler.OnBeforeClose(IWebBrowser browserControl, IBrowser browser) {

        }

        void ILifeSpanHandler.OnAfterCreated(IWebBrowser browserControl, IBrowser browser) {
            Thread t = new Thread(() =>
            {
                if (browser.IsPopup)
                {
                    Thread.Sleep(7000);
                    browser.CloseBrowser(true);
                }
            });
            t.Start();
        }
    }
    public class Pipe
    {
        public string Read(NamedPipeClientStream client)
        {
            var buffer = new byte[1000];
            client.Read(buffer, 0, 1000);
            return GetString(buffer);
        }
        public void Response(NamedPipeClientStream sender, string content)
        {
            var data = GetBytes(content);
            sender.Write(data, 0, data.Length);
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
        private byte[] GetBytes(string str)
        {
            str = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        public string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            string base64 = Regex.Replace(new string(chars), @"[^a-zA-Z0-9\+\/=]+", string.Empty);
            string result = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            return result;
        }
    }
    public static class InfomationStartup
    {
        public static string IdDropzWindow;
        public static NamedPipeClientStream client;
        public static string Balance;
        public static bool AccountLive;
        public static bool Hcaptcha;
        public static Thread AutoClaimThread;
        public static string Url;
        public static string UserAgent;
        public static string ProxyType;
        public static string Host;
        public static string Port;
    }
}
