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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DoubanFM.Interop;
using System.Windows.Interop;

namespace DoubanFM
{
	/// <summary>
	/// ShadowWindow.xaml 的交互逻辑
	/// </summary>
	public partial class ShadowWindow : Window
	{
		protected IntPtr Handle = IntPtr.Zero;
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

		static ShadowWindow()
		{
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

		static Uri GetImageUri(string filename)
		{
			return new Uri("pack://application:,,,/DoubanFM;component/Images/Shadow/" + filename);
		}

		public ShadowWindow(Window owner)
		{
			InitializeComponent();

			if (owner.IsLoaded)
			{
				Init(owner);
			}
			else
			{
				owner.SourceInitialized += delegate
				{
					Init(owner);
				};
			}
		}

		void Init(Window owner)
		{
			Owner = owner;
			Handle = new WindowInteropHelper(this).EnsureHandle();
			OwnerHandle = new WindowInteropHelper(Owner).EnsureHandle();

			int extendedStyle = NativeMethods.GetWindowLong(Handle, GWL.EXSTYLE);
			NativeMethods.SetWindowLong(Handle, GWL.EXSTYLE, extendedStyle
				//鼠标穿透
				| WS.EX.TRANSPARENT);

			Owner.Activated += new EventHandler(Owner_Activated);
			Owner.Deactivated += new EventHandler(Owner_Deactivated);
			Owner.StateChanged += new EventHandler(Owner_StateChanged);
			Owner.IsVisibleChanged += new DependencyPropertyChangedEventHandler(Owner_IsVisibleChanged);
			Owner.LocationChanged += new EventHandler(Owner_LocationChanged);
			Owner.SizeChanged += new SizeChangedEventHandler(Owner_SizeChanged);
			
			if (Owner.IsActive)
			{
				ChangeToActive();
			}
			else
			{
				ChangeToInactive();
			}

			Owner_LocationChanged(null, null);
			Owner_SizeChanged(null, null);

			ConsiderShowShadow();
		}

		void Owner_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			this.Width = Owner.ActualWidth + 24 + 24;
			this.Height = Owner.ActualHeight + 24 + 24;
		}

		void Owner_LocationChanged(object sender, EventArgs e)
		{
			this.Left = Owner.Left - 24;
			this.Top = Owner.Top - 24;
		}

		void Owner_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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

		void Owner_StateChanged(object sender, EventArgs e)
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

		void Owner_Activated(object sender, EventArgs e)
		{
			ChangeToActive();
		}

		void Owner_Deactivated(object sender, EventArgs e)
		{
			ChangeToInactive();
		}

		protected void HideShadow()
		{
			try
			{
				Hide();
			}
			catch { }
		}

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
			Owner.Activate();
		}

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

		public static ShadowWindow Attach(Window window)
		{
			return new ShadowWindow(window);
		}
	}
}
