/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using DoubanFM.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DoubanFM
{
	/// <summary>
	/// 阴影窗口，用于在无边框的矩形窗口四周显示类似Aero特效的阴影
	/// </summary>
	public partial class ShadowWindow : Window
	{
		/// <summary>
		/// 阴影窗口的句柄
		/// </summary>
		protected IntPtr Handle = IntPtr.Zero;

		/// <summary>
		/// 父窗口的句柄
		/// </summary>
		protected IntPtr OwnerHandle = IntPtr.Zero;

		protected static readonly ImageSource ActiveTopLeftImage;
		protected static readonly ImageSource ActiveTopImage;
		protected static readonly ImageSource ActiveTopRightImage;
		protected static readonly ImageSource ActiveLeftImage;
		protected static readonly ImageSource ActiveRightImage;
		protected static readonly ImageSource ActiveBottomLeftImage;
		protected static readonly ImageSource ActiveBottomImage;
		protected static readonly ImageSource ActiveBottomRightImage;

		protected static readonly ImageSource InactiveTopLeftImage;
		protected static readonly ImageSource InactiveTopImage;
		protected static readonly ImageSource InactiveTopRightImage;
		protected static readonly ImageSource InactiveLeftImage;
		protected static readonly ImageSource InactiveRightImage;
		protected static readonly ImageSource InactiveBottomLeftImage;
		protected static readonly ImageSource InactiveBottomImage;
		protected static readonly ImageSource InactiveBottomRightImage;

		protected const double shadowSize = 24;
		protected static readonly GridLength gridShadowSize = new GridLength(shadowSize, GridUnitType.Pixel);

		public GridLength GridShadowSize { get { return gridShadowSize; } }

		static ShadowWindow()
		{
			//加载阴影图片
			ActiveTopLeftImage = new BitmapImage(GetImageUri("ACTIVESHADOWTOPLEFT.png"));
			ActiveTopImage = new BitmapImage(GetImageUri("ACTIVESHADOWTOP.png"));
			ActiveTopRightImage = new BitmapImage(GetImageUri("ACTIVESHADOWTOPRIGHT.png"));
			ActiveLeftImage = new BitmapImage(GetImageUri("ACTIVESHADOWLEFT.png"));
			ActiveRightImage = new BitmapImage(GetImageUri("ACTIVESHADOWRIGHT.png"));
			ActiveBottomLeftImage = new BitmapImage(GetImageUri("ACTIVESHADOWBOTTOMLEFT.png"));
			ActiveBottomImage = new BitmapImage(GetImageUri("ACTIVESHADOWBOTTOM.png"));
			ActiveBottomRightImage = new BitmapImage(GetImageUri("ACTIVESHADOWBOTTOMRIGHT.png"));

			InactiveTopLeftImage = new BitmapImage(GetImageUri("INACTIVESHADOWTOPLEFT.png"));
			InactiveTopImage = new BitmapImage(GetImageUri("INACTIVESHADOWTOP.png"));
			InactiveTopRightImage = new BitmapImage(GetImageUri("INACTIVESHADOWTOPRIGHT.png"));
			InactiveLeftImage = new BitmapImage(GetImageUri("INACTIVESHADOWLEFT.png"));
			InactiveRightImage = new BitmapImage(GetImageUri("INACTIVESHADOWRIGHT.png"));
			InactiveBottomLeftImage = new BitmapImage(GetImageUri("INACTIVESHADOWBOTTOMLEFT.png"));
			InactiveBottomImage = new BitmapImage(GetImageUri("INACTIVESHADOWBOTTOM.png"));
			InactiveBottomRightImage = new BitmapImage(GetImageUri("INACTIVESHADOWBOTTOMRIGHT.png"));

			//冻结阴影图像，以便图像重复使用
			ActiveTopLeftImage.Freeze();
			ActiveTopImage.Freeze();
			ActiveTopRightImage.Freeze();
			ActiveLeftImage.Freeze();
			ActiveRightImage.Freeze();
			ActiveBottomLeftImage.Freeze();
			ActiveBottomImage.Freeze();
			ActiveBottomRightImage.Freeze();

			InactiveTopLeftImage.Freeze();
			InactiveTopImage.Freeze();
			InactiveTopRightImage.Freeze();
			InactiveLeftImage.Freeze();
			InactiveRightImage.Freeze();
			InactiveBottomLeftImage.Freeze();
			InactiveBottomImage.Freeze();
			InactiveBottomRightImage.Freeze();
		}

		/// <summary>
		/// 根据文件名获取图片的URI
		/// </summary>
		/// <param name="filename">文件名</param>
		/// <returns>图片的URI</returns>
		private static Uri GetImageUri(string filename)
		{
			return new Uri("pack://application:,,,/DoubanFM;component/Images/Shadow/" + filename);
		}

		/// <summary>
		/// 生成 <see cref="ShadowWindow"/> class 的新实例。
		/// </summary>
		/// <param name="owner">父窗口</param>
		public ShadowWindow(Window owner)
		{
			InitializeComponent();

			if (owner.IsLoaded)
			{
				Init(owner);
			}
			else
			{
				owner.ContentRendered += delegate
				{
					Init(owner);
				};
			}
			this.LocationChanged += ShadowWindow_LocationChanged;
		}

		/// <summary>
		/// 初始化阴影窗口
		/// </summary>
		/// <param name="owner">父窗口</param>
		private void Init(Window owner)
		{
			Owner = owner;
			Handle = new WindowInteropHelper(this).EnsureHandle();
			OwnerHandle = new WindowInteropHelper(Owner).EnsureHandle();

			int extendedStyle = NativeMethods.GetWindowLong(Handle, GWL.EXSTYLE);
			NativeMethods.SetWindowLong(Handle, GWL.EXSTYLE, extendedStyle
				//鼠标穿透
				| WS.EX.TRANSPARENT);

			//监听父窗口的事件
			Owner.Activated += new EventHandler(Owner_Activated);
			Owner.Deactivated += new EventHandler(Owner_Deactivated);
			Owner.StateChanged += new EventHandler(Owner_StateChanged);
			Owner.IsVisibleChanged += new DependencyPropertyChangedEventHandler(Owner_IsVisibleChanged);
			Owner.LocationChanged += new EventHandler(Owner_LocationChanged);
			Owner.SizeChanged += new SizeChangedEventHandler(Owner_SizeChanged);

			//初始化活动或非活动的阴影
			if (Owner.IsActive)
			{
				ChangeToActive();
			}
			else
			{
				ChangeToInactive();
			}

			//初始化大小和位置
			Owner_LocationChanged(null, null);
			Owner_SizeChanged(null, null);

			//考虑显示阴影
			ConsiderShowShadow();
		}

		private void Owner_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			this.Width = Owner.ActualWidth + shadowSize * 2;
			this.Height = Owner.ActualHeight + shadowSize * 2;
		}

		private bool isOwnerLocationChanged = false;
		private object lockObject = new object();

		private void Owner_LocationChanged(object sender, EventArgs e)
		{
			lock (lockObject)
			{
				isOwnerLocationChanged = true;
				this.Left = Owner.Left - shadowSize;
				this.Top = Owner.Top - shadowSize;
				isOwnerLocationChanged = false;
			}
		}

		private void ShadowWindow_LocationChanged(object sender, EventArgs e)
		{
			if (!isOwnerLocationChanged)
			{
				this.Left = Owner.Left - shadowSize;
				this.Top = Owner.Top - shadowSize;
			}
		}

		private void Owner_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue)
			{
				ConsiderShowShadow();
			}
			else
			{
				HideShadow();
			}
		}

		private void Owner_StateChanged(object sender, EventArgs e)
		{
			switch (Owner.WindowState)
			{
				case WindowState.Maximized:
				case WindowState.Minimized:
					HideShadow();
					break;

				case WindowState.Normal:
					ConsiderShowShadow();
					break;
				default:
					break;
			}
		}

		private void Owner_Activated(object sender, EventArgs e)
		{
			ChangeToActive();
		}

		private void Owner_Deactivated(object sender, EventArgs e)
		{
			ChangeToInactive();
		}

		/// <summary>
		/// 隐藏阴影
		/// </summary>
		protected void HideShadow()
		{
			try
			{
				Hide();
			}
			catch { }
		}

		/// <summary>
		/// 考虑显示阴影，若符合显示阴影的条件，则显示阴影，否则什么都不做
		/// </summary>
		protected void ConsiderShowShadow()
		{
			if (Owner.IsVisible && Owner.Visibility == Visibility.Visible && Owner.WindowState == System.Windows.WindowState.Normal)
			{
				try
				{
					Show();
					//保证阴影窗口始终和目标窗口的Z序相邻，防止有时其他窗口覆盖在目标窗口上时阴影窗口会覆盖其他窗口。
					NativeMethods.SetWindowPos(Handle, OwnerHandle, 0, 0, 0, 0, SWP.SHOWWINDOW | SWP.NOACTIVATE | SWP.NOOWNERZORDER | SWP.NOREPOSITION | SWP.NOMOVE | SWP.NOSIZE);
				}
				catch { }
			}
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			try
			{
				Owner.Activate();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
		}

		/// <summary>
		/// 变更为活动窗口的阴影
		/// </summary>
		protected void ChangeToActive()
		{
			ImTopLeft.Source = ActiveTopLeftImage;
			ImTop.Source = ActiveTopImage;
			ImTopRight.Source = ActiveTopRightImage;
			ImLeft.Source = ActiveLeftImage;
			ImRight.Source = ActiveRightImage;
			ImBottomLeft.Source = ActiveBottomLeftImage;
			ImBottom.Source = ActiveBottomImage;
			ImBottomRight.Source = ActiveBottomRightImage;
		}

		/// <summary>
		/// 变更为非活动窗口的阴影
		/// </summary>
		protected void ChangeToInactive()
		{
			ImTopLeft.Source = InactiveTopLeftImage;
			ImTop.Source = InactiveTopImage;
			ImTopRight.Source = InactiveTopRightImage;
			ImLeft.Source = InactiveLeftImage;
			ImRight.Source = InactiveRightImage;
			ImBottomLeft.Source = InactiveBottomLeftImage;
			ImBottom.Source = InactiveBottomImage;
			ImBottomRight.Source = InactiveBottomRightImage;
		}

		/// <summary>
		/// 为目标窗口附加阴影
		/// </summary>
		/// <param name="window">目标窗口</param>
		/// <returns>附加上的阴影窗口</returns>
		public static ShadowWindow Attach(Window window)
		{
			return new ShadowWindow(window);
		}
	}
}