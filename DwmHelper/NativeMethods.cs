using System;
using System.Runtime.InteropServices;

namespace Kfstorm.DwmHelper
{
    internal class NativeMethods
    {
        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmGetColorizationColor(out uint colorizationColor, [MarshalAs(UnmanagedType.Bool)]out bool colorizationOpaqueBlend);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmIsCompositionEnabled([MarshalAs(UnmanagedType.Bool)] out bool enabled);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND blurBehind);

        [DllImport("Gdi32.dll")]
        internal static extern IntPtr CreateRectRgn([In] int nLeftRect, [In] int nTopRect, [In] int nRightRect, [In] int nBottomRect);
        [DllImport("Gdi32.dll")]
        internal static extern bool DeleteObject([In] IntPtr hObject);
   }
}
