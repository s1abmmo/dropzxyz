using System;
using System.Text.RegularExpressions;
using CefSharp;
using System.Windows.Forms;

namespace ctc
{
    public class StringCondition: ClaimToolCommand
    {
        private static readonly StringCondition Self = new StringCondition();

        private StringCondition()
        {
        }
        public bool Run(string fullcommand)
        {
            //MessageBox.Show(fullcommand);
            bool ok = false;
            MatchCollection coll = Regex.Matches(fullcommand, "^string\\('([^']+)'=='([^']+)'\\)");
            string javascript = coll[0].Groups[1].Value;
            string bieuthuc = coll[0].Groups[2].Value;
            //MessageBox.Show(javascript +" "+ bieuthuc);
            string result = EvaluateJavascript.Instance().Run(javascript);
            //MessageBox.Show("result " + result);
            if (result!=null && Regex.IsMatch(result, bieuthuc))
                ok = true;
            return ok;
        }
        public static StringCondition Instance()
        {
            return Self;
        }
    }
}
