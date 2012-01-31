using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;
using DoubanFM.Interop;

namespace DoubanFM.Aero
{
	public class AeroHelper
	{
		public static bool ExtendGlassFrame(Window window, Thickness margin)
		{
			if (!AeroGlassCompositionEnabled)
				return false;

			IntPtr hwnd = new WindowInteropHelper(window).Handle;
			if (hwnd == IntPtr.Zero)
				throw new InvalidOperationException("The Window must be shown before extending glass.");

			MARGINS margins = new MARGINS((int)margin.Left, (int)margin.Top, (int)margin.Right, (int)margin.Bottom);
			NativeMethods.DwmExtendFrameIntoClientArea(hwnd, ref margins);

			return true;
		}

		public static bool EnableBlurBehindWindow(Window window)
		{
			if (!AeroGlassCompositionEnabled)
				return false;

			IntPtr hwnd = new WindowInteropHelper(window).Handle;
			if (hwnd == IntPtr.Zero)
				throw new InvalidOperationException("The Window must be shown before extending glass.");

			DWM_BLURBEHIND bb = new DWM_BLURBEHIND();
			bb.dwFlags = DWM_BLURBEHIND.DWM_BB_ENABLE | DWM_BLURBEHIND.DWM_BB_BLURREGION;
			bb.fEnable = true;
			bb.hRegionBlur = NativeMethods.CreateRectRgn(0, 0, (int)window.ActualWidth, (int)window.ActualHeight);
			NativeMethods.DwmEnableBlurBehindWindow(hwnd, ref bb);
			NativeMethods.DeleteObject(bb.hRegionBlur);
			
			return true;
		}

		public static bool AeroGlassCompositionEnabled
		{
			get
			{
				if (Environment.OSVersion.Version.Major >= 6)
				{
					try
					{
						return NativeMethods.DwmIsCompositionEnabled();
					}
					catch
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
		}
	}
}