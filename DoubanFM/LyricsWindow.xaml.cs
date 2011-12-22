/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using DoubanFM.Core;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Globalization;

namespace DoubanFM
{
	/// <summary>
	/// LyricsWindow.xaml 的交互逻辑
	/// </summary>
	public partial class LyricsWindow : Window
	{
		public static readonly DependencyProperty LyricsSettingProperty = DependencyProperty.Register("LyricsSetting", typeof(LyricsSetting), typeof(LyricsWindow));
		public LyricsSetting LyricsSetting
		{
			get { return (LyricsSetting)GetValue(LyricsSettingProperty); }
			set { SetValue(LyricsSettingProperty, value); }
		}

		private Lyrics _lyrics;
		/// <summary>
		/// 歌词分析器
		/// </summary>
		public Lyrics Lyrics
		{
			get { return _lyrics; }
			internal set
			{
				if (_lyrics != value)
				{
					_lyrics = value;
					_lyricsCurrentIndex = int.MinValue;
				}
			}
		}
		/// <summary>
		/// 当前歌词所在位置
		/// </summary>
		private int _lyricsCurrentIndex = int.MinValue;
		/// <summary>
		/// 各种无法在XAML里直接启动的Storyboard
		/// </summary>
		private Storyboard ChangeLyricsStoryboard, HideLyricsStoryboard;
		/// <summary>
		/// 歌词笔画
		/// </summary>
		private Geometry _textGeometry;

		private FormattedText _formattedText;
		
		[DllImport("user32.dll")]
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
		[DllImport("user32.dll", SetLastError = true)]
		static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		static readonly int GWL_EXSTYLE = (-20);
		static readonly int WS_EX_TRANSPARENT = 0x0020;
		static readonly int WS_EX_TOOLWINDOW = 0x00000080;

		public LyricsWindow(LyricsSetting lyricsSetting = null)
		{
			this.InitializeComponent();

			// 在此点之下插入创建对象所需的代码。
			LyricsSetting = lyricsSetting;

			ChangeLyricsStoryboard = (Storyboard)FindResource("ChangeLyricsStoryboard");
			HideLyricsStoryboard = (Storyboard)FindResource("HideLyricsStoryboard");

			this.SourceInitialized += new EventHandler((o, e) =>
			{
				var hwnd = new WindowInteropHelper(this).Handle;
				int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
				SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle
					//鼠标穿透
					| WS_EX_TRANSPARENT
					//在按下Alt+Tab时不显示
					| WS_EX_TOOLWINDOW);
			});

			UpdateForegroundSetting();

			Binding binding = new Binding();
			binding.Source = LyricsSetting;
			binding.Path = new PropertyPath(DoubanFM.LyricsSetting.FontFamilyProperty);
			this.SetBinding(LyricsFontFamilyProperty, binding);

			Binding binding2 = new Binding();
			binding2.Source = LyricsSetting;
			binding2.Path = new PropertyPath(DoubanFM.LyricsSetting.FontSizeProperty);
			this.SetBinding(LyricsFontSizeProperty, binding2);

			Binding binding3 = new Binding();
			binding3.Source = LyricsSetting;
			binding3.Path = new PropertyPath(DoubanFM.LyricsSetting.FontWeightProperty);
			binding3.Converter = new OpenTypeWeightToFontWeightConverter();
			this.SetBinding(LyricsFontWeightProperty, binding3);

			Binding binding4 = new Binding();
			binding4.Source = LyricsSetting;
			binding4.Path = new PropertyPath(DoubanFM.LyricsSetting.StrokeWeightProperty);
			this.SetBinding(LyricsStrokeWeightProperty, binding4);
		}

		/// <summary>
		/// 根据当前设置应用歌词颜色
		/// </summary>
		public void UpdateForegroundSetting()
		{
			if (LyricsSetting.AutoForeground)
			{
				SetAutoForeground();
			}
			else
			{
				SetManualForeground();
			}
		}

		/// <summary>
		/// 设置自动变换歌词颜色
		/// </summary>
		protected void SetAutoForeground()
		{
			Binding binding = new Binding();
			binding.Source = Application.Current.MainWindow;
			binding.Path = new PropertyPath(System.Windows.Window.BackgroundProperty);
			binding.Converter = new BackgroundToLyricsForegroundConverter();
			PathText.SetBinding(Path.FillProperty, binding);
		}

		/// <summary>
		/// 设置手动更换歌词颜色
		/// </summary>
		protected void SetManualForeground()
		{
			PathText.Fill = new SolidColorBrush();
			Binding binding = new Binding();
			binding.Source = LyricsSetting;
			binding.Path = new PropertyPath(DoubanFM.LyricsSetting.ForegroundProperty);
			BindingExpressionBase expression = BindingOperations.SetBinding(PathText.Fill, SolidColorBrush.ColorProperty, binding);
		}

