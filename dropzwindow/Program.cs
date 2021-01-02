using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;

namespace dropzwindow
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            InfomationStartup.IdDropzWindow = args[0];
            InfomationStartup.UserAgent = Encoding.UTF8.GetString(Convert.FromBase64String(args[1]));
            string[] proxy = Encoding.UTF8.GetString(Convert.FromBase64String(args[2])).Split('|');
                InfomationStartup.ProxyType = proxy[0];
                InfomationStartup.Host = proxy[1];
                InfomationStartup.Port = proxy[2];
            InfomationStartup.AmountWithdraw =Convert.ToInt32( Encoding.UTF8.GetString(Convert.FromBase64String(args[3])));
            InfomationStartup.Width = Convert.ToInt32(Encoding.UTF8.GetString(Convert.FromBase64String(args[4])));
            InfomationStartup.Height = Convert.ToInt32(Encoding.UTF8.GetString(Convert.FromBase64String(args[5])));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
