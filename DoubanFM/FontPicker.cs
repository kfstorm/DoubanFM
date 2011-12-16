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

namespace DoubanFM
{
	[TemplatePart(Name = "PART_FontComboBox", Type = typeof(ComboBox))]
	public class FontPicker : Control
	{
		static FontPicker()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(FontPicker), new FrameworkPropertyMetadata(typeof(FontPicker)));
		}

		public FontFamily Font
		{
			get { return (FontFamily)GetValue(FontProperty); }
			set { SetValue(FontProperty, value); }
		}

		public static readonly DependencyProperty FontProperty =
			DependencyProperty.Register("Font", typeof(FontFamily), typeof(FontPicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnFontChanged)));

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
			(d as FontPicker).RaiseEvent(new RoutedPropertyChangedEventArgs<FontFamily>((FontFamily)e.OldValue, (FontFamily)e.NewValue, FontChangedEvent));
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

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			CbFont = this.Template.FindName("PART_FontComboBox", this) as ComboBox;
			CbFont.SelectionChanged += new SelectionChangedEventHandler(CbFont_SelectionChanged);
			CbFont.KeyUp += new KeyEventHandler(CbFont_KeyUp);
			
			var fontFamilies = Fonts.SystemFontFamilies;
			List<string> names = new List<string>();
			string selectedName = null;
			foreach (var fontFamily in fontFamilies)
			{
				string name = GetFontName(fontFamily);
				names.Add(name);
				if (name == GetFontName(Font))
					selectedName = name;
			}
			names.Sort();
			CbFont.ItemsSource = names;
			if (Font != null)
				CbFont.Text = Font.ToString();
		}

		void CbFont_KeyUp(object sender, KeyEventArgs e)
		{
			UpdateFontFamily();
		}

		void CbFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//引发SelectionChanged事件时Text属性还未政变，所以要延迟读取Text属性的值
			if (e.AddedItems.Count == 0)
			{
				Dispatcher.BeginInvoke(new Action(() => { UpdateFontFamily(); }));
			}
			else
			{
				Font = new FontFamily(e.AddedItems[0] as string);
			}
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
				Font = null;
			}
		}
	}
}