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
	/// BalloonSongInfo.xaml 的交互逻辑
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