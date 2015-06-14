/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System.Windows;
using DoubanFM.Core;
using System.Windows.Media.Animation;

namespace DoubanFM.NotifyIcon
{
	/// <summary>
	/// 托盘控制面板
	/// </summary>
	public partial class PopupControlPanel
	{
		public PopupControlPanel()
		{
			InitializeComponent();

			player = FindResource("Player") as Player;
		}

		/// <summary>
		/// 平滑地显示封面
		/// </summary>
		public void ShowCoverSmooth()
		{
			((Storyboard)FindResource("ShowCoverSmooth")).Begin();
		}
		/// <summary>
		/// 隐藏封面
		/// </summary>
		public void HideCover()
		{
			((Storyboard)FindResource("HideCover")).Begin();
		}

		private readonly Player player;

		private void ButtonNext_Click(object sender, RoutedEventArgs e)
		{
            var mainWindow = Application.Current.MainWindow as DoubanFMWindow;
            if (mainWindow != null) mainWindow.Next();
		}

		private void ButtonNever_Click(object sender, RoutedEventArgs e)
		{
            var mainWindow = Application.Current.MainWindow as DoubanFMWindow;
            if (mainWindow != null) mainWindow.Never();
		}

		private void ShareButton_Click(object sender, RoutedEventArgs e)
		{
			// 在此处添加事件处理程序实现。
			if (player.CurrentSong != null)
				new Share(player, (Share.Sites)((FrameworkElement)e.Source).Tag).Go();
		}

		private void CheckBoxShowLyrics_Checked(object sender, RoutedEventArgs e)
		{
			// 在此处添加事件处理程序实现。
            if (player != null)
            {
                var mainWindow = Application.Current.MainWindow as DoubanFMWindow;
                if (mainWindow != null) mainWindow.ShowLyrics();
            }
		}

	    private void CheckBoxShowLyrics_Unchecked(object sender, RoutedEventArgs e)
	    {
	        if (player != null)
	        {
	            var mainWindow = Application.Current.MainWindow as DoubanFMWindow;
	            if (mainWindow != null) mainWindow.HideLyrics();
	        }
	    }

	    private void BtnCopyUrl_Click(object sender, RoutedEventArgs e)
	    {
	        if (player.CurrentSong != null)
	        {
	            new Share(player).Go();
	            MessageBox.Show(Application.Current.MainWindow, DoubanFM.Resources.Resources.UrlCopyedToClipboard,
	                            DoubanFM.Resources.Resources.SuccessfullyCopied, MessageBoxButton.OK,
	                            MessageBoxImage.Information);
	        }
	    }

	    private void BtnOneKeyShare_Click(object sender, RoutedEventArgs e)
		{
		    var mainWindow = Application.Current.MainWindow as DoubanFMWindow;
            if (mainWindow != null) mainWindow.OneKeyShare();
		}

	    private void ButtonExit_Click(object sender, RoutedEventArgs e)
		{
            var mainWindow = Application.Current.MainWindow as DoubanFMWindow;
            if (mainWindow != null) mainWindow.Close();
		}

	    private void BtnDownloadSearch_Click(object sender, RoutedEventArgs e)
	    {
            var mainWindow = Application.Current.MainWindow as DoubanFMWindow;
            if (mainWindow != null) mainWindow.SearchDownload();
	    }
	}
}
