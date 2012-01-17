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
using DoubanFM.Core;

namespace DoubanFM
{
	/// <summary>
	/// GeneralSettingWindow.xaml 的交互逻辑
	/// </summary>
	public partial class GeneralSettingWindow : ChildWindowBase
	{
		public GeneralSettingWindow()
		{
			InitializeComponent();

			CbSearchGoogleMusic.IsChecked = DownloadSearch.Settings.DownloadSite.HasFlag(DownloadSite.GoogleMusic);
			CbSearchBaiduTing.IsChecked = DownloadSearch.Settings.DownloadSite.HasFlag(DownloadSite.BaiduTing);
		}

		private void BtnApplyProxy_Click(object sender, RoutedEventArgs e)
		{
			(Owner as DoubanFMWindow).ApplyProxy();
		}

		private void CbSearchGoogleMusic_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if (CbSearchGoogleMusic.IsChecked == true)
			{
				DownloadSearch.Settings.DownloadSite |= DownloadSite.GoogleMusic;
			}
			else
			{
				DownloadSearch.Settings.DownloadSite &= ~DownloadSite.GoogleMusic;
			}
		}

		private void CbSearchBaiduTing_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if (CbSearchBaiduTing.IsChecked == true)
			{
				DownloadSearch.Settings.DownloadSite |= DownloadSite.BaiduTing;
			}
			else
			{
				DownloadSearch.Settings.DownloadSite &= ~DownloadSite.BaiduTing;
			}
		}
	}
}