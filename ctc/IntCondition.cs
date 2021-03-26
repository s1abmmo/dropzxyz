using System;
using System.Text.RegularExpressions;
using CefSharp;

namespace ctc
{
    public class IntCondition: ClaimToolCommand
    {
        private static readonly IntCondition Self = new IntCondition();

        private IntCondition()
        {
        }
        public bool Run(string fullcommand)
        {
            bool ok = false;
            MatchCollection coll = Regex.Matches(fullcommand, "^int\\('([^']+)'\\)([>=<]{1,2})(\\d+)");
            string javascript = coll[0].Groups[1].Value;
            string bieuthuc = coll[0].Groups[2].Value;
            int so = Convert.ToInt32(coll[0].Groups[3].Value);
            int so2 = Convert.ToInt32(Convert.ToDecimal(EvaluateJavascript.Instance().Run(javascript)));
            switch (bieuthuc)
            {
                case ">":
                    if (so2 > so)
                        ok = true;
                    break;
                case ">=":
                    if (so2 >= so)
                        ok = true;
                    break;
                case "==":
                    if (so2 == so)
                        ok = true;
                    break;
                case "<=":
                    if (so2 <= so)
                        ok = true;
                    break;
                case "<":
                    if (so2 < so)
                        ok = true;
                    break;
                default:
                    break;
            }
            return ok;
        }
        public static IntCondition Instance()
        {
            return Self;
        }
    }
}
