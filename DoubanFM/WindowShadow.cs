using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace DoubanFM
{
	/// <summary>
	/// Turns out that settings AllowsTransparency="True" in conjunction with DwmSetWindowAttribute is not a good idea,
	/// DwmSetWindowAttribute gets disabled (and won't re-enable) in a bunch of different scenarios (such as changed 
	/// focus).
	/// 
	/// To work around this, this class implements the transparent windows as shadows method from 
	/// http://www.nikosbaxevanis.com/bonus-bits/2010/12/building-a-metro-ui-with-wpf.html
	/// </summary>
	class WindowShadow
	{
		private const Int32 c_edgeWndSize = 23;

		private Window m_wndT;
		private Window m_wndL;
		private Window m_wndB;
		private Window m_wndR;

		private Window m_target;

		[DllImport("user32.dll")]
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
		[DllImport("user32.dll", SetLastError = true)]
		static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		static readonly int GWL_EXSTYLE = (-20);
		static readonly int WS_EX_TRANSPARENT = 0x0020;
		
		public WindowShadow(Window target)
		{
			m_target = target;

			target.Closed += target_Closed;
			target.Activated += target_Activated;
			target.Deactivated += target_Deactivated;

			target.LocationChanged += new EventHandler(target_LocationChanged);
			target.SizeChanged += new SizeChangedEventHandler(target_LocationChanged);
			target.StateChanged += new EventHandler(target_StateChanged);
			target.IsVisibleChanged += new DependencyPropertyChangedEventHandler(target_IsVisibleChanged);

			InitializeSurrounds();

			target.Dispatcher.BeginInvoke(new Action(() =>
				{
					target_LocationChanged(null, null);
					ShowSurrounds();
				}));
		}

		~WindowShadow()
		{
			if (m_target != null)
			{
				m_target.Closed -= target_Closed;
				m_target.Activated -= target_Activated;
				m_target.Deactivated -= target_Deactivated;
			}
		}

		void target_StateChanged(object sender, EventArgs e)
		{
			if (m_target.WindowState == WindowState.Normal && m_target.Visibility == Visibility.Visible)
			{
				target_LocationChanged(null, null);
				ShowSurrounds();
			}
			else
			{
				HideSurrounds();
			}
		}
		
		void target_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (m_target.WindowState == WindowState.Normal && m_target.Visibility == Visibility.Visible)
			{
				target_LocationChanged(null, null);
				ShowSurrounds();
			}
			else
			{
				HideSurrounds();
			}
		}
		
		void target_Activated(object sender, EventArgs e)
		{
			SetSurroundShadows(true);
		}

		void target_Deactivated(object sender, EventArgs e)
		{
			SetSurroundShadows(false);
		}

		void target_Closed(object sender, EventArgs e)
		{
			CloseSurrounds();
		}

		/// <summary>
		/// Initializes the surrounding windows.
		/// </summary>
		private void InitializeSurrounds()
		{
			// Top.
			m_wndT = CreateTransparentWindow();

			// Left.
			m_wndL = CreateTransparentWindow();

			// Bottom.
			m_wndB = CreateTransparentWindow();

			// Right.
			m_wndR = CreateTransparentWindow();

			SetSurroundShadows(m_target.IsActive);
		}

		/// <summary>
		/// Creates an empty window.
		/// </summary>
		/// <returns></returns>
		private Window CreateTransparentWindow()
		{
			Window wnd = new Window();

			wnd.AllowsTransparency = true;
			wnd.ShowActivated = false;
			wnd.ShowInTaskbar = false;
			wnd.WindowStyle = WindowStyle.None;
			wnd.Background = null;
			wnd.Owner = m_target; // owned windows will not show up in alt+tab

			wnd.SourceInitialized += new EventHandler((o, e) =>
			{
				var hwnd = new WindowInteropHelper(wnd).Handle;
				int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
				SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle
					//鼠标穿透
					| WS_EX_TRANSPARENT);
			});
			
			// set initial height to 0 so that the window doesn't "pop in" from a larger size
			wnd.Height = 0;
			wnd.Width = 0;

			return wnd;
		}

		/// <summary>
		/// Sets the artificial drop shadow.
		/// </summary>
		/// <param name="active">if set to <c>true</c> [active].</param>
		private void SetSurroundShadows(Boolean active = true)
		{
			if (active)
			{
				m_wndT.Content = GetDecorator("images/shadow/ACTIVESHADOWTOP.PNG");
				m_wndL.Content = GetDecorator("images/shadow/ACTIVESHADOWLEFT.PNG");
				m_wndB.Content = GetDecorator("images/shadow/ACTIVESHADOWBOTTOM.PNG");
				m_wndR.Content = GetDecorator("images/shadow/ACTIVESHADOWRIGHT.PNG");
			}
			else
			{
				m_wndT.Content = GetDecorator("images/shadow/INACTIVESHADOWTOP.PNG");
				m_wndL.Content = GetDecorator("images/shadow/INACTIVESHADOWLEFT.PNG");
				m_wndB.Content = GetDecorator("images/shadow/INACTIVESHADOWBOTTOM.PNG");
				m_wndR.Content = GetDecorator("images/shadow/INACTIVESHADOWRIGHT.PNG");
			}
		}

		private Decorator GetDecorator(String imageUri, CornerRadius radius = new CornerRadius())
		{
			Border border = new Border();
			border.CornerRadius = radius;
			border.Background = new ImageBrush(
				new BitmapImage(
					new Uri(BaseUriHelper.GetBaseUri(m_target),
						imageUri)));

			return border;
		}

		/// <summary>
		/// Handles the location changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> 
		/// instance containing the event data.</param>
		private void target_LocationChanged(Object sender, EventArgs e)
		{
			m_wndT.Height = c_edgeWndSize;
			m_wndT.Left = m_target.Left;
			m_wndT.Top = m_target.Top - m_wndT.Height;
			m_wndT.Width = m_target.Width;

			m_wndL.Width = c_edgeWndSize;
			m_wndL.Left = m_target.Left - m_wndL.Width;
			m_wndL.Top = m_target.Top;
			m_wndL.Height = m_target.Height;

			m_wndB.Height = c_edgeWndSize;
			m_wndB.Left = m_target.Left;
			m_wndB.Top = m_target.Top + m_target.Height;
			m_wndB.Width = m_target.Width;

			m_wndR.Left = m_target.Left + m_target.Width;
			m_wndR.Top = m_target.Top;
			m_wndR.Width = c_edgeWndSize;
			m_wndR.Height = m_target.Height;
		}

		/// <summary>
		/// Shows the surrounding windows.
		/// </summary>
		private void ShowSurrounds()
		{
			m_wndT.Show();
			m_wndL.Show();
			m_wndB.Show();
			m_wndR.Show();
		}

		/// <summary>
		/// Hides the surrounding windows.
		/// </summary>
		private void HideSurrounds()
		{
			m_wndT.Hide();
			m_wndL.Hide();
			m_wndB.Hide();
			m_wndR.Hide();
		}
		/// <summary>
		/// Closes the surrounding windows.
		/// </summary>
		private void CloseSurrounds()
		{
			m_wndT.Close();
			m_wndL.Close();
			m_wndB.Close();
			m_wndR.Close();
		}
	}
}
