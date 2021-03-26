using System;
using System.Text.RegularExpressions;
using System.Threading;
using CefSharp;
using CefSharp.WinForms;
using ctc;
using dropzwindow.ConfigBrowser;
using System.Windows.Forms;

namespace dropzwindow.Command
{
    public class HandleCommand
    {
        public static void Auto(int widthbrowser, int heightbrowser, bool visible)
        {
            Info.Setting.Width = widthbrowser;
            Info.Setting.Height = heightbrowser;
            //Info.Setting.Top = this.Top;
            //Info.Setting.Left = this.Left;
            Info.Setting.Visible = visible;
            string[] AutoScriptRow = Info.AutoScript.Script.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            while (true)
            {
                foreach (string row in AutoScriptRow)
                {
                    string[] column = row.Split('|');
                    foreach (string command in column)
                    {

                        WaitPageLoad.Instance().Run(null);

                        try {
                            if (Regex.IsMatch(command, "^getdata"))
                            {
                                MatchCollection coll = Regex.Matches(command, "^getdata\\('([^']+)'\\)");
                                string data = coll[0].Groups[1].Value;

                                InfomationStartup.Response = EvaluateJavascript.Instance().Run(data);
                            }else if (Regex.IsMatch(command, "^checkcaptcha"))
                            {
                                MatchCollection coll = Regex.Matches(command, "^checkcaptcha\\('([^']+)','([^']+)'\\)");
                                string detectecaptcha = coll[0].Groups[1].Value;
                                string clickcaptchatosolve = coll[0].Groups[2].Value;
                                //MessageBox.Show(detectecaptcha + " " + clickcaptchatosolve + " " + detectecapchasolved);
                                if (CheckVisible.Instance().Run("checkvisible('"+ detectecaptcha + "')" ))
                                {
                                    //MessageBox.Show(detectecaptcha + " " + clickcaptchatosolve);
                                    InfomationStartup.Captcha = true;
                                    while (true)
                                    {
                                        try
                                        {
                                            Thread.Sleep(1000);
                                            if (InfomationStartup.ClickRecaptcha)
                                            {
                                                Click.Instance().Run("click('"+ clickcaptchatosolve + "',20,20)");
                                                InfomationStartup.ClickRecaptcha = false;
                                            }
                                            if (StringCondition.Instance().Run("string('document.querySelector(\""+ clickcaptchatosolve + "\").getAttribute(\"data-hcaptcha-response\")==\"\";'=='False')"))
                                                break;
                                        }
                                        catch (Exception e) { MessageBox.Show(e.Message); }
                                    }
                                    InfomationStartup.Captcha = false;
                                }
                            }
                            else if (!Browser.HandleAutoCommand(command))
                                break;
                        }
                        catch(Exception e) { MessageBox.Show("command:"+ command + " : "+ e.Message);
                        }

                    }
                }
            }
        }

    }
}
