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
            try
            {
                InfomationStartup.IdDropzWindow = args[0];
                string autoparameters = Encoding.UTF8.GetString(Convert.FromBase64String(args[1]));
                string autoscript = Encoding.UTF8.GetString(Convert.FromBase64String(args[2]));
                string config = Encoding.UTF8.GetString(Convert.FromBase64String(args[3]));

                Info.AutoParameters = JsonConverting.DecodeJson(autoparameters);

                Info.AutoScript = JsonConverting.DecodeJsonAutoScript(autoscript);

                Info.Setting = JsonConverting.DecodeJsonSetting(config);
                //MessageBox.Show(Info.AutoParameters.ToString());
            }
            catch (Exception e){ //MessageBox.Show(e.Message);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
