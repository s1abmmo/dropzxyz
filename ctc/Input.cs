using System;
using System.Text.RegularExpressions;
using System.Threading;
using CefSharp;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ctc
{
    public class Input: ClaimToolCommand
    {
        private static readonly Input Self = new Input();

        private Input()
        {
        }
        public bool Run(string fullcommand)
        {
            MatchCollection coll = Regex.Matches(fullcommand, "^input\\('([^']+)',(\\d)+,(\\d)+,([^,]),(\\d)+\\)");
            string element = coll[0].Groups[1].Value;
            int left =Convert.ToInt32(coll[0].Groups[2].Value);
            int top = Convert.ToInt32(coll[0].Groups[3].Value);
            string text = coll[0].Groups[4].Value;
            int delay = Convert.ToInt32(coll[0].Groups[5].Value);

            bool success = false;

            success = Click.Instance().Run("click('"+element+"',"+ left .ToString()+ ","+top.ToString()+")");
            if (success)
            {
                Thread.Sleep(500);
                char[] chararrayinput = text.ToCharArray();

                foreach (char ch in chararrayinput)
                {
                    Thread.Sleep(delay);
                    KeyEvent k = new KeyEvent();
                    k.WindowsKeyCode = (int)ch;
                    k.FocusOnEditableField = true;
                    k.IsSystemKey = false;
                    k.Type = KeyEventType.Char;

                    Browser.ChromeBrowser.GetBrowser().GetHost().SendKeyEvent(k);
                }

            }
            return success;
        }
        public static Input Instance()
        {
            return Self;
        }
        public static Keys ConvertCharToVirtualKey(char ch)
        {
            short vkey = VkKeyScan(ch);
            Keys retval = (Keys)(vkey & 0xff);
            int modifiers = vkey >> 8;

            if ((modifiers & 1) != 0) retval |= Keys.Shift;
            if ((modifiers & 2) != 0) retval |= Keys.Control;
            if ((modifiers & 4) != 0) retval |= Keys.Alt;

            return retval;
        }

        [DllImport("user32.dll")]
        private static extern short VkKeyScan(char ch);
    }
}
