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

			CbOutputDevice.Items.Add("默认");
			foreach (var device in Bass.BassEngine.GetDeviceInfos())
			{
				CbOutputDevice.Items.Add(device);
			}
			if (Bass.BassEngine.Instance.Device == null)
			{
				CbOutputDevice.SelectedIndex = 0;
			}
			else
			{
				CbOutputDevice.SelectedItem = Bass.BassEngine.Instance.Device;
			}
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

		bool error = false;

		private void CbOutputDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (error) return;
			try
			{
				if (e.AddedItems.Count > 0 && e.AddedItems[0] is Bass.DeviceInfo)
				{
					(Application.Current.MainWindow as DoubanFMWindow).ChangeOutputDevice((Bass.DeviceInfo)e.AddedItems[0]);
				}
				else
				{
					(Application.Current.MainWindow as DoubanFMWindow).ChangeOutputDevice(null);
				}
			}
			catch (Exception ex)
			{
				error = true;
				if (e.RemovedItems.Count > 0)
				{
					CbOutputDevice.SelectedItem = e.RemovedItems[0];
				}
				MessageBox.Show(ex.Message, null, MessageBoxButton.OK, MessageBoxImage.Error);
				error = false;
			}
		}
	}
}