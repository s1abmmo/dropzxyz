using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace main
{
    /// <summary>
    /// Interaction logic for CaptchaSolver.xaml
    /// </summary>
    public partial class CaptchaSolver : Window
    {
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const short SWP_NOMOVE = 0X2;
        //const short SWP_NOSIZE = 1;
        //const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;
        const int SWP_HIDEWINDOW = 0x0080;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

        public CaptchaSolver()
        {
            InitializeComponent();
            this.DataContext = MainWindow.captchasolverstatus;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (MainWindow.captchasolverstatus.Running)
            {
                MainWindow.captchasolverstatus.Running = false;
                this.DataContext = null;
                this.DataContext = MainWindow.captchasolverstatus;
            }
            else
            {
                MainWindow.captchasolverstatus.Running = true;
                this.DataContext = null;
                this.DataContext = MainWindow.captchasolverstatus;

                Thread mainThread = new Thread(() =>
                {
                    while (MainWindow.captchasolverstatus.Running)
                    {
                        Thread.Sleep(1000);
                        //MessageBox.Show(Process.GetProcessById(MainWindow.items[0].IdProcess).MainWindowHandle.ToString());
                        if (MainWindow.captchasolverstatus.Next)
                        {
                            MainWindow.captchasolverstatus.Next = false;
                            //MessageBox.Show("ok2");
                            if (LastDropzWindow.Hwnd != IntPtr.Zero)
                            {
                                SetParent(LastDropzWindow.Hwnd, IntPtr.Zero);
                                if(LastDropzWindow.Visible)
                                SetWindowPos(LastDropzWindow.Hwnd, 0, LastDropzWindow.Left, LastDropzWindow.Top, 0, 0, SWP_SHOWWINDOW);
                                else SetWindowPos(LastDropzWindow.Hwnd, 0, LastDropzWindow.Left, LastDropzWindow.Top, 0, 0, SWP_HIDEWINDOW);
                                MoveWindow(LastDropzWindow.Hwnd, LastDropzWindow.Left, LastDropzWindow.Top, LastDropzWindow.Width, LastDropzWindow.Height, true);

                                LastDropzWindow.Hwnd = IntPtr.Zero;
                            }
                            List<DropzWindow> listcaptcha = ListItem.ListItem.items.FindAll(item => item.Captcha == true && item.AutoRunning == true && item.Start == true);
                            if (listcaptcha.Count > 0)
                            {
                                DropzWindow min = listcaptcha[0];
                                foreach (DropzWindow item in listcaptcha)
                                {
                                    if (item.CaptchaTime < min.CaptchaTime)
                                        min = item;
                                }

                                DropzWindow minItem = ListItem.ListItem.items.Find(item => item.Id == min.Id);

                                LastDropzWindow.Id = minItem.Id;
                                LastDropzWindow.Hwnd = minItem.Hwnd;

                                RECT rct;
                                if (GetWindowRect(new HandleRef(this, LastDropzWindow.Hwnd), out rct))
                                {
                                    LastDropzWindow.Width= rct.Right - rct.Left;
                                    LastDropzWindow.Height= rct.Bottom - rct.Top;
                                    LastDropzWindow.Top= rct.Top;
                                    LastDropzWindow.Left= rct.Left;
                                    LastDropzWindow.Visible = IsWindowVisible(LastDropzWindow.Hwnd);
                                }

                                ListItem.ListItem.items.Find(item => item == min).CommandSend = "clickcaptcha|400|550";
                                //MessageBox.Show(ListItem.ListItem.items[LastDropzWindow.Index].Script.ScriptName);
                                //IntPtr pr = Process.GetProcessById(ListItem.ListItem.items[LastDropzWindow.Index].IdProcess).MainWindowHandle;
                                SetParent(minItem.Hwnd, main.Handle);
                                SetWindowPos(minItem.Hwnd, 0, -10, -30, 0, 0,SWP_SHOWWINDOW);
                            }
                        }
                    }
                });
                mainThread.Start();

            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow.captchasolverstatus.Next = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MainWindow.captchasolverstatus.Running = false;
            this.Close();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();

        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.captchasolverstatus.CheckCaptchaRunning = true;
            MainWindow.captchasolverstatus.CheckCaptcha = new Thread(() => {
                while (true)
                {
                    Thread.Sleep(1000);
                    if (LastDropzWindow.Hwnd == null || LastDropzWindow.Hwnd == IntPtr.Zero)
                        if (ListItem.ListItem.items.FindAll(item => item.Captcha == true && item.AutoRunning == true && item.Start == true).Count > 0)
                            MainWindow.captchasolverstatus.Next = true;
                }
            });
            MainWindow.captchasolverstatus.CheckCaptcha.Start();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow.captchasolverstatus.CheckCaptchaRunning = false;
            try
            {
                MainWindow.captchasolverstatus.CheckCaptcha.Abort();
            }
            catch { }
        }
    }
    class LastDropzWindow
    {
        public static int Id;
        public static IntPtr Hwnd;
        public static int Width;
        public static int Height;
        public static int Top;
        public static int Left;
        public static bool Visible;
    }
    public class CaptchaSolverStatus
    {
        public bool Running { get; set; }
        public bool Next { get; set; }
        public string Content { get; set; }
        public Thread CheckCaptcha { get; set; }
        public bool CheckCaptchaRunning { get; set; }
    }
}

