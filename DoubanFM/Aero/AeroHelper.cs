using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;
using DoubanFM.Interop;

namespace DoubanFM.Aero
{
	/// <summary>
	/// 为窗口启用Aero效果提供支持
	/// </summary>
	public class AeroHelper
	{
		/// <summary>
		/// 将窗口边框的Aero效果扩展到客户区域
		/// </summary>
		/// <param name="window">目标窗口</param>
		/// <param name="margin">外边距</param>
		/// <returns>成功与否</returns>
		public static bool ExtendGlassFrame(Window window, Thickness margin)
		{
			if (!AeroGlassCompositionEnabled)
				return false;

			IntPtr hwnd = new WindowInteropHelper(window).Handle;
			if (hwnd == IntPtr.Zero)
				throw new InvalidOperationException("在启用Aero效果前窗口必须已显示");

			MARGINS margins = new MARGINS((int)margin.Left, (int)margin.Top, (int)margin.Right, (int)margin.Bottom);
			NativeMethods.DwmExtendFrameIntoClientArea(hwnd, ref margins);

			return true;
		}

		/// <summary>
		/// 在窗口背后启用模糊效果
		/// </summary>
		/// <param name="window">目标窗口</param>
		/// <returns>成功与否</returns>
		public static bool EnableBlurBehindWindow(Window window)
		{
			if (!AeroGlassCompositionEnabled)
				return false;

			IntPtr hwnd = new WindowInteropHelper(window).Handle;
			if (hwnd == IntPtr.Zero)
				throw new InvalidOperationException("在启用Aero效果前窗口必须已显示");

			//创建DWM_BLURBEHIND结构
			DWM_BLURBEHIND bb = new DWM_BLURBEHIND();
			bb.dwFlags = DWM_BLURBEHIND.DWM_BB_ENABLE | DWM_BLURBEHIND.DWM_BB_BLURREGION;
			bb.fEnable = true;
			bb.hRegionBlur = NativeMethods.CreateRectRgn(0, 0, (int)window.ActualWidth, (int)window.ActualHeight);

			NativeMethods.DwmEnableBlurBehindWindow(hwnd, ref bb);
			//回收句柄
			NativeMethods.DeleteObject(bb.hRegionBlur);
			
			return true;
		}

		/// <summary>
		/// 系统是否已启用Aero特效组合，若没启用，则不能使用Aero效果
		/// </summary>
		public static bool AeroGlassCompositionEnabled
		{
			get
			{
				//只有Windows Vista或更高版本才支持Aero特效
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