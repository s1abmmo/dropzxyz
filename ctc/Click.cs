using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CefSharp;
using System.Windows.Forms;

namespace ctc
{
    public class Click: ClaimToolCommand
    {
        private static readonly Click Self = new Click();

        private Click()
        {
        }
        public bool Run(string fullcommand)
        {
            Match match = Regex.Match(fullcommand, "click\\('([^']+)',(\\d+),(\\d+)\\)");
            string element = match.Groups[1].Value;
            int left =Convert.ToInt32(match.Groups[2].Value);
            int top = Convert.ToInt32(match.Groups[3].Value);
            //MessageBox.Show(element + left.ToString() + top.ToString());
            bool success = false;
            if (CheckVisible.Instance().Run("checkvisible('"+ element + "')"))
            {
                Browser.ChromeBrowser.EvaluateScriptAsync("document.querySelector('" + element + "').scrollIntoView();");
                Thread.Sleep(100);
                Browser.ChromeBrowser.EvaluateScriptAsync("window.scrollBy(0, -" + (Browser.ChromeBrowser.Height / 2).ToString() + ");");
                Thread.Sleep(500);
                int x = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript.Instance().Run("document.querySelector('" + element + "').getBoundingClientRect().x;")));
                int y = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript.Instance().Run("document.querySelector('" + element + "').getBoundingClientRect().y;")));
                if (x != 0 && y != 0)
                {
                    Browser.ChromeBrowser.GetBrowser().GetHost().SendMouseClickEvent(x + left, y + top, MouseButtonType.Left, false, 1, CefEventFlags.None);
                    Thread.Sleep(100);
                    Browser.ChromeBrowser.GetBrowser().GetHost().SendMouseClickEvent(x + left, y + top, MouseButtonType.Left, true, 1, CefEventFlags.None);
                    success = true;
                }
            }
            //MessageBox.Show("click success:" + success.ToString());
            return success;
        }
        public static Click Instance()
        {
            return Self;
        }
    }
}
