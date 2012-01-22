using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;

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

			DwmApi.MARGINS margins = new DwmApi.MARGINS((int)margin.Left, (int)margin.Top, (int)margin.Right, (int)margin.Bottom);
			DwmApi.DwmExtendFrameIntoClientArea(hwnd, margins);

			return true;
		}

		public static bool EnableBlurBehindWindow(Window window)
		{
			if (!AeroGlassCompositionEnabled)
				return false;

			IntPtr hwnd = new WindowInteropHelper(window).Handle;
			if (hwnd == IntPtr.Zero)
				throw new InvalidOperationException("The Window must be shown before extending glass.");

			DwmApi.DWM_BLURBEHIND bb = new DwmApi.DWM_BLURBEHIND();
			bb.dwFlags = DwmApi.DWM_BLURBEHIND.DWM_BB_ENABLE | DwmApi.DWM_BLURBEHIND.DWM_BB_BLURREGION;
			bb.fEnable = true;
			bb.hRegionBlur = DoubanFM.NativeMethods.CreateRectRgn(0, 0, (int)window.ActualWidth, (int)window.ActualHeight);
			DwmApi.DwmEnableBlurBehindWindow(hwnd, bb);
			DoubanFM.NativeMethods.DeleteObject(bb.hRegionBlur);
			
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
						return DwmApi.DwmIsCompositionEnabled();
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