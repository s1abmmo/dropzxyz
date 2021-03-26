using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ctc
{
    public class Url : ClaimToolCommand
    {
        private static readonly Url Self = new Url();

        private Url()
        {
        }
        public bool Run(string fullcommand)
        {
            Match match=Regex.Match(fullcommand, "url\\('([^']+)'\\)");
            string url = match.Groups[1].Value;
            //MessageBox.Show(url);
            Browser.ChromeBrowser.Load(url);
            while (true)
            {
                Thread.Sleep(500);
                if (!Browser.ChromeBrowser.IsLoading)
                    break;
            }
            return true;
        }
        public static Url Instance()
        {
            return Self;
        }
    }
}