		/// <summary>
		/// 更换歌词
		/// </summary>
		protected void ChangeLyrics(string newLyrics)
		{
			((StringAnimationUsingKeyFrames)ChangeLyricsStoryboard.Children[1]).KeyFrames[0].Value = newLyrics;
			ChangeLyricsStoryboard.Begin();
		}

		/// <summary>
		/// 隐藏歌词
		/// </summary>
		protected void HideLyrics()
		{
			HideLyricsStoryboard.Begin();
		}

		/// <summary>
		/// 按时间刷新歌词
		/// </summary>
		public void Refresh(TimeSpan time)
		{
			if (_lyrics != null)
			{
				_lyrics.Refresh(time + ((StringAnimationUsingKeyFrames)ChangeLyricsStoryboard.Children[1]).KeyFrames[0].KeyTime.TimeSpan);
				if (_lyrics.CurrentIndex != _lyricsCurrentIndex)
				{
					_lyricsCurrentIndex = _lyrics.CurrentIndex;
					ChangeLyrics(_lyrics.CurrentLyrics);
				}
			}
			else HideLyrics();
		}

		#region 绘制歌词

		public FontFamily LyricsFontFamily
		{
			get { return (FontFamily)GetValue(LyricsFontFamilyProperty); }
			set { SetValue(LyricsFontFamilyProperty, value); }
		}

		public static readonly DependencyProperty LyricsFontFamilyProperty =
			DependencyProperty.Register("LyricsFontFamily", typeof(FontFamily), typeof(LyricsWindow), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, new PropertyChangedCallback(OnLyricsFontFamilyChanged)));

		static void OnLyricsFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as LyricsWindow).UpdateText();
		}

		public double LyricsFontSize
		{
			get { return (double)GetValue(LyricsFontSizeProperty); }
			set { SetValue(LyricsFontSizeProperty, value); }
		}

		public static readonly DependencyProperty LyricsFontSizeProperty =
			DependencyProperty.Register("LyricsFontSize", typeof(double), typeof(LyricsWindow), new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, new PropertyChangedCallback(OnLyricsFontSizeChanged)));

		static void OnLyricsFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as LyricsWindow).UpdateText();
			(d as LyricsWindow).UpdateStrokeAndShadow();
		}

		public FontWeight LyricsFontWeight
		{
			get { return (FontWeight)GetValue(LyricsFontWeightProperty); }
			set { SetValue(LyricsFontWeightProperty, value); }
		}

		public static readonly DependencyProperty LyricsFontWeightProperty =
			DependencyProperty.Register("LyricsFontWeight", typeof(FontWeight), typeof(LyricsWindow), new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, new PropertyChangedCallback(OnLyricsFontWeightChanged)));

		static void OnLyricsFontWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as LyricsWindow).UpdateText();
		}

		public double LyricsStrokeWeight
		{
			get { return (double)GetValue(LyricsStrokeWeightProperty); }
			set { SetValue(LyricsStrokeWeightProperty, value); }
		}

		public static readonly DependencyProperty LyricsStrokeWeightProperty =
			DependencyProperty.Register("LyricsStrokeWeight", typeof(double), typeof(LyricsWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLyricsStrokeWeightChanged)));

		static void OnLyricsStrokeWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as LyricsWindow).UpdateStrokeAndShadow();
		}

		public string LyricsText
		{
			get { return (string)GetValue(LyricsTextProperty); }
			set { SetValue(LyricsTextProperty, value); }
		}

		public static readonly DependencyProperty LyricsTextProperty =
			DependencyProperty.Register("LyricsText", typeof(string), typeof(LyricsWindow), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnLyricsTextChanged)));

		static void OnLyricsTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as LyricsWindow).UpdateText();
		}

		/// <summary>
		/// 重绘歌词
		/// </summary>
		public void UpdateStrokeAndShadow()
		{
			PathText.StrokeThickness = LyricsStrokeWeight / 48.0 * LyricsFontSize;
			ShadowEffect.ShadowDepth = PathText.StrokeThickness;
			if (ShadowEffect.ShadowDepth == 0)
				ShadowEffect.Opacity = 0;
			else
				ShadowEffect.Opacity = 1;
		}

		public void UpdateText()
		{
			CreateText(LyricsText);
			PathText.Data = _textGeometry;
		}

		/// <summary>
		/// Create the outline geometry based on the formatted text.
		/// </summary>
		public void CreateText(string text)
		{
			// Create the formatted text based on the properties set.
			_formattedText = new FormattedText(
				text == null ? "" : text,
				CultureInfo.GetCultureInfo("zh-cn"),
				FlowDirection.LeftToRight,
				new Typeface(LyricsFontFamily == null ? SystemFonts.MessageFontFamily : LyricsFontFamily, FontStyles.Normal, LyricsFontWeight, FontStretches.Normal),
				LyricsFontSize,
				System.Windows.Media.Brushes.Black // This brush does not matter since we use the geometry of the text. 
				);
			
			// Build the geometry object that represents the text.
			_textGeometry = _formattedText.BuildGeometry(new System.Windows.Point(0, 0));
		}

		#endregion
	}
}