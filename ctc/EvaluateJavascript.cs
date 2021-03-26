using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using System.Windows.Forms;

namespace ctc
{
    public class EvaluateJavascript
    {
        private static readonly EvaluateJavascript Self = new EvaluateJavascript();

        private EvaluateJavascript()
        {
        }
        public string Run(string javascript)
        {
            string result = null;
            Browser.ChromeBrowser.EvaluateScriptAsync(javascript).ContinueWith(x =>
            {
                var response = x.Result;

                if (response.Success && response.Result != null)
                {
                    var startDate = response.Result;
                    result = startDate.ToString();
                }
            }).Wait();
            return result;
        }
        public static EvaluateJavascript Instance()
        {
            return Self;
        }
    }
}
