using System;
using System.Text;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Text.RegularExpressions;
using ctc;
using dropzwindow.ConfigBrowser;
using dropzwindow.Command;
using System.Collections.Generic;
using System.Reflection;

namespace dropzwindow
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            dropzwindow.HandleScript.HandleScript.ReplaceVariable();
            dropzwindow.HandleScript.HandleScript.CheckScriptInvalid();
            //MessageBox.Show(dropzwindow.HandleScript.HandleScript.CheckScriptInvalid().ToString());
            this.button1.Visible = true;
            this.Width = Info.Setting.Width;
            this.Height = Info.Setting.Height;
            InfomationStartup.ClickRecaptcha = false;
            InitializeChromium.Initialize();
            this.Controls.Add(Browser.ChromeBrowser);
            Thread t = new Thread(() =>
            {
                InfomationStartup.client = new NamedPipeClientStream("dropzwindow" + InfomationStartup.IdDropzWindow);
                InfomationStartup.client.Connect();
                while (true)
                {
                    Thread.Sleep(250);
                    string data = new Pipe().Read(InfomationStartup.client);
                    string response = HandleData(data);
                    new Pipe().Response(InfomationStartup.client, response);
                    if (!InfomationStartup.client.IsConnected)
                        Application.Exit();
                }
            });
            t.Start();
        }

        private string HandleData(string data)
        {
            if (data == "Start")
            {
                return this.Handle.ToString();
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
                                HandleCommand.Auto(this.Width,this.Height,this.Visible);
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
            }else if (Regex.IsMatch(data,"^clickcaptcha"))
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    this.Visible = true;
                    string[] widthheight = data.Split('|');
                    this.Width = Convert.ToInt32(widthheight[1]);
                    this.Height = Convert.ToInt32(widthheight[2]);
                    InfomationStartup.ClickRecaptcha = true;
                    Application.DoEvents();
                });
                return "OK";
            }
            return "null";
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {

            //var DLL = Assembly.LoadFile(@"C:\Users\HaiDang\Project\dropzxyz\ClassLibrary1\bin\Debug\ClassLibrary1.dll");

            //foreach (Type type in DLL.GetExportedTypes())
            //{
            //    dynamic c = Activator.CreateInstance(type);
            //    c.Run();
            //}

            foo("https://whoer.net/");
        }
        public async void foo(string targetUrl)
        {
            var cookieManager = Cef.GetGlobalCookieManager();
            var visitor = new CookieCollector();

            cookieManager.VisitUrlCookies(targetUrl, true, visitor);

            var cookies = await visitor.Task; // AWAIT !!!!!!!!!
            var cookieHeader = CookieCollector.GetCookieHeader(cookies);
            MessageBox.Show(cookieHeader);
        }

    }

    class CookieCollector : ICookieVisitor
    {
        private readonly TaskCompletionSource<List<Cookie>> _source = new TaskCompletionSource<List<Cookie>>();

        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            _cookies.Add(cookie);

            if (count == (total - 1))
            {
                _source.SetResult(_cookies);
            }
            return true;
        }

        // https://github.com/amaitland/CefSharp.MinimalExample/blob/ce6e579ad77dc92be94c0129b4a101f85e2fd75b/CefSharp.MinimalExample.WinForms/ListCookieVisitor.cs
        // CefSharp.MinimalExample.WinForms ListCookieVisitor 

        public Task<List<Cookie>> Task => _source.Task;

        public static string GetCookieHeader(List<Cookie> cookies)
        {

            StringBuilder cookieString = new StringBuilder();
            string delimiter = string.Empty;

            foreach (var cookie in cookies)
            {
                cookieString.Append(delimiter);
                cookieString.Append(cookie.Name);
                cookieString.Append('=');
                cookieString.Append(cookie.Value);
                delimiter = "; ";
            }

            return cookieString.ToString();
        }

        private readonly List<Cookie> _cookies = new List<Cookie>();
        public void Dispose()
        {
        }
    }

    public static class InfomationStartup
    {
        public static string IdDropzWindow;
        public static NamedPipeClientStream client;
        public static string Response;
        public static bool Captcha;
        public static Thread AutoThread;
        public static bool ClickRecaptcha;
        public static int Top;
        public static int Left;
    }
    //public class MyRequestHandler
    //{

    //    bool IRequestHandler.GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
    //    {

    //        if (isProxy == true)
    //        {
    //            callback.Continue("Username", "Password");

    //            return true;
    //        }

    //        return false;

    //    }
    //}
}
