using System.Threading;
using System.Text.RegularExpressions;
using System.Text;
using System;
using System.Windows.Forms;

namespace ctc
{
    public class CheckVisible:ClaimToolCommand
    {
        private static readonly CheckVisible Self = new CheckVisible();

        private CheckVisible()
        {
        }
        public bool Run(string fullcommand)
        {
            //MessageBox.Show("checkvisible: " + fullcommand);
            MatchCollection coll = Regex.Matches(fullcommand, "checkvisible\\('([^']+)'\\)");
            string element = coll[0].Groups[1].Value;
            //MessageBox.Show("checkvisible element: " + element);
            bool Visible = false;
            int width = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript.Instance().Run("document.querySelector('" + element + "').getBoundingClientRect().width;")));
            int height = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript.Instance().Run( "document.querySelector('" + element + "').getBoundingClientRect().height;")));
            if (width != 0 && height != 0)
            {
                Visible = true;
            }
            return Visible;
        }
        public static CheckVisible Instance()
        {
            return Self;
        }
    }
}
