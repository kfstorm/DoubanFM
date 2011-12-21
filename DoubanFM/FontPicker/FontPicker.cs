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
using System.Globalization;

namespace DoubanFM
{
	[TemplatePart(Name = "PART_FontComboBox", Type = typeof(ComboBox))]
	public class FontPicker : Control
	{

		/// <summary>
		/// 获取字体名称（简体中文字体返回中文名称）（只适用于单一字体，不适用于组合字体）
		/// </summary>
		public static string GetFontName(FontFamily fontFamily, CultureInfo cultureInfo)
		{
			if (fontFamily == null) return string.Empty;
			if (fontFamily.FamilyNames.ContainsKey(System.Windows.Markup.XmlLanguage.GetLanguage(cultureInfo.Name)))
			{
				return fontFamily.FamilyNames[System.Windows.Markup.XmlLanguage.GetLanguage(cultureInfo.Name)];
			}
			return fontFamily.FamilyNames.First().Value;
		}

		public static string GetFontName(FontFamily fontFamily)
		{
			return GetFontName(fontFamily, CultureInfo.CurrentCulture);
		}

		static FontPicker()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(FontPicker), new FrameworkPropertyMetadata(typeof(FontPicker)));

			var fonts = from font in Fonts.SystemFontFamilies select GetFontName(font);
			SystemFonts = fonts.ToList();
			SystemFonts.Sort();
		}

		public FontFamily Font
		{
			get { return (FontFamily)GetValue(FontProperty); }
			set { SetValue(FontProperty, value); }
		}

		public static readonly DependencyProperty FontProperty =
			DependencyProperty.Register("Font", typeof(FontFamily), typeof(FontPicker), new FrameworkPropertyMetadata(System.Windows.SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnFontChanged)));

		public static void OnFontChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FontPicker picker = (FontPicker)d;
			if (e.NewValue != null && picker.CbFont != null)
			{
				string str = ((FontFamily)e.NewValue).ToString();
				if (picker.CbFont.Text.Replace('，', ',') != str)
				{
					picker.CbFont.Text = str;
				}
			}
			picker.RaiseEvent(new RoutedPropertyChangedEventArgs<FontFamily>((FontFamily)e.OldValue, (FontFamily)e.NewValue, FontChangedEvent));
		}

		public event RoutedPropertyChangedEventHandler<FontFamily> FontChanged
		{
			add
			{
				AddHandler(FontChangedEvent, value);
			}
			remove
			{
				RemoveHandler(FontChangedEvent, value);
			}
		}

		public static readonly RoutedEvent FontChangedEvent = EventManager.RegisterRoutedEvent("FontChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<FontFamily>), typeof(FontPicker));

		private ComboBox CbFont;

		public static readonly List<string> SystemFonts;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			CbFont = this.Template.FindName("PART_FontComboBox", this) as ComboBox;

			CbFont.ItemsSource = SystemFonts;
			CbFont.SelectionChanged += new SelectionChangedEventHandler(CbFont_SelectionChanged);
			CbFont.KeyUp += new KeyEventHandler(CbFont_KeyUp);
			if (Font != null)
			{
				CbFont.Text = Font.ToString();
			}
		}

		void CbFont_KeyUp(object sender, KeyEventArgs e)
		{
			UpdateFontFamily();
		}

		void CbFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//引发SelectionChanged事件时Text属性还未政变，所以要延迟读取Text属性的值
			Dispatcher.BeginInvoke(new Action(() => { UpdateFontFamily(); }));
		}

		/// <summary>
		/// 根据填写的字体名称更新字体设置
		/// </summary>
		protected void UpdateFontFamily()
		{
			try
			{
				Font = new FontFamily(CbFont.Text.Replace('，', ','));
			}
			catch
			{
				Font = System.Windows.SystemFonts.MessageFontFamily;
			}
		}
	}
}