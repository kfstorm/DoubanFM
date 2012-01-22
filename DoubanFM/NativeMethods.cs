using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DoubanFM
{
	public static class NativeMethods
	{
		#region HotKey
		[DllImport("user32.dll")]
		internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint controlKey, uint virtualKey);
		[DllImport("user32.dll")]
		internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
		#endregion

		#region Hook
		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr SetWindowsHookEx(HookType code, HookProc func, IntPtr instance, int threadID);
		[DllImport("user32.dll")]
		internal static extern int UnhookWindowsHookEx(IntPtr hook);
		[DllImport("user32.dll")]
		internal static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

		internal enum HookType : int
		{
			WH_JOURNALRECORD = 0,
			WH_JOURNALPLAYBACK = 1,
			WH_KEYBOARD = 2,
			WH_GETMESSAGE = 3,
			WH_CALLWNDPROC = 4,
			WH_CBT = 5,
			WH_SYSMSGFILTER = 6,
			WH_MOUSE = 7,
			WH_HARDWARE = 8,
			WH_DEBUG = 9,
			WH_SHELL = 10,
			WH_FOREGROUNDIDLE = 11,
			WH_CALLWNDPROCRET = 12,
			WH_KEYBOARD_LL = 13,
			WH_MOUSE_LL = 14
		}

		internal struct KBDLLHOOKSTRUCT
		{
			public UInt32 vkCode;
			public UInt32 scanCode;
			public UInt32 flags;
			public UInt32 time;
			public IntPtr extraInfo;
		}

		// hook method called by system
		internal delegate IntPtr HookProc(int code, IntPtr wParam, ref NativeMethods.KBDLLHOOKSTRUCT lParam);
		#endregion

		#region Window
		internal const int GWL_EXSTYLE = (-20);
		internal const int WS_EX_TRANSPARENT = 0x0020;
		internal const int WS_EX_TOOLWINDOW = 0x00000080;
		[DllImport("user32.dll")]
		internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
		[DllImport("user32.dll", SetLastError = true)]
		internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		#endregion

		#region Region

		[DllImport("Gdi32.dll")]
		internal static extern IntPtr CreateRectRgn([In] int nLeftRect, [In] int nTopRect, [In] int nRightRect, [In] int nBottomRect);
		[DllImport("Gdi32.dll")]
		internal static extern bool DeleteObject([In] IntPtr hObject);
		#endregion

		#region Others
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern IntPtr GetModuleHandle(string lpModuleName);
		#endregion
	}
}
