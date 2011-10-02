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

namespace DoubanFM
{
	/// <summary>
	/// HotKeyWindow.xaml 的交互逻辑
	/// </summary>
	public partial class HotKeyWindow : ChildWindowBase
	{
		/// <summary>
		/// 热键设置，关闭窗口前为默认设置，关闭窗口后更新为新设置
		/// </summary>
		internal HotKeys HotKeys { get; private set; }
		/// <summary>
		/// 热键
		/// </summary>
		private HotKey _likeUnlike, _never, _playPause, _next, _showMinimize, _showHide;

		internal HotKeyWindow(DoubanFMWindow owner, HotKeys hotKeys)
		{
			InitializeComponent();

			HotKeys = hotKeys;
			LikeUnlike.IsEnabled = hotKeys.ContainsKey(DoubanFMWindow.Commands.LikeUnlike);
			if (LikeUnlike.IsEnabled)
			{
				_likeUnlike = hotKeys[DoubanFMWindow.Commands.LikeUnlike];
				LikeUnlike.Text = _likeUnlike.ToString();
			}
			Never.IsEnabled = hotKeys.ContainsKey(DoubanFMWindow.Commands.Never);
			if (Never.IsEnabled)
			{
				_never = hotKeys[DoubanFMWindow.Commands.Never];
				Never.Text = _never.ToString();
			}
			PlayPause.IsEnabled = hotKeys.ContainsKey(DoubanFMWindow.Commands.PlayPause);
			if (PlayPause.IsEnabled)
			{
				_playPause = hotKeys[DoubanFMWindow.Commands.PlayPause];
				PlayPause.Text = _playPause.ToString();
			}
			Next.IsEnabled = hotKeys.ContainsKey(DoubanFMWindow.Commands.Next);
			if (Next.IsEnabled)
			{
				_next = hotKeys[DoubanFMWindow.Commands.Next];
				Next.Text = _next.ToString();
			}
			ShowMinimize.IsEnabled = hotKeys.ContainsKey(DoubanFMWindow.Commands.ShowMinimize);
			if (ShowMinimize.IsEnabled)
			{
				_showMinimize = hotKeys[DoubanFMWindow.Commands.ShowMinimize];
				ShowMinimize.Text = _showMinimize.ToString();
			}
			ShowHide.IsEnabled = hotKeys.ContainsKey(DoubanFMWindow.Commands.ShowHide);
			if (ShowHide.IsEnabled)
			{
				_showHide = hotKeys[DoubanFMWindow.Commands.ShowHide];
				ShowHide.Text = _showHide.ToString();
			}
		}

		/// <summary>
		/// 当输入框中有键按下时调用
		/// </summary>
		private void SetHotKey_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			// 在此处添加事件处理程序实现。
			HotKey.ControlKeys control = HotKey.ControlKeys.None;
			System.Windows.Forms.Keys key = System.Windows.Forms.Keys.None;
			foreach (Key kkey in Enum.GetValues(typeof(Key)))
				try
				{
					if (Keyboard.IsKeyDown(kkey))
					{
						if (kkey == Key.LeftCtrl || kkey == Key.RightCtrl)
							control |= HotKey.ControlKeys.Ctrl;
						else if (kkey == Key.LeftShift || kkey == Key.RightShift)
							control |= HotKey.ControlKeys.Shift;
						else if (kkey == Key.LeftAlt || kkey == Key.RightAlt)
							control |= HotKey.ControlKeys.Alt;
						else if (kkey == Key.LWin || kkey == Key.RWin)
							control |= HotKey.ControlKeys.Win;
						else key = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(kkey);
					}
				}
				catch { }
			if (sender == LikeUnlike)
			{
				_likeUnlike = new HotKey(control, key);
				LikeUnlike.Text = _likeUnlike.ToString();
			}
			if (sender == Never)
			{
				_never = new HotKey(control, key);
				Never.Text = _never.ToString();
			}
			if (sender == PlayPause)
			{
				_playPause = new HotKey(control, key);
				PlayPause.Text = _playPause.ToString();
			}
			if (sender == Next)
			{
				_next = new HotKey(control, key);
				Next.Text = _next.ToString();
			}
			if (sender == ShowMinimize)
			{
				_showMinimize = new HotKey(control, key);
				ShowMinimize.Text = _showMinimize.ToString();
			}
			if (sender == ShowHide)
			{
				_showHide = new HotKey(control, key);
				ShowHide.Text = _showHide.ToString();
			}
			e.Handled = true;
		}

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			HotKeys.Clear();
			if (LikeUnlike.IsEnabled && _likeUnlike != null) HotKeys.AddHotKey(DoubanFMWindow.Commands.LikeUnlike, _likeUnlike);
			if (Never.IsEnabled && _never != null) HotKeys.AddHotKey(DoubanFMWindow.Commands.Never, _never);
			if (PlayPause.IsEnabled && _playPause != null) HotKeys.AddHotKey(DoubanFMWindow.Commands.PlayPause, _playPause);
			if (Next.IsEnabled && _next != null) HotKeys.AddHotKey(DoubanFMWindow.Commands.Next, _next);
			if (ShowMinimize.IsEnabled && _showMinimize != null) HotKeys.AddHotKey(DoubanFMWindow.Commands.ShowMinimize, _showMinimize);
			if (ShowHide.IsEnabled && _showHide != null) HotKeys.AddHotKey(DoubanFMWindow.Commands.ShowHide, _showHide);
			this.Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}