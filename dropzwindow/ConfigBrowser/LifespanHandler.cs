using System;
using System.Threading;
using System.Windows.Forms;
using CefSharp;

namespace dropzwindow
{
    public class LifespanHandler : ILifeSpanHandler
    {
        //event that receive url popup
        public event Action popup_request;
        public Form form;
        bool ILifeSpanHandler.OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            form = new Form();
            if (Info.Setting.HidePopup)
            {
                form.Activate();
            }
            else
            {
                form.Width = Info.Setting.Width;
                form.Height = Info.Setting.Height;
                form.Show();
            }
            windowInfo.SetAsChild(form.Handle, 0, 0, 0, 0);
            windowInfo.Width = Info.Setting.Width;
            windowInfo.Height = Info.Setting.Height;
            newBrowser = null;
            return false;
        }
        bool ILifeSpanHandler.DoClose(IWebBrowser browserControl, IBrowser browser)
        { return false; }

        void ILifeSpanHandler.OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
        {

        }

        void ILifeSpanHandler.OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
        {
            Thread t = new Thread(() =>
            {
                if (browser.IsPopup)
                {
                    Thread.Sleep(Info.Setting.DelayClosePopup);
                    browser.CloseBrowser(true);
                }
            });
            t.Start();
        }
    }
}
