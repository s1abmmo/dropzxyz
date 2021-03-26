using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace ctc
{
    public class Delay:ClaimToolCommand
    {
        private static readonly Delay Self = new Delay();

        private Delay()
        {
        }
        public bool Run(string fullcommand)
        {
            MatchCollection coll = Regex.Matches(fullcommand, "^delay\\((\\d)+\\)");
            int delay =Convert.ToInt32(coll[0].Groups[1].Value);
            Thread.Sleep(delay);
            return true;
        }
        public static Delay Instance()
        {
            return Self;
        }
    }
}
