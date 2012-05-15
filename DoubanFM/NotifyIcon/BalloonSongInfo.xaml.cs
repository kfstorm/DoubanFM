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

		/// <summary>
		/// 清除歌曲和频道的绑定
		/// </summary>
		public void ClearBindings()
		{
			var tempSource = Cover.Source;
			BindingOperations.ClearBinding(Cover, Image.SourceProperty);
			Cover.Source = tempSource;
			var tempText = TbChannel.Text;
			BindingOperations.ClearBinding(TbChannel, TextBlock.TextProperty);
			TbChannel.Text = tempText;
			tempText = TbTitle.Text;
			BindingOperations.ClearBinding(TbTitle, TextBlock.TextProperty);
			TbTitle.Text = tempText;
			tempText = TbArtist.Text;
			BindingOperations.ClearBinding(TbArtist, TextBlock.TextProperty);
			TbArtist.Text = tempText;
			tempText = TbAlbum.Text;
			BindingOperations.ClearBinding(TbAlbum, TextBlock.TextProperty);
			TbAlbum.Text = tempText;
		}
	}
}