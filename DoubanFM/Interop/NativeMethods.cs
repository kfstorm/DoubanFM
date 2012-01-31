using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DoubanFM.Interop
{
	internal static class NativeMethods
	{
		#region HotKey
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint controlKey, uint virtualKey);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
		#endregion

		#region Hook
		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr SetWindowsHookEx(WH idHook, HOOKPROC lpfn, IntPtr hMod, int dwThreadId);
		[DllImport("user32.dll")]
		internal static extern int UnhookWindowsHookEx(IntPtr hook);
		[DllImport("user32.dll")]
		internal static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);
		#endregion

		#region Window
		[DllImport("user32.dll")]
		internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
		[DllImport("user32.dll", SetLastError = true)]
		internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SWP uFlags);
		#endregion

		#region Region
		[DllImport("Gdi32.dll")]
		internal static extern IntPtr CreateRectRgn([In] int nLeftRect, [In] int nTopRect, [In] int nRightRect, [In] int nBottomRect);
		[DllImport("Gdi32.dll")]
		internal static extern bool DeleteObject([In] IntPtr hObject);
		#endregion

		#region Desktop Window Manager
		[DllImport("dwmapi.dll", PreserveSig = false)]
		internal static extern void DwmEnableBlurBehindWindow(IntPtr hWnd, ref DWM_BLURBEHIND pBlurBehind);
		[DllImport("dwmapi.dll", PreserveSig = false)]
		internal static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMargins);
		[DllImport("dwmapi.dll", PreserveSig = false)]
		internal static extern bool DwmIsCompositionEnabled();
		[DllImport("dwmapi.dll", PreserveSig = false)]
		internal static extern void DwmGetColorizationColor(out int pcrColorization, [MarshalAs(UnmanagedType.Bool)]out bool pfOpaqueBlend);
		[DllImport("dwmapi.dll", PreserveSig = false)]
		internal static extern void DwmEnableComposition(bool bEnable);
		[DllImport("dwmapi.dll", PreserveSig = false)]
		internal static extern IntPtr DwmRegisterThumbnail(IntPtr dest, IntPtr source);
		[DllImport("dwmapi.dll", PreserveSig = false)]
		internal static extern void DwmUnregisterThumbnail(IntPtr hThumbnail);
		[DllImport("dwmapi.dll", PreserveSig = false)]
		internal static extern void DwmUpdateThumbnailProperties(IntPtr hThumbnail, ref DWM_THUMBNAIL_PROPERTIES props);
		[DllImport("dwmapi.dll", PreserveSig = false)]
		internal static extern void DwmQueryThumbnailSourceSize(IntPtr hThumbnail, out System.Windows.Size size);
		#endregion

		#region Others
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern IntPtr GetModuleHandle(string lpModuleName);
		#endregion
	}
}
