using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dropzwindow
{
    public class Setting
    {
        public ProxyType Proxytype { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool HidePopup { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string UserAgent { get; set; }
        public bool Visible { get; set; }
        public int DelayClosePopup { get; set; }
    }
    public enum ProxyType
    {
        none,
        socks5,
        https
    }
    public enum ScriptType
    {
        claimtoolcommand,
        csscript
    }
}
