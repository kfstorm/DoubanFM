/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Interop;

namespace DoubanFM
{
	/// <summary>
	/// 窗口基类
	/// </summary>
	public class WindowBase : Window
	{
		static WindowBase()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowBase), new FrameworkPropertyMetadata(typeof(WindowBase)));
		}

		public WindowBase()
		{
			this.SourceInitialized += delegate
			{
				shadow = new WindowShadow(this);

				if (Environment.OSVersion.Version.Major >= 6)
				{
					this.AeroGlassCompositionChanged += new EventHandler<Aero.AeroGlassCompositionChangedEventArgs>(WindowBase_AeroGlassCompositionChanged);

					WindowInteropHelper interopHelper = new WindowInteropHelper(this);

					// add Window Proc hook to capture DWM messages
					HwndSource source = HwndSource.FromHwnd(interopHelper.Handle);
					source.AddHook(new HwndSourceHook(WndProc));

					Aero.AeroHelper.EnableBlurBehindWindow(this);
				}
			};
		}

		void WindowBase_AeroGlassCompositionChanged(object sender, Aero.AeroGlassCompositionChangedEventArgs e)
		{
			if (e.GlassAvailable)
			{
				Aero.AeroHelper.EnableBlurBehindWindow(this);
			}
		}

		private WindowShadow shadow;

		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			try
			{
				this.DragMove();
			}
			catch { }

			base.OnMouseLeftButtonDown(e);
		}

		public event EventHandler<Aero.AeroGlassCompositionChangedEventArgs> AeroGlassCompositionChanged;

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == Aero.DwmApi.DWMMessages.WM_DWMCOMPOSITIONCHANGED
				|| msg == Aero.DwmApi.DWMMessages.WM_DWMNCRENDERINGCHANGED)
			{
				if (AeroGlassCompositionChanged != null)
				{
					AeroGlassCompositionChanged(this,
						new Aero.AeroGlassCompositionChangedEventArgs(Aero.AeroHelper.AeroGlassCompositionEnabled));
				}

				handled = true;
			}
			return IntPtr.Zero;
		}
	}
}
