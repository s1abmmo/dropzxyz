using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Reflection;

namespace dropzwindow.HandleScript
{
    public class HandleScript
    {
        public static void ReplaceVariable()
        {
            try
            {

                foreach (dynamic entry in Info.AutoParameters)
                {
                    string name = "{" + entry.Name + "}";
                    string value = entry.Value;
                    //MessageBox.Show(name+ value);
                    Info.AutoScript.Script=Info.AutoScript.Script.Replace(name, value);
                }

            }
            catch (Exception e) { //MessageBox.Show(e.Message);
            }
        }
        public static bool CheckScriptInvalid()
        {
            bool invalid = false;
            if (!Regex.IsMatch(Info.AutoScript.Script, "{|}"))
                invalid = true;
            return invalid;
        }
    }
}
