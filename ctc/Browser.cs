using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.WinForms;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace ctc
{
    public class Browser
    {
        private static readonly Dictionary<string, ClaimToolCommand> Handlers = new Dictionary<string, ClaimToolCommand>
        {
            { "url",Url.Instance()},
            { "checkvisible",CheckVisible.Instance()},
            { "int",IntCondition.Instance()},
            { "string",StringCondition.Instance()},
            { "input",Input.Instance()},
            { "delay",Delay.Instance()},
            { "javascript",ExecuteJavascript.Instance()},
            { "waitload",WaitPageLoad.Instance()},
            { "click",Click.Instance()},
        };
        public static ChromiumWebBrowser ChromeBrowser { get; set; }
        public static bool HandleAutoCommand(string claimtoolcommand)
        {
            MatchCollection coll = Regex.Matches(claimtoolcommand, "^([a-z]+)\\(");
            string namecommand = coll[0].Groups[1].Value;
            //MessageBox.Show("claimtoolcommand: " + claimtoolcommand+Environment.NewLine+"name command: " +namecommand);
            bool result=Handlers[namecommand].Run(claimtoolcommand);
            return result;
        }

    }

    //public class HandleAutoCommand
    //{
    //    public bool Url(ChromiumWebBrowser browser, string url)
    //    {
    //        browser.Load(url);
    //        while (true)
    //        {
    //            Thread.Sleep(500);
    //            if (!browser.IsLoading)
    //                break;
    //        }
    //        return true;
    //    }
    //    public bool CheckVisible(ChromiumWebBrowser browser, string element)
    //    {
    //        bool Visible = false;
    //        int width = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript(browser, "document.querySelector('" + element + "').getBoundingClientRect().width;")));
    //        int height = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript(browser, "document.querySelector('" + element + "').getBoundingClientRect().height;")));
    //        if (width != 0 && height != 0)
    //        {
    //            Visible = true;
    //        }
    //        return Visible;
    //    }
    //    public bool Click(ChromiumWebBrowser browser, string element, int width, int height)
    //    {
    //        bool success = false;
    //        if (CheckVisible(browser, element))
    //        {
    //            browser.EvaluateScriptAsync("document.querySelector('" + element + "').scrollIntoView();");
    //            Thread.Sleep(100);
    //            browser.EvaluateScriptAsync("window.scrollBy(0, -" + (browser.Height / 2).ToString() + ");");
    //            Thread.Sleep(500);
    //            int x = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript(browser, "document.querySelector('" + element + "').getBoundingClientRect().x;")));
    //            int y = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript(browser, "document.querySelector('" + element + "').getBoundingClientRect().y;")));
    //            if (x != 0 && y != 0)
    //            {
    //                browser.GetBrowser().GetHost().SendMouseClickEvent(x + width, y + height, MouseButtonType.Left, false, 1, CefEventFlags.None);
    //                Thread.Sleep(100);
    //                browser.GetBrowser().GetHost().SendMouseClickEvent(x + width, y + height, MouseButtonType.Left, true, 1, CefEventFlags.None);
    //                success = true;
    //            }
    //        }
    //        return success;
    //    }
    //    public bool WaitPageLoad(ChromiumWebBrowser browser)
    //    {
    //        while (true)
    //        {
    //            Thread.Sleep(500);
    //            if (!browser.IsLoading)
    //                break;
    //        }
    //        return true;
    //    }
    //    public bool IntCondition(ChromiumWebBrowser browser, string command)
    //    {
    //        bool ok = false;
    //        MatchCollection coll = Regex.Matches(command, "^int\\('([^']+)'\\)([>=<]{1,2})(\\d+)");
    //        string javascript = coll[0].Groups[1].Value;
    //        string bieuthuc = coll[0].Groups[2].Value;
    //        int so = Convert.ToInt32(coll[0].Groups[3].Value);
    //        int so2 = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript(browser, javascript)));
    //        switch (bieuthuc)
    //        {
    //            case ">":
    //                if (so2 > so)
    //                    ok = true;
    //                break;
    //            case ">=":
    //                if (so2 >= so)
    //                    ok = true;
    //                break;
    //            case "==":
    //                if (so2 == so)
    //                    ok = true;
    //                break;
    //            case "<=":
    //                if (so2 <= so)
    //                    ok = true;
    //                break;
    //            case "<":
    //                if (so2 < so)
    //                    ok = true;
    //                break;
    //            default:
    //                break;
    //        }
    //        return ok;
    //    }

    //    public bool StringCondition(ChromiumWebBrowser browser, string command)
    //    {
    //        bool ok = false;
    //        string[] command1 = command.Split('\'');
    //        if (Regex.IsMatch(EvaluateJavascript(browser, command1[1]), command1[3]))
    //            ok = true;
    //        return ok;
    //    }

    //    public string EvaluateJavascript(ChromiumWebBrowser browser, string javascript)
    //    {
    //        string result = null;
    //        browser.EvaluateScriptAsync(javascript).ContinueWith(x =>
    //        {
    //            var response = x.Result;

    //            if (response.Success && response.Result != null)
    //            {
    //                var startDate = response.Result;
    //                result = startDate.ToString();
    //            }
    //        }).Wait();
    //        return result;
    //    }

    //    public bool Input(ChromiumWebBrowser browser, string element, int left, int top, string input, int delay)
    //    {
    //        bool success = false;

    //        success = Click(browser, element, left, top);
    //        if (success)
    //        {
    //            Thread.Sleep(500);
    //            char[] chararrayinput = input.ToCharArray();

    //            foreach (char ch in chararrayinput)
    //            {
    //                Thread.Sleep(delay);
    //                KeyEvent k = new KeyEvent();
    //                k.WindowsKeyCode = (int)ch;
    //                k.FocusOnEditableField = true;
    //                k.IsSystemKey = false;
    //                k.Type = KeyEventType.Char;

    //                browser.GetBrowser().GetHost().SendKeyEvent(k);

    //            }

    //        }

    //        return success;
    //    }

    //    public static Keys ConvertCharToVirtualKey(char ch)
    //    {
    //        short vkey = VkKeyScan(ch);
    //        Keys retval = (Keys)(vkey & 0xff);
    //        int modifiers = vkey >> 8;

    //        if ((modifiers & 1) != 0) retval |= Keys.Shift;
    //        if ((modifiers & 2) != 0) retval |= Keys.Control;
    //        if ((modifiers & 4) != 0) retval |= Keys.Alt;

    //        return retval;
    //    }

    //    [DllImport("user32.dll")]
    //    private static extern short VkKeyScan(char ch);

    //}

}
