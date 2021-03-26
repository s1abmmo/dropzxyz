using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using ctc;

namespace dropzwindow.ConfigBrowser
{
    public class InitializeChromium
    {
        public static void Initialize()
        {
            CefSettings settings = new CefSettings();
            //CefSharpSettings.Proxy=new ProxyOptions(ip:"",port:"",username:)
            settings.CachePath = Application.StartupPath + "/CacheCef/" + InfomationStartup.IdDropzWindow;
            settings.UserAgent = Info.Setting.UserAgent;
            settings.DisableGpuAcceleration();
            settings = Proxy.ConfigProxy(settings);
            Cef.Initialize(settings);
            Browser.ChromeBrowser = new ChromiumWebBrowser("");//https://whoer.net/
            Browser.ChromeBrowser.LifeSpanHandler = new LifespanHandler();
            //Browser.ChromeBrowser.RequestHandler =new MyRequestHandler();
            Browser.ChromeBrowser.Dock = DockStyle.Fill;
        }
    }
}
