using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp.WinForms;
using CefSharp;

namespace dropzwindow.ConfigBrowser
{
    public class Proxy
    {
        public static CefSettings ConfigProxy(CefSettings settings)
        {
            switch (Info.Setting.Proxytype)
            {
                case ProxyType.socks5:
                    settings.CefCommandLineArgs.Add("proxy-server", "socks5://" + Info.Setting.Host + ":" + Info.Setting.Port);
                    break;
                case ProxyType.https:
                    CefSharpSettings.Proxy = new ProxyOptions(ip: Info.Setting.Host, port: Info.Setting.Port.ToString(), username: Info.Setting.Username, password: Info.Setting.Password);
                    //settings.CefCommandLineArgs.Add("proxy-server", "https://" + Info.Setting.Host + ":" + Info.Setting.Port);
                    break;
                default:
                    break;
            }
            return settings;
        }
    }
}
