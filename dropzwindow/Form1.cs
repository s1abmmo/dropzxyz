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
using System.Globalization;

namespace dropzwindow
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Width = InfomationStartup.Width;
            this.Height = InfomationStartup.Height;
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
            if (InfomationStartup.ProxyType == "socks5")
                settings.CefCommandLineArgs.Add("proxy-server", "socks5://" + InfomationStartup.Host + ":" + InfomationStartup.Port);
            //settings.CefCommandLineArgs.Add("--disable-notifications","");
            //settings.DisableGpuAcceleration();
            Cef.Initialize(settings);
            // Create a browser component
            chromeBrowser = new ChromiumWebBrowser("http://my.dropz.xyz");
            chromeBrowser.LifeSpanHandler = new LifespanHandler();
            //JsDialogHandler jss = new JsDialogHandler();
            //chromeBrowser.JsDialogHandler=jss;
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
            else if (data == "Auto")
            {
                bool threadalive = false;
                try
                {
                    if (InfomationStartup.AutoThread.IsAlive)
                        threadalive = true;
                }
                catch { }
                if (!threadalive)
                {
                    InfomationStartup.AutoThread = new Thread(() =>
                        {
                            try
                            {
                                Auto();
                            }
                            catch (Exception e){
                                InfomationStartup.Response = "Auto Error";
                                try
                                {
                                    File.AppendAllText(Application.StartupPath + "//logs.txt", DateTime.Now.ToString()+":"+ e.Message+Environment.NewLine);
                                }
                                catch {
                                    try { File.WriteAllText(Application.StartupPath + "//logs.txt", DateTime.Now.ToString() + ":" + e.Message + Environment.NewLine); } catch { }
                                }
                            }
                        });
                    InfomationStartup.AutoThread.Start();
                    return "OK";
                }
            }
            else if (data == "CheckData")
            {
                return InfomationStartup.Response + "|" + InfomationStartup.Captcha.ToString();
            } else if (data == "StopAuto")
            {
                try
                {
                    InfomationStartup.AutoThread.Abort();
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
        private void Auto()
        {
            InfomationStartup.AutoScript=File.ReadAllLines(Application.StartupPath + "//Auto//" + InfomationStartup.AutoScriptNameFile);
            while (true)
            {
                for(int a=0;a< InfomationStartup.AutoScript.Length; a++)
                {
                    string[] AutoScriptChildren = InfomationStartup.AutoScript[a].Split('|');
                    for (int b=0;b< AutoScriptChildren.Length; b++)
                    {
                        //MessageBox.Show(AutoScriptChildren[b]);
                        if (Regex.IsMatch(AutoScriptChildren[b], "^url"))
                        {
                            if (!new HandleAutoCommand().Url(chromeBrowser, AutoScriptChildren[b].Split('\'')[1]))
                                break;
                        }else if(Regex.IsMatch(AutoScriptChildren[b], "^checkvisible"))
                        {
                            if (!new HandleAutoCommand().CheckVisible(chromeBrowser, AutoScriptChildren[b].Split('\'')[1]))
                                break;
                        }else if (Regex.IsMatch(AutoScriptChildren[b], "^click"))
                        {
                            if (!new HandleAutoCommand().Click(chromeBrowser, AutoScriptChildren[b].Split('\'')[1]))
                                break;
                        }else if(Regex.IsMatch(AutoScriptChildren[b], "^delay"))
                        {
                            MatchCollection coll = Regex.Matches(AutoScriptChildren[b], "^delay\\((\\d+)\\)");
                            int delaytime = Convert.ToInt32(coll[0].Groups[1].Value);
                            Thread.Sleep(delaytime);
                        }
                        else if (Regex.IsMatch(AutoScriptChildren[b], "^getdata"))
                        {
                            InfomationStartup.Response = new HandleAutoCommand().EvaluateJavascript(chromeBrowser, AutoScriptChildren[b].Split('\'')[1]);
                        }
                        else if (Regex.IsMatch(AutoScriptChildren[b], "^checkcaptcha"))
                        {
                            if (new HandleAutoCommand().CheckVisible(chromeBrowser, AutoScriptChildren[b].Split('\'')[1]))
                            {
                                InfomationStartup.Captcha = true;
                                while (true)
                                {
                                    Thread.Sleep(1000);
                                    if (new HandleAutoCommand().CheckVisible(chromeBrowser, AutoScriptChildren[b].Split('\'')[3]))
                                        break;
                                }
                                InfomationStartup.Captcha = false;
                            }
                        }
                        else if(Regex.IsMatch(AutoScriptChildren[b], "^javascript"))
                        {
                            new HandleAutoCommand().EvaluateJavascript(chromeBrowser, AutoScriptChildren[b].Split('\'')[1]);
                        }else if(Regex.IsMatch(AutoScriptChildren[b], "^int"))
                        {
                            if (!new HandleAutoCommand().IntCondition(chromeBrowser, AutoScriptChildren[b]))
                                break;
                        }else if (Regex.IsMatch(AutoScriptChildren[b], "^string"))
                        {
                            if (!new HandleAutoCommand().StringCondition(chromeBrowser, AutoScriptChildren[b]))
                                break;
                        }
                        else if (Regex.IsMatch(AutoScriptChildren[b], "^waitload"))
                        {
                            if (!new HandleAutoCommand().WaitPageLoad(chromeBrowser))
                                break;
                        }
                    }
                }
            }
            //chromeBrowser.Load("https://my.dropz.xyz/site-friends/");
            //while (true)
            //{
            //    Thread.Sleep(500);
            //    if (!chromeBrowser.IsLoading)
            //        break;
            //}
            //while (true)
            //{
            //    while (true)
            //    {
            //        Thread.Sleep(1000);
            //        if (!chromeBrowser.IsLoading)
            //            break;
            //    }
            //    string balance = EvaluateScript(chromeBrowser, "document.querySelector('b#display_pending_drops').innerHTML;");
            //    if (balance == null)
            //    {
            //        InfomationStartup.AccountLive = false;
            //    }
            //    else
            //    {
            //        InfomationStartup.AccountLive = true;
            //        InfomationStartup.Balance = balance;
            //    }
            //    if (int.Parse(InfomationStartup.Balance, NumberStyles.Currency) >InfomationStartup.AmountWithdraw)
            //    {
            //        if (CheckVisibleElement(chromeBrowser, "button#payout_bt")){
            //            chromeBrowser.EvaluateScriptAsync("document.querySelector('button#payout_bt').scrollIntoView();");
            //            Thread.Sleep(100);
            //            chromeBrowser.EvaluateScriptAsync("window.scrollBy(0, -"+(InfomationStartup.Height/2).ToString()+");");
            //            Thread.Sleep(500);
            //            int x = Convert.ToInt32(Convert.ToDecimal(EvaluateScript(chromeBrowser, "document.querySelector('button#payout_bt').getBoundingClientRect().x;")));
            //            int y = Convert.ToInt32(Convert.ToDecimal(EvaluateScript(chromeBrowser, "document.querySelector('button#payout_bt').getBoundingClientRect().y;")));
            //            chromeBrowser.GetBrowser().GetHost().SendMouseClickEvent(x + 5, y + 5, MouseButtonType.Left, false, 1, CefEventFlags.None);
            //            Thread.Sleep(100);
            //            chromeBrowser.GetBrowser().GetHost().SendMouseClickEvent(x + 5, y + 5, MouseButtonType.Left, true, 1, CefEventFlags.None);
            //            Thread.Sleep(1000);
            //            chromeBrowser.Load("https://my.dropz.xyz/site-friends/");
            //        }
            //    }else if (CheckVisibleElement(chromeBrowser, "button.swal2-confirm.swal2-styled"))
            //    {
            //        chromeBrowser.EvaluateScriptAsync("document.querySelector('button.swal2-confirm.swal2-styled').scrollIntoView();");
            //        Thread.Sleep(100);
            //        chromeBrowser.EvaluateScriptAsync("window.scrollBy(0, -" + (InfomationStartup.Height / 2).ToString() + ");");
            //        Thread.Sleep(500);
            //        int x = Convert.ToInt32(Convert.ToDecimal(EvaluateScript(chromeBrowser, "document.querySelector('button.swal2-confirm.swal2-styled').getBoundingClientRect().x;")));
            //        int y = Convert.ToInt32(Convert.ToDecimal(EvaluateScript(chromeBrowser, "document.querySelector('button.swal2-confirm.swal2-styled').getBoundingClientRect().y;")));
            //        chromeBrowser.GetBrowser().GetHost().SendMouseClickEvent(x + 5, y + 5, MouseButtonType.Left, false, 1, CefEventFlags.None);
            //        Thread.Sleep(100);
            //        chromeBrowser.GetBrowser().GetHost().SendMouseClickEvent(x + 5, y + 5, MouseButtonType.Left, true, 1, CefEventFlags.None);
            //    }
            //    else if (CheckVisibleElement(chromeBrowser, "a.btn.btn-app>i.fa.fa-play") || CheckVisibleElement(chromeBrowser, "a.btn.btn-app>i.fa.fa-repeat"))
            //    {
            //        InfomationStartup.Hcaptcha = false;
            //        chromeBrowser.EvaluateScriptAsync("document.querySelector('a.btn.btn-app').scrollIntoView();");
            //        Thread.Sleep(100);
            //        chromeBrowser.EvaluateScriptAsync("window.scrollBy(0, -" + (InfomationStartup.Height / 2).ToString() + ");");
            //        Thread.Sleep(500);
            //        int x = Convert.ToInt32(Convert.ToDecimal(EvaluateScript(chromeBrowser, "document.querySelector('a.btn.btn-app').getBoundingClientRect().x;")));
            //        int y = Convert.ToInt32(Convert.ToDecimal(EvaluateScript(chromeBrowser, "document.querySelector('a.btn.btn-app').getBoundingClientRect().y;")));
            //        chromeBrowser.GetBrowser().GetHost().SendMouseClickEvent(x + 5, y + 5, MouseButtonType.Left, false, 1, CefEventFlags.None);
            //        Thread.Sleep(100);
            //        chromeBrowser.GetBrowser().GetHost().SendMouseClickEvent(x + 5, y + 5, MouseButtonType.Left, true, 1, CefEventFlags.None);
            //    }
            //    else if (CheckVisibleElement(chromeBrowser, "div.h-captcha"))
            //    {
            //        InfomationStartup.Hcaptcha = true;
            //    }
            //    else if (!Regex.IsMatch(chromeBrowser.Address, "site-friends"))
            //    {
            //        chromeBrowser.Load("https://my.dropz.xyz/site-friends/");
            //    }
            //}
        }
        //private bool CheckVisibleElement(ChromiumWebBrowser browser, string element)
        //{
        //    bool Visible = false;
        //    int width = Convert.ToInt32(Convert.ToDecimal(EvaluateScript(browser, "document.querySelector('" + element + "').getBoundingClientRect().width;")));
        //    int height = Convert.ToInt32(Convert.ToDecimal(EvaluateScript(browser, "document.querySelector('" + element + "').getBoundingClientRect().height;")));
        //    if (width != 0 && height != 0) {
        //        Visible = true;
        //    }
        //    return Visible;
        //}
        //public string EvaluateScript(ChromiumWebBrowser browser, string script)
        //{
        //    string result = null;
        //    browser.EvaluateScriptAsync(script).ContinueWith(x =>
        //    {
        //        var response = x.Result;

        //        if (response.Success && response.Result != null)
        //        {
        //            var startDate = response.Result;
        //            result = startDate.ToString();
        //        }
        //    }).Wait();
        //    return result;
        //}
    }

    public class HandleAutoCommand
    {
        public bool Url(ChromiumWebBrowser browser,string url)
        {
            browser.Load(url);
            while (true)
            {
                Thread.Sleep(500);
                if (!browser.IsLoading)
                    break;
            }
            return true;
        }
        public bool CheckVisible(ChromiumWebBrowser browser, string element)
        {
            bool Visible = false;
            int width = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript(browser, "document.querySelector('" + element + "').getBoundingClientRect().width;")));
            int height = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript(browser, "document.querySelector('" + element + "').getBoundingClientRect().height;")));
            if (width != 0 && height != 0)
            {
                Visible = true;
            }
            return Visible;
        }
        public bool Click(ChromiumWebBrowser browser, string element)
        {
            bool success = false;
            if (CheckVisible(browser, element))
            {
                browser.EvaluateScriptAsync("document.querySelector('" + element + "').scrollIntoView();");
                Thread.Sleep(100);
                browser.EvaluateScriptAsync("window.scrollBy(0, -" + (InfomationStartup.Height / 2).ToString() + ");");
                Thread.Sleep(500);
                int x = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript(browser, "document.querySelector('" + element + "').getBoundingClientRect().x;")));
                int y = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript(browser, "document.querySelector('" + element + "').getBoundingClientRect().y;")));
                if (x != 0 && y != 0)
                {
                    browser.GetBrowser().GetHost().SendMouseClickEvent(x + 5, y + 5, MouseButtonType.Left, false, 1, CefEventFlags.None);
                    Thread.Sleep(100);
                    browser.GetBrowser().GetHost().SendMouseClickEvent(x + 5, y + 5, MouseButtonType.Left, true, 1, CefEventFlags.None);
                    success = true; 
                }
            }
            return success;
        }
        public bool WaitPageLoad(ChromiumWebBrowser browser)
        {
            while (true)
            {
                Thread.Sleep(500);
                if (!browser.IsLoading)
                    break;
            }
            return true;
        }
        public bool IntCondition(ChromiumWebBrowser browser,string command)
        {
            bool ok = false;
            string[] command1 = command.Split('\'');
            MatchCollection coll = Regex.Matches(command1[2], "([>=<]{1,2})(\\d+)");
            string bieuthuc=coll[0].Groups[1].Value;
            int so = Convert.ToInt32(coll[0].Groups[2].Value);
            int so2 = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript(browser, command1[2])));
            switch (bieuthuc)
            {
                case ">":
                    if (so2 > so)
                        ok = true;
                    break;
                case ">=":
                    if (so2 >= so)
                        ok = true;
                    break;
                case "==":
                    if (so2 == so)
                        ok = true;
                    break;
                case "<=":
                    if (so2 <= so)
                        ok = true;
                    break;
                case "<":
                    if (so2 < so)
                        ok = true;
                    break;
                default:
                    break;
            }
            return ok;
        }

        public bool StringCondition(ChromiumWebBrowser browser, string command)
        {
            bool ok = false;
            string[] command1 = command.Split('\'');
            if (Regex.IsMatch(EvaluateJavascript(browser, command1[1]), command1[3]))
                ok = true;
            return ok;
        }

        public string EvaluateJavascript(ChromiumWebBrowser browser, string javascript)
        {
            string result = null;
            browser.EvaluateScriptAsync(javascript).ContinueWith(x =>
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
            if (InfomationStartup.HidePopup)
            {
                form.Activate();
            }
            else
            {
                form.Width = InfomationStartup.Width;
                form.Height = InfomationStartup.Height;
                form.Show();
            }
            windowInfo.SetAsChild(form.Handle, 0,0,0,0);
            windowInfo.Width = InfomationStartup.Width;
            windowInfo.Height = InfomationStartup.Height;
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
                    Thread.Sleep(InfomationStartup.TimeDelayClosePopup);
                    browser.CloseBrowser(true);
                }
            });
            t.Start();
        }
    }
    //public class JsDialogHandler : IJsDialogHandler
    //{
    //    public bool OnJSDialog(IWebBrowser browserControl, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText, IJsDialogCallback callback, ref bool suppressMessage)
    //    {
    //        callback.Continue(true);
    //        return false;
    //    }
    //    public bool OnJSBeforeUnload(IWebBrowser browserControl, IBrowser browser, string message, bool isReload, IJsDialogCallback callback)
    //    {
    //        return true;
    //    }

    //    public void OnResetDialogState(IWebBrowser browserControl, IBrowser browser)
    //    {

    //    }

    //    public void OnDialogClosed(IWebBrowser browserControl, IBrowser browser)
    //    {

    //    }
    //}
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
        public static string Response;
        public static bool Captcha;
        public static Thread AutoThread;
        public static string AutoScriptNameFile;
        public static string[] AutoScript;
        public static string Url;
        public static string UserAgent;
        public static string ProxyType;
        public static string Host;
        public static string Port;
        public static int Width;
        public static int Height;
        public static bool HidePopup;
        public static int TimeDelayClosePopup;
    }
}
