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
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace DoubanFM
{
	/// <summary>
	/// LyricsSettingWindow.xaml 的交互逻辑
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
		}

		private void SliderStrokeWeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			(Owner as DoubanFMWindow)._lyricsWindow.Update();
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
	}
}
