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
	/// <summary>
	/// 字体选择器
	/// </summary>
	[TemplatePart(Name = "PART_FontComboBox", Type = typeof(ComboBox))]
	public class FontPicker : Control
	{

		/// <summary>
		/// 获取字体名称（根据指定的CultureInfo获取本地化的字体名称）（只适用于单一字体，不适用于组合字体）
		/// </summary>
		/// <param name="fontFamily">一系列字体</param>
		/// <param name="cultureInfo">指定的CultureInfo</param>
		/// <returns>
		/// 字体名称
		/// </returns>
		public static string GetFontName(FontFamily fontFamily, CultureInfo cultureInfo)
		{
			if (fontFamily == null) return string.Empty;
			if (fontFamily.FamilyNames.ContainsKey(System.Windows.Markup.XmlLanguage.GetLanguage(cultureInfo.Name)))
			{
				return fontFamily.FamilyNames[System.Windows.Markup.XmlLanguage.GetLanguage(cultureInfo.Name)];
			}
			return fontFamily.FamilyNames.First().Value;
		}

		/// <summary>
		/// 获取字体名称（根据当前线程的CultureInfo获取本地化的字体名称）（只适用于单一字体，不适用于组合字体）
		/// </summary>
		/// <param name="fontFamily">一系列字体</param>
		/// <returns>字体名称</returns>
		public static string GetFontName(FontFamily fontFamily)
		{
			return GetFontName(fontFamily, CultureInfo.CurrentCulture);
		}

		static FontPicker()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(FontPicker), new FrameworkPropertyMetadata(typeof(FontPicker)));

			//获取系统中已安装的字体
			var fonts = from font in Fonts.SystemFontFamilies select GetFontName(font);
			SystemFonts = fonts.ToList();
			SystemFonts.Sort();
		}

		/// <summary>
		/// 选中的字体
		/// </summary>
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
				//将全角逗号替换为半角逗号，以支持用全角逗号作为字体的分隔符
				if (picker.CbFont.Text.Replace('，', ',') != str)
				{
					picker.CbFont.Text = str;
				}
			}
			picker.RaiseEvent(new RoutedPropertyChangedEventArgs<FontFamily>((FontFamily)e.OldValue, (FontFamily)e.NewValue, FontChangedEvent));
		}

		/// <summary>
		/// 当选中的字体改变时发生。
		/// </summary>
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

		/// <summary>
		/// 用于显示和选择字体的控件
		/// </summary>
		private ComboBox CbFont;

		/// <summary>
		/// 系统中已安装字体的字符串列表
		/// </summary>
		public static readonly List<string> SystemFonts;

		/// <summary>
		/// 在派生类中重写后，每当应用程序代码或内部进程调用 <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>，都将调用此方法。
		/// </summary>
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

		/// <summary>
		/// 当释放一个按键时，Combobox中填写的内容可能改变，需要更新选择的字体
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
		void CbFont_KeyUp(object sender, KeyEventArgs e)
		{
			UpdateFontFamily();
		}

		/// <summary>
		/// 当Combobox中选择的项改变时，需要更新选择的字体
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
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