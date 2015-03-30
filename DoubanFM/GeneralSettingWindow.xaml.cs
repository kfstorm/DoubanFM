/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

            CbSearchBaiduMusic.IsChecked = DownloadSearch.Settings.DownloadSite.HasFlag(DownloadSite.BaiduMusic);
            CbSearchQQMusic.IsChecked = DownloadSearch.Settings.DownloadSite.HasFlag(DownloadSite.QQMusic);

            //Init proxy setting.
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
            }
            PbProxyPassword.Password = player.Settings.ProxyPassword;

            //Init output device setting.
            CbOutputDevice.Items.Add(DoubanFM.Resources.Resources.Default);
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

            //Init language setting.
            var availableCultrues = new[] { CultureInfo.GetCultureInfo("en-US"), CultureInfo.GetCultureInfo("zh-CN") };
            CultureInfo selectedLanguage = null;
            foreach (var cultrue in availableCultrues)
            {
                var cb = new ComboBoxItem { Content = cultrue.NativeName, Tag = cultrue };
                CbLanguage.Items.Add(cb);
                if (cultrue.LCID == player.Settings.CultureInfo.LCID)
                {
                    selectedLanguage = cultrue;
                }
            }
            if (selectedLanguage == null)
            {
                selectedLanguage = availableCultrues[0];
            }
            CbLanguage.SelectedItem = CbLanguage.Items.OfType<ComboBoxItem>().First(tb => ((CultureInfo)tb.Tag).LCID == selectedLanguage.LCID);
            CbLanguage.SelectionChanged += CbLanguage_SelectionChanged;
        }

		private void BtnApplyProxy_Click(object sender, RoutedEventArgs e)
		{
			(Owner as DoubanFMWindow).ApplyProxy();
		}

		private void CbSearchBaiduMusic_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if (CbSearchBaiduMusic.IsChecked == true)
			{
				DownloadSearch.Settings.DownloadSite |= DownloadSite.BaiduMusic;
			}
			else
			{
				DownloadSearch.Settings.DownloadSite &= ~DownloadSite.BaiduMusic;
			}
		}

        private void CbSearchQQMusic_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CbSearchQQMusic.IsChecked == true)
            {
                DownloadSearch.Settings.DownloadSite |= DownloadSite.QQMusic;
            }
            else
            {
                DownloadSearch.Settings.DownloadSite &= ~DownloadSite.QQMusic;
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
				MessageBox.Show(ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
				error = false;
			}
		}

        private static bool changed;

        private void CbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var cb = e.AddedItems[0] as ComboBoxItem;
                if (cb != null)
                {
                    var culture = cb.Tag as CultureInfo;
                    if (culture != null)
                    {
                        player.Settings.CultureInfo = culture;
                        if (!changed)
                        {
                            changed = true;
                            MessageBox.Show(this, DoubanFM.Resources.Resources.LanguageChangedHint, string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
        }

	    private void CheckBoxEnableDownloadRateRestriction_OnClick(object sender, RoutedEventArgs e)
	    {
            Bass.BassEngine.Instance.SetDownloadRateRestriction(CheckBoxEnableDownloadRateRestriction.IsChecked == true);
	    }
	}
}