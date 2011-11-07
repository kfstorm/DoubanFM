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
			InitializeComponent();

			LyricsSetting = setting;

			var fontFamilies = Fonts.SystemFontFamilies;
			List<string> names = new List<string>();
			string selectedName = null;
			foreach (var fontFamily in fontFamilies)
			{
				string name = GetFontName(fontFamily);
				names.Add(name);
				if (name == GetFontName(LyricsSetting.FontFamily))
					selectedName = name;
			}
			names.Sort();
			CbFontFamily.ItemsSource = names;
			if (LyricsSetting.FontFamily != null)
				CbFontFamily.Text = LyricsSetting.FontFamily.ToString();
		}

		/// <summary>
		/// 获取字体名称（简体中文字体返回中文名称）
		/// </summary>
		public static string GetFontName(FontFamily fontFamily)
		{
			if (fontFamily == null) return string.Empty;
			if (fontFamily.FamilyNames.ContainsKey(System.Windows.Markup.XmlLanguage.GetLanguage("zh-cn")))
			{
				return fontFamily.FamilyNames[System.Windows.Markup.XmlLanguage.GetLanguage("zh-cn")];
			}
			return fontFamily.FamilyNames.First().Value;
		}

		private void CbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				LyricsSetting.FontFamily = new FontFamily(CbFontFamily.Text);
			}
			catch
			{
				LyricsSetting.FontFamily = null;
			}
		}
		private void CbFontFamily_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			try
			{
				LyricsSetting.FontFamily = new FontFamily(CbFontFamily.Text);
			}
			catch
			{
				LyricsSetting.FontFamily = null;
			}
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
			(Owner as DoubanFMWindow).ShowDesktopLyrics();
		}

		private void CbEnableDesktopLyrics_Unchecked(object sender, RoutedEventArgs e)
		{
			(Owner as DoubanFMWindow).HideDesktopLyrics();
		}

		private void CbEnableEmbeddedLyrics_Checked(object sender, RoutedEventArgs e)
		{
			(Owner as DoubanFMWindow).ShowEmbeddedLyrics();
		}

		private void CbEnableEmbeddedLyrics_Unchecked(object sender, RoutedEventArgs e)
		{
			(Owner as DoubanFMWindow).HideEmbeddedLyrics();
		}

	}
}
