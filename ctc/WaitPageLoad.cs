using System.Threading;

namespace ctc
{
    public class WaitPageLoad: ClaimToolCommand
    {
        private static readonly WaitPageLoad Self = new WaitPageLoad();

        private WaitPageLoad()
        {
        }
        public bool Run(string nothing)
        {
            while (true)
            {
                Thread.Sleep(500);
                if (!Browser.ChromeBrowser.IsLoading)
                    break;
            }
            return true;
        }
        public static WaitPageLoad Instance()
        {
            return Self;
        }
    }
}
