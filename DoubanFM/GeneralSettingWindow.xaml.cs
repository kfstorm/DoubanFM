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
	/// 常规设置窗口
	/// </summary>
	public partial class GeneralSettingWindow : ChildWindowBase
	{
		Player player;

		public GeneralSettingWindow()
		{
			InitializeComponent();
			player = (Player)FindResource("Player");

			CbSearchGoogleMusic.IsChecked = DownloadSearch.Settings.DownloadSite.HasFlag(DownloadSite.GoogleMusic);
			CbSearchBaiduTing.IsChecked = DownloadSearch.Settings.DownloadSite.HasFlag(DownloadSite.BaiduTing);

			switch (player.Settings.ProxyKind)
			{
				case Settings.ProxyKinds.Default:
					RbDefaultProxy.IsChecked = true;
					break;
				case Settings.ProxyKinds.None:
					RbNoProxy.IsChecked = true;
					break;
				case Settings.ProxyKinds.Custom:
					RbCustomProxy.IsChecked = true;
					break;
				default:
					break;
			}

			PbProxyPassword.Password = player.Settings.ProxyPassword;
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

		private void ProxyKindChanged(object sender, RoutedEventArgs e)
		{
			Settings.ProxyKinds newKind = Settings.ProxyKinds.Default;
			if (RbNoProxy.IsChecked == true)
			{
				newKind = Settings.ProxyKinds.None;
			}
			if (RbCustomProxy.IsChecked == true)
			{
				newKind = Settings.ProxyKinds.Custom;
			}
			
			if (player.Settings.ProxyKind != newKind)
			{
				player.Settings.ProxyKind = newKind;
			}
		}

		private void PbProxyPassword_PasswordChanged(object sender, RoutedEventArgs e)
		{
			player.Settings.ProxyPassword = PbProxyPassword.Password;
		}
	}
}