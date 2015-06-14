/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	/// 歌词设置窗口
	/// </summary>
	public partial class LyricsSettingWindow : ChildWindowBase
	{
		public static readonly DependencyProperty LyricsSettingProperty = DependencyProperty.Register("LyricsSetting", typeof(LyricsSetting), typeof(LyricsSettingWindow));

		public LyricsSetting LyricsSetting
		{
			get { return (LyricsSetting)GetValue(LyricsSettingProperty); }
			set { SetValue(LyricsSettingProperty, value); }
		}

		public LyricsSettingWindow(LyricsSetting setting)
		{
			LyricsSetting = setting;
			InitializeComponent();

			this.Closed += delegate
			{
				if (CbShowLyricsBackground.IsChecked == true)
				{
					(Owner as DoubanFMWindow)._lyricsWindow.HideBoundary();
				}
			};

			UpdateComboBoxOutputScreen();
		}

		private void CbShowLyrics_Checked(object sender, RoutedEventArgs e)
		{
			(Owner as DoubanFMWindow).ShowLyrics();
		}

		private void CbShowLyrics_Unchecked(object sender, RoutedEventArgs e)
		{
			(Owner as DoubanFMWindow).HideLyrics();
		}

		private void CbEnableDesktopLyrics_Checked(object sender, RoutedEventArgs e)
		{
			if ((FindResource("Player") as Core.Player).Settings.ShowLyrics)
				(Owner as DoubanFMWindow).ShowDesktopLyrics();
		}

		private void CbEnableDesktopLyrics_Unchecked(object sender, RoutedEventArgs e)
		{
			(Owner as DoubanFMWindow).HideDesktopLyrics();
		}

		private void CbEnableEmbeddedLyrics_Checked(object sender, RoutedEventArgs e)
		{
			if ((FindResource("Player") as Core.Player).Settings.ShowLyrics)
				(Owner as DoubanFMWindow).ShowEmbeddedLyrics();
		}

		private void CbEnableEmbeddedLyrics_Unchecked(object sender, RoutedEventArgs e)
		{
			(Owner as DoubanFMWindow).HideEmbeddedLyrics();
		}

		private void CbAutoForeground_Click(object sender, RoutedEventArgs e)
		{
			(Owner as DoubanFMWindow)._lyricsWindow.UpdateForegroundSetting();
		}

		private void CbShowLyricsBackground_Click(object sender, RoutedEventArgs e)
		{
			if (CbShowLyricsBackground.IsChecked == true)
			{
				(Owner as DoubanFMWindow)._lyricsWindow.ShowBoundary();
			}
			else
			{
				(Owner as DoubanFMWindow)._lyricsWindow.HideBoundary();
			}
		}

		private void UpdateComboBoxOutputScreen()
		{
			pauseUpdateSizeAndLocation = true;

			cbOutputScreen.Items.Clear();
			var screens = Screen.AllScreens;
			for (var i = 0; i < screens.Length; ++i)
			{
				cbOutputScreen.Items.Add(new ComboBoxItem() { Content = string.Format(DoubanFM.Resources.Resources.OutputScreenFormatString, i + 1), Tag = screens[i].DeviceName });
				if (screens[i].DeviceName == LyricsSetting.DesktopLyricsScreen)
				{
					cbOutputScreen.SelectedIndex = i;
				}
				if (cbOutputScreen.SelectedItem == null && screens[i].Primary)
				{
					cbOutputScreen.SelectedIndex = i;
				}
			}

			if (cbOutputScreen.SelectedItem != null)
			{
				LyricsSetting.DesktopLyricsScreen = (cbOutputScreen.SelectedItem as ComboBoxItem).Tag as string;
				((DoubanFMWindow)Owner)._lyricsWindow.UpdateSizeAndLocation();
			}

			pauseUpdateSizeAndLocation = false;
		}

		private bool pauseUpdateSizeAndLocation = false;

		private void cbOutputScreen_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (pauseUpdateSizeAndLocation) return;
			if (e.AddedItems.Count <= 0) return;
			var deviceName = ((ComboBoxItem)e.AddedItems[0]).Tag as string;
			LyricsSetting.DesktopLyricsScreen = deviceName;
			((DoubanFMWindow)Owner)._lyricsWindow.UpdateSizeAndLocation();
		}
	}
}