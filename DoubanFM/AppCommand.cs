using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DoubanFM
{
    public class AppCommand : IDisposable
    {
        public class AppCommandEventArgs : EventArgs
        {
            public Command Command { get; private set; }
            public Device Device { get; private set; }
            public Keys Keys { get; private set; }
            public bool Handled { get; set; }

            public AppCommandEventArgs(Command command, Device device, Keys keys)
            {
                Command = command;
                Device = device;
                Keys = keys;
            }
        }

        public delegate void AppCommndEventhandler(object sender, AppCommandEventArgs e);

        public event AppCommndEventhandler Fire;

        protected virtual void OnFire(AppCommandEventArgs e)
        {
            AppCommndEventhandler handler = Fire;
            if (handler != null) handler(this, e);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DeregisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint RegisterWindowMessage(string lpString);

        private static int WM_SHELLHOOKMESSAGE;
        private static readonly IntPtr HSHELL_APPCOMMAND = new IntPtr(12);
        private const uint FAPPCOMMAND_MASK = 0xF000;

        private IntPtr hWnd;
        private HwndSource source;
        private bool disposed = false;

        public AppCommand(IntPtr hWnd)
        {
            this.hWnd = hWnd;
        }

        public AppCommand(Window window)
            : this(new WindowInteropHelper(window).EnsureHandle())
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                }

                Stop();
                hWnd = IntPtr.Zero;

                disposed = true;

            }
        }

        ~AppCommand()
        {
            Dispose(false);
        }


        public void Start()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            if (source == null)
            {
                source = HwndSource.FromHwnd(hWnd);
                if (source == null)
                {
                    throw new InvalidOperationException("hWnd is invalid.");
                }
                source.AddHook(WndProc);
                WM_SHELLHOOKMESSAGE = (int) RegisterWindowMessage("SHELLHOOK");
                if (WM_SHELLHOOKMESSAGE == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error, "Register window message 'SHELLHOOK' failed.");
                }
                if (!RegisterShellHookWindow(hWnd))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error, "Call RegisterShellHookWindow failed.");
                }
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SHELLHOOKMESSAGE && wParam == HSHELL_APPCOMMAND)
            {
                var command = GetAppCommandLParam(lParam);
                var device = GetDeviceLParam(lParam);
                var keys = GetKeyStateLParam(lParam);

                var e = new AppCommandEventArgs(command, device, keys);
                OnFire(e);
                handled = e.Handled;
            }
            return IntPtr.Zero;
        }

        protected static Command GetAppCommandLParam(IntPtr lParam)
        {
            return (Command) ((short) (((ushort) ((((uint) lParam.ToInt64()) >> 16) & 0xffff)) & ~FAPPCOMMAND_MASK));
        }

        protected static Device GetDeviceLParam(IntPtr lParam)
        {
            return (Device) ((ushort) (((ushort) ((((uint) lParam.ToInt64()) >> 16) & 0xffff)) & FAPPCOMMAND_MASK));
        }

        protected static Keys GetKeyStateLParam(IntPtr lParam)
        {
            return (Keys) ((ushort) (((uint) lParam.ToInt64()) & 0xffff));
        }

        public void Stop()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            if (source != null)
            {
                source.RemoveHook(WndProc);
                if (!source.IsDisposed)
                {
                    if (!DeregisterShellHookWindow(hWnd))
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw new Win32Exception(error, "Call DeregisterShellHookWindow failed.");
                    }
                    source.Dispose();
                }
                source = null;
            }
        }

        public enum Command
        {
            APPCOMMAND_BASS_BOOST = 20,
            APPCOMMAND_BASS_DOWN = 19,
            APPCOMMAND_BASS_UP = 21,
            APPCOMMAND_BROWSER_BACKWARD = 1,
            APPCOMMAND_BROWSER_FAVORITES = 6,
            APPCOMMAND_BROWSER_FORWARD = 2,
            APPCOMMAND_BROWSER_HOME = 7,
            APPCOMMAND_BROWSER_REFRESH = 3,
            APPCOMMAND_BROWSER_SEARCH = 5,
            APPCOMMAND_BROWSER_STOP = 4,
            APPCOMMAND_CLOSE = 31,
            APPCOMMAND_COPY = 36,
            APPCOMMAND_CORRECTION_LIST = 45,
            APPCOMMAND_CUT = 37,
            APPCOMMAND_DICTATE_OR_COMMAND_CONTROL_TOGGLE = 43,
            APPCOMMAND_FIND = 28,
            APPCOMMAND_FORWARD_MAIL = 40,
            APPCOMMAND_HELP = 27,
            APPCOMMAND_LAUNCH_APP1 = 17,
            APPCOMMAND_LAUNCH_APP2 = 18,
            APPCOMMAND_LAUNCH_MAIL = 15,
            APPCOMMAND_LAUNCH_MEDIA_SELECT = 16,
            APPCOMMAND_MEDIA_CHANNEL_DOWN = 52,
            APPCOMMAND_MEDIA_CHANNEL_UP = 51,
            APPCOMMAND_MEDIA_FAST_FORWARD = 49,
            APPCOMMAND_MEDIA_NEXTTRACK = 11,
            APPCOMMAND_MEDIA_PAUSE = 47,
            APPCOMMAND_MEDIA_PLAY = 46,
            APPCOMMAND_MEDIA_PLAY_PAUSE = 14,
            APPCOMMAND_MEDIA_PREVIOUSTRACK = 12,
            APPCOMMAND_MEDIA_RECORD = 48,
            APPCOMMAND_MEDIA_REWIND = 50,
            APPCOMMAND_MEDIA_STOP = 13,
            APPCOMMAND_MIC_ON_OFF_TOGGLE = 44,
            APPCOMMAND_MICROPHONE_VOLUME_DOWN = 25,
            APPCOMMAND_MICROPHONE_VOLUME_MUTE = 24,
            APPCOMMAND_MICROPHONE_VOLUME_UP = 26,
            APPCOMMAND_NEW = 29,
            APPCOMMAND_OPEN = 30,
            APPCOMMAND_PASTE = 38,
            APPCOMMAND_PRINT = 33,
            APPCOMMAND_REDO = 35,
            APPCOMMAND_REPLY_TO_MAIL = 39,
            APPCOMMAND_SAVE = 32,
            APPCOMMAND_SEND_MAIL = 41,
            APPCOMMAND_SPELL_CHECK = 42,
            APPCOMMAND_TREBLE_DOWN = 22,
            APPCOMMAND_TREBLE_UP = 23,
            APPCOMMAND_UNDO = 34,
            APPCOMMAND_VOLUME_DOWN = 9,
            APPCOMMAND_VOLUME_MUTE = 8,
            APPCOMMAND_VOLUME_UP = 10
        }

        public enum Device
        {
            FAPPCOMMAND_KEY = 0,
            FAPPCOMMAND_MOUSE = 0x8000,
            FAPPCOMMAND_OEM = 0x1000
        }

        [Flags]
        public enum Keys
        {
            MK_CONTROL = 0x0008,
            MK_LBUTTON = 0x0001,
            MK_MBUTTON = 0x0010,
            MK_RBUTTON = 0x0002,
            MK_SHIFT = 0x0004,
            MK_XBUTTON1 = 0x0020,
            MK_XBUTTON2 = 0x0040
        }
    }
}