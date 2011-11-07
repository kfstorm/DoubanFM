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
		private FontFamily _lastFontFamily;
		private double _lastFontSize;
		private FontStyle _lastFontStyle;
		private FontWeight _lastFontWeight;
		private FontStretch _lastFontStretch;
		private double _lastStrokeWeight;

		[DllImport("user32.dll")]
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
		[DllImport("user32.dll", SetLastError = true)]
		static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		static readonly int GWL_EXSTYLE = (-20);
		static readonly int WS_EX_TRANSPARENT = 0x0020;

		public LyricsWindow(LyricsSetting lyricsSetting = null)
		{
			this.InitializeComponent();

			// 在此点之下插入创建对象所需的代码。
			LyricsSetting = lyricsSetting;
		
			ChangeLyricsStoryboard = (Storyboard)FindResource("ChangeLyricsStoryboard");
			HideLyricsStoryboard = (Storyboard)FindResource("HideLyricsStoryboard");

			//鼠标穿透
			this.SourceInitialized += new EventHandler((o, e) =>
			{
				var hwnd = new WindowInteropHelper(this).Handle;
				int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
				SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
			});
		}

		private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			// 在此处添加事件处理程序实现。
			this.DragMove();
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

		/// <summary>
		/// 重绘歌词
		/// </summary>
		public void Update()
		{
			if (_formattedText == null || _formattedText.Text != CurrentLyrics.Text
				|| _lastFontFamily != CurrentLyrics.FontFamily
				|| _lastFontSize != CurrentLyrics.FontSize
				|| _lastFontStretch != CurrentLyrics.FontStretch
				|| _lastFontStyle != CurrentLyrics.FontStyle
				|| _lastFontWeight != CurrentLyrics.FontWeight
				|| _lastStrokeWeight != LyricsSetting.StrokeWeight)
			{
				CreateText(CurrentLyrics.Text);
				PathText.Data = _textGeometry;
				PathText.StrokeThickness = LyricsSetting.StrokeWeight / 48.0 * CurrentLyrics.FontSize;
				ShadowEffect.ShadowDepth = PathText.StrokeThickness;
				if (ShadowEffect.ShadowDepth == 0)
					ShadowEffect.Opacity = 0;
				else
					ShadowEffect.Opacity = 1;
			}
		}

		/// <summary>
		/// Create the outline geometry based on the formatted text.
		/// </summary>
		public void CreateText(string text)
		{
			_lastFontFamily = CurrentLyrics.FontFamily;
			_lastFontSize = CurrentLyrics.FontSize;
			_lastFontStretch = CurrentLyrics.FontStretch;
			_lastFontStyle = CurrentLyrics.FontStyle;
			_lastFontWeight = CurrentLyrics.FontWeight;
			_lastStrokeWeight = LyricsSetting.StrokeWeight;

			// Create the formatted text based on the properties set.
			_formattedText = new FormattedText(
				text == null ? "" : text,
				CultureInfo.GetCultureInfo("zh-cn"),
				FlowDirection.LeftToRight,
				new Typeface(_lastFontFamily, _lastFontStyle, _lastFontWeight, _lastFontStretch),
				_lastFontSize,
				System.Windows.Media.Brushes.Black // This brush does not matter since we use the geometry of the text. 
				);

			// Build the geometry object that represents the text.
			_textGeometry = _formattedText.BuildGeometry(new System.Windows.Point(0, 0));
		}

		private void CurrentLyrics_LayoutUpdated(object sender, EventArgs e)
		{
			Update();
		}
	}
}