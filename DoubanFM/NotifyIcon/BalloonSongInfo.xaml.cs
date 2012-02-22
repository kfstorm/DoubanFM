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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;

namespace DoubanFM.NotifyIcon
{
	/// <summary>
	/// 显示歌曲信息的气泡
	/// </summary>
	public partial class BalloonSongInfo : UserControl
	{
		public BalloonSongInfo()
		{
			InitializeComponent();

			Hardcodet.Wpf.TaskbarNotification.TaskbarIcon.AddBalloonClosingHandler(this, OnBalloonClosing);
		}

		void OnBalloonClosing(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
		}

		/// <summary>
		/// 平滑地显示封面
		/// </summary>
		public void ShowCoverSmooth()
		{
			((Storyboard)FindResource("ShowCoverSmooth")).Begin();
		}

		private void FadeOut_Completed(object sender, EventArgs e)
		{
			((Popup)this.Parent).IsOpen = false;
		}

		private void userControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			((DoubanFMWindow)App.Current.MainWindow).ShowFront();
		}
	}
}