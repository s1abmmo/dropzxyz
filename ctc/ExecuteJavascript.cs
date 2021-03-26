using System.Text.RegularExpressions;

namespace ctc
{
    public class ExecuteJavascript:ClaimToolCommand
    {
        private static readonly ExecuteJavascript Self = new ExecuteJavascript();

        private ExecuteJavascript()
        {
        }
        public bool Run(string fullcommand)
        {
            MatchCollection coll = Regex.Matches(fullcommand, "^javascript\\('([^']+)'\\)");
            string javascript = coll[0].Groups[1].Value;
            EvaluateJavascript.Instance().Run(javascript);
            return true;
        }
        public static ExecuteJavascript Instance()
        {
            return Self;
        }
    }
}
