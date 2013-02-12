/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using DoubanFM.Interop;
using System;
using System.Windows;
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
			this.ContentRendered += delegate
			{
				//添加窗口阴影
				ShadowWindow.Attach(this);
			};

			this.SourceInitialized += delegate
			{
				//只有Windows Vista和Windows 7支持Aero特效
				if (Environment.OSVersion.Version >= new Version(6, 0) && Environment.OSVersion.Version < new Version(6, 2))
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
				//只有Windows Vista和Windows 7支持Aero特效
				if (Environment.OSVersion.Version >= new Version(6, 0) && Environment.OSVersion.Version < new Version(6, 2))
				{
					Aero.AeroHelper.EnableBlurBehindWindow(this);
				}
			};
		}

		/// <summary>
		/// Aero效果设置已改变
		/// </summary>
		private void WindowBase_AeroGlassCompositionChanged(object sender, Aero.AeroGlassCompositionChangedEventArgs e)
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
		private bool pressed = false;

        /// <summary>
        /// 鼠标相对于窗口的位置
        /// </summary>
	    private Point? mousePosition;

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

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            mousePosition = e.GetPosition(this);
        }

		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
		{
			pressed = false;
		    mousePosition = null;
			base.OnMouseLeave(e);
		}

		/// <summary>
		/// 支持拖拽窗口
		/// </summary>
		protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseMove(e);

            //当鼠标没动，但有控件在鼠标下方移动时，仍会触发MouseMove事件，所以要根据鼠标位置来判断是否真的移动了。
            //只有鼠标真的移动了，才可能触发窗口拖动。
            var newPosition = e.GetPosition(this);
		    bool moved = mousePosition.HasValue && newPosition != mousePosition.Value;
		    mousePosition = newPosition;

            if (!IsDraging && pressed && System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
			{
			    if (moved)
			    {
			        IsDraging = true;
			        DragMove();
			        pressed = false;
			        IsDraging = false;
			    }
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