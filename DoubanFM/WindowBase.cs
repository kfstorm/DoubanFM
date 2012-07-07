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
using DoubanFM.Interop;

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
			this.Loaded += delegate
			{
				//添加窗口阴影
				ShadowWindow.Attach(this);
			};

			this.SourceInitialized += delegate
			{
				if (Environment.OSVersion.Version.Major >= 6)
				{
					//监听系统中Aero设置的改变
					this.AeroGlassCompositionChanged += new EventHandler<Aero.AeroGlassCompositionChangedEventArgs>(WindowBase_AeroGlassCompositionChanged);

					//添加钩子来捕获DWM消息
					WindowInteropHelper interopHelper = new WindowInteropHelper(this);
					HwndSource source = HwndSource.FromHwnd(interopHelper.Handle);
					source.AddHook(new HwndSourceHook(WndProc));

					//尝试启用Aero模糊效果
					Aero.AeroHelper.EnableBlurBehindWindow(this);
				}
			};

			//当窗口大小改变后，需调整Aero模糊效果的范围，所以这里调用EnableBlurBehindWindow方法更新模糊效果
			this.SizeChanged += delegate
			{
				if (Environment.OSVersion.Version.Major >= 6)
				{
					Aero.AeroHelper.EnableBlurBehindWindow(this);
				}
			};
		}

		/// <summary>
		/// Aero效果设置已改变
		/// </summary>
		void WindowBase_AeroGlassCompositionChanged(object sender, Aero.AeroGlassCompositionChangedEventArgs e)
		{
			if (e.GlassAvailable)
			{
				Aero.AeroHelper.EnableBlurBehindWindow(this);
			}
		}

		/// <summary>
		/// 当前是否正在拖拽窗口
		/// </summary>
		protected bool IsDraging = false;
		/// <summary>
		/// 是否按下了鼠标左键（不要用Mouse.LeftButton去检测）
		/// </summary>
		bool pressed = false;
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			pressed = true;
			base.OnMouseLeftButtonDown(e);
		}
		protected override void OnMouseRightButtonUp(System.Windows.Input.MouseButtonEventArgs e)
		{
			pressed = false;
			base.OnMouseRightButtonUp(e);
		}
		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
		{
			pressed = false;
			base.OnMouseLeave(e);
		}
		/// <summary>
		/// 支持拖拽窗口
		/// </summary>
		protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!IsDraging && pressed && System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
			{
				IsDraging = true;
				DragMove();
				pressed = false;
				IsDraging = false;
			}
		}

		/// <summary>
		/// 当Aero效果设置已改变时发生。
		/// </summary>
		public event EventHandler<Aero.AeroGlassCompositionChangedEventArgs> AeroGlassCompositionChanged;

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WM.DWMCOMPOSITIONCHANGED
				|| msg == WM.DWMNCRENDERINGCHANGED)
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
