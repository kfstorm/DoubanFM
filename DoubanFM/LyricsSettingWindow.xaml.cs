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
			CbFontFamily.SelectedItem = selectedName;
			
			List<FontWeight> weights = new List<FontWeight>();
			var properties = typeof(FontWeights).GetProperties();
			foreach (var property in properties)
			{

				if (property.PropertyType == typeof(FontWeight))
				{
					weights.Add((FontWeight)property.GetValue(null, null));
				}
			}
			CbFontWeight.ItemsSource = weights.Distinct();
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
			if (CbFontFamily.SelectedItem != null)
				LyricsSetting.FontFamily = new FontFamily(CbFontFamily.SelectedItem as string);
			else LyricsSetting.FontFamily = null;
		}
	}
}
