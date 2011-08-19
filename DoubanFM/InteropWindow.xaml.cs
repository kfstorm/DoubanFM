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
using DoubanFM.Core;

namespace DoubanFM
{
    /// <summary>
    /// InteropWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InteropWindow : Window
    {
        /// <summary>
        /// 使用COPYDATA，WM_USER只能用于应用程序内部的通讯，跨进程用COPYDATA
        /// </summary>
        public const int WM_COPYDATA = 0x004A;

        public InteropWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 查找目标发送窗体
        /// </summary>
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [StructLayout(LayoutKind.Sequential)]
        public struct CopyDataStruct
        {
            public IntPtr dwData;
            public int cbData;//字符串长度
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;//字符串
        }
        /// <summary>
        /// 发送消息方法
        /// </summary>
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage
        (
        IntPtr hWnd,                   //目标窗体句柄
        int Msg,                       //WM_COPYDATA
        int wParam,                                             //自定义数值
        ref  CopyDataStruct lParam             //结构体
        );
        /// <summary>
        /// SendMessage To Window
        /// </summary>
        /// <param name="windowName">window的title，建议加上GUID，不会重复</param>
        /// <param name="strMsg">要发送的字符串</param>
        public static void SendMessage(string windowName, string strMsg)
        {
            if (strMsg == null) return;
            IntPtr hwnd = FindWindow(null, windowName);
            if (hwnd != IntPtr.Zero)
            {
                CopyDataStruct cds;
                cds.dwData = IntPtr.Zero;
                cds.lpData = strMsg;
                //注意：长度为字节数
                cds.cbData = System.Text.Encoding.Default.GetBytes(strMsg).Length + 1;
                // 消息来源窗体
                int fromWindowHandler = 0;
                SendMessage(hwnd, WM_COPYDATA, fromWindowHandler, ref  cds);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (PresentationSource.FromVisual(this) as HwndSource).AddHook(new HwndSourceHook(this.WndProc));
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_COPYDATA)
            {
                CopyDataStruct cds = (CopyDataStruct)System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(CopyDataStruct));
                List<string> commandLineArgs = new List<string>();
                bool haveQuote = false;
                StringBuilder sb = new StringBuilder();
                foreach (char c in cds.lpData)
                {
                    if (char.IsWhiteSpace(c) && haveQuote == false)
                    {
                        if (sb.Length > 0)
                        {
                            commandLineArgs.Add(sb.ToString());
                            sb.Clear();
                        }
                    }
                    else if (c == '\"')
                        if (haveQuote)
                        {
                            commandLineArgs.Add(sb.ToString());
                            sb.Clear();
                            haveQuote = false;
                        }
                        else haveQuote = true;
                    else sb.Append(c);
                }
                if (sb.Length > 0)
                    commandLineArgs.Add(sb.ToString());
                Channel channel = Channel.FromCommandLineArgs(commandLineArgs);
                if (channel != null)
                    (App.Current.MainWindow as DoubanFMWindow).InteropChangeChannel(channel);
            }
            return hwnd;
        }
    }
}
