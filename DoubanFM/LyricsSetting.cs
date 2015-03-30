/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace DoubanFM
{
	/// <summary>
	/// 歌词设置
	/// </summary>
	[Serializable]
	public class LyricsSetting : DependencyObject, ISerializable
	{
		public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(LyricsSetting), new PropertyMetadata(new FontFamily("Segoe UI,楷体")));
		public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(LyricsSetting), new PropertyMetadata(28.0));
		public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register("FontWeight", typeof(int), typeof(LyricsSetting), new PropertyMetadata(System.Windows.FontWeights.Normal.ToOpenTypeWeight()));
		public static readonly DependencyProperty StrokeWeightProperty = DependencyProperty.Register("StrokeWeight", typeof(double), typeof(LyricsSetting), new PropertyMetadata(3.0));
		public static readonly DependencyProperty TopMarginProperty = DependencyProperty.Register("TopMargin", typeof(double), typeof(LyricsSetting), new PropertyMetadata(0.0));
		public static readonly DependencyProperty BottomMarginProperty = DependencyProperty.Register("BottomMargin", typeof(double), typeof(LyricsSetting), new PropertyMetadata(0.0));
		public static readonly DependencyProperty LeftMarginProperty = DependencyProperty.Register("LeftMargin", typeof(double), typeof(LyricsSetting), new PropertyMetadata(0.05));
		public static readonly DependencyProperty RightMarginProperty = DependencyProperty.Register("RightMargin", typeof(double), typeof(LyricsSetting), new PropertyMetadata(0.05));
		public static readonly DependencyProperty EnableDesktopLyricsProperty = DependencyProperty.Register("EnableDesktopLyrics", typeof(bool), typeof(LyricsSetting), new PropertyMetadata(true));
		public static readonly DependencyProperty EnableEmbeddedLyricsProperty = DependencyProperty.Register("EnableEmbeddedLyrics", typeof(bool), typeof(LyricsSetting));
		public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register("Opacity", typeof(double), typeof(LyricsSetting), new PropertyMetadata(1.0));
		public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Color), typeof(LyricsSetting), new PropertyMetadata(Colors.White));
		public static readonly DependencyProperty StrokeColorProperty = DependencyProperty.Register("StrokeColor", typeof(Color), typeof(LyricsSetting), new PropertyMetadata(Color.FromArgb(0x22, 0, 0, 0)));
		public static readonly DependencyProperty ShadowColorProperty = DependencyProperty.Register("ShadowColor", typeof(Color), typeof(LyricsSetting), new PropertyMetadata(Colors.Black));
		public static readonly DependencyProperty AutoForegroundProperty = DependencyProperty.Register("AutoForeground", typeof(bool), typeof(LyricsSetting), new PropertyMetadata(false));
		public static readonly DependencyProperty SingleLineLyricsProperty = DependencyProperty.Register("SingleLineLyrics", typeof(bool), typeof(LyricsSetting), new PropertyMetadata(false));
		public static readonly DependencyProperty HorizontalAlignmentProperty = DependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), typeof(LyricsSetting), new PropertyMetadata(HorizontalAlignment.Center));
		public static readonly DependencyProperty VerticalAlignmentProperty = DependencyProperty.Register("VerticalAlignment", typeof(VerticalAlignment), typeof(LyricsSetting), new PropertyMetadata(VerticalAlignment.Bottom));
		public static readonly DependencyProperty ForceTopMostProperty = DependencyProperty.Register("ForceTopMost", typeof(bool), typeof(LyricsSetting), new PropertyMetadata(false));
		public static readonly DependencyProperty HideWhenPauseProperty = DependencyProperty.Register("HideWhenPause", typeof(bool), typeof(LyricsSetting), new PropertyMetadata(true));
		public static readonly DependencyProperty DesktopLyricsScreenProperty = DependencyProperty.Register("DesktopLyricsScreen", typeof(string), typeof(LyricsSetting), new PropertyMetadata(Screen.PrimaryScreen.DeviceName));

		/// <summary>
		/// 字体
		/// </summary>
		public FontFamily FontFamily
		{
			get { return (FontFamily)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		/// <summary>
		/// 大小
		/// </summary>
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		/// <summary>
		/// 粗细
		/// </summary>
		public int FontWeight
		{
			get { return (int)GetValue(FontWeightProperty); }
			set { SetValue(FontWeightProperty, value); }
		}

		/// <summary>
		/// 描边粗细
		/// </summary>
		public double StrokeWeight
		{
			get { return (double)GetValue(StrokeWeightProperty); }
			set { SetValue(StrokeWeightProperty, value); }
		}

		/// <summary>
		/// 顶部边距
		/// </summary>
		public double TopMargin
		{
			get { return (double)GetValue(TopMarginProperty); }
			set { SetValue(TopMarginProperty, value); }
		}

		/// <summary>
		/// 底部边距
		/// </summary>
		public double BottomMargin
		{
			get { return (double)GetValue(BottomMarginProperty); }
			set { SetValue(BottomMarginProperty, value); }
		}

		/// <summary>
		/// 左边距
		/// </summary>
		public double LeftMargin
		{
			get { return (double)GetValue(LeftMarginProperty); }
			set { SetValue(LeftMarginProperty, value); }
		}

		/// <summary>
		/// 右边距
		/// </summary>
		public double RightMargin
		{
			get { return (double)GetValue(RightMarginProperty); }
			set { SetValue(RightMarginProperty, value); }
		}

		/// <summary>
		/// 是否开启桌面歌词
		/// </summary>
		public bool EnableDesktopLyrics
		{
			get { return (bool)GetValue(EnableDesktopLyricsProperty); }
			set { SetValue(EnableDesktopLyricsProperty, value); }
		}

		/// <summary>
		/// 是否开启内嵌歌词
		/// </summary>
		public bool EnableEmbeddedLyrics
		{
			get { return (bool)GetValue(EnableEmbeddedLyricsProperty); }
			set { SetValue(EnableEmbeddedLyricsProperty, value); }
		}

		/// <summary>
		/// 不透明度
		/// </summary>
		public double Opacity
		{
			get { return (double)GetValue(OpacityProperty); }
			set { SetValue(OpacityProperty, value); }
		}

		/// <summary>
		/// 前景色
		/// </summary>
		public Color Foreground
		{
			get { return (Color)GetValue(ForegroundProperty); }
			set { SetValue(ForegroundProperty, value); }
		}

		/// <summary>
		/// 描边色
		/// </summary>
		public Color StrokeColor
		{
			get { return (Color)GetValue(StrokeColorProperty); }
			set { SetValue(StrokeColorProperty, value); }
		}

		/// <summary>
		/// 阴影色
		/// </summary>
		public Color ShadowColor
		{
			get { return (Color)GetValue(ShadowColorProperty); }
			set { SetValue(ShadowColorProperty, value); }
		}

		/// <summary>
		/// 自动前景
		/// </summary>
		public bool AutoForeground
		{
			get { return (bool)GetValue(AutoForegroundProperty); }
			set { SetValue(AutoForegroundProperty, value); }
		}

		/// <summary>
		/// 启用单行歌词
		/// </summary>
		public bool SingleLineLyrics
		{
			get { return (bool)GetValue(SingleLineLyricsProperty); }
			set { SetValue(SingleLineLyricsProperty, value); }
		}

		/// <summary>
		/// 水平对齐方式
		/// </summary>
		public HorizontalAlignment HorizontalAlignment
		{
			get { return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty); }
			set { SetValue(HorizontalAlignmentProperty, value); }
		}

		/// <summary>
		/// 垂直对齐方式
		/// </summary>
		public VerticalAlignment VerticalAlignment
		{
			get { return (VerticalAlignment)GetValue(VerticalAlignmentProperty); }
			set { SetValue(VerticalAlignmentProperty, value); }
		}

		/// <summary>
		/// 是否强力置顶
		/// </summary>
		public bool ForceTopMost
		{
			get { return (bool)GetValue(ForceTopMostProperty); }
			set { SetValue(ForceTopMostProperty, value); }
		}

		/// <summary>
		/// 暂停时隐藏歌词
		/// </summary>
		public bool HideWhenPause
		{
			get { return (bool)GetValue(HideWhenPauseProperty); }
			set { SetValue(HideWhenPauseProperty, value); }
		}

		/// <summary>
		/// 显示桌面歌词的显示器名称
		/// </summary>
		public string DesktopLyricsScreen
		{
			get { return (string)GetValue(DesktopLyricsScreenProperty); }
			set { SetValue(DesktopLyricsScreenProperty, value); }
		}

		/// <summary>
		/// 数据保存文件夹
		/// </summary>
		private static string _dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"K.F.Storm\豆瓣电台");

		/// <summary>
		/// 加载设置
		/// </summary>
		internal static LyricsSetting Load()
		{
			LyricsSetting setting = null;
			try
			{
				using (FileStream stream = File.OpenRead(Path.Combine(_dataFolder, "LyricsSetting.dat")))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					setting = (LyricsSetting)formatter.Deserialize(stream);
				}
			}
			catch
			{
				setting = new LyricsSetting();
			}
			return setting;
		}

		/// <summary>
		/// 保存设置
		/// </summary>
		internal void Save()
		{
			try
			{
				if (!Directory.Exists(_dataFolder))
					Directory.CreateDirectory(_dataFolder);
				using (FileStream stream = File.OpenWrite(Path.Combine(_dataFolder, "LyricsSetting.dat")))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, this);
				}
			}
			catch { }
		}

		public LyricsSetting()
		{
		}

		protected LyricsSetting(SerializationInfo info, StreamingContext context)
		{
			LyricsSetting def = new LyricsSetting();
			try { FontFamily = new FontFamily(info.GetString("FontFamily")); }
			catch { }
			try { FontSize = info.GetDouble("FontSize"); }
			catch { }
			try { FontWeight = info.GetInt32("FontWeight"); }
			catch { }
			try { StrokeWeight = info.GetDouble("StrokeWeight"); }
			catch { }
			try { TopMargin = info.GetDouble("TopMargin"); }
			catch { }
			try { BottomMargin = info.GetDouble("BottomMargin"); }
			catch { }
			try { LeftMargin = info.GetDouble("LeftMargin"); }
			catch { }
			try { RightMargin = info.GetDouble("RightMargin"); }
			catch { }
			try { EnableDesktopLyrics = info.GetBoolean("EnableDesktopLyrics"); }
			catch { }
			try { EnableEmbeddedLyrics = info.GetBoolean("EnableEmbeddedLyrics"); }
			catch { }
			try { Opacity = info.GetDouble("Opacity"); }
			catch { }
			try { Foreground = (Color)ColorConverter.ConvertFromString(info.GetString("Foreground")); }
			catch { }
			try { StrokeColor = (Color)ColorConverter.ConvertFromString(info.GetString("StrokeColor")); }
			catch { }
			try { ShadowColor = (Color)ColorConverter.ConvertFromString(info.GetString("ShadowColor")); }
			catch { }
			try { AutoForeground = info.GetBoolean("AutoForeground"); }
			catch { }
			try { SingleLineLyrics = info.GetBoolean("SingleLineLyrics"); }
			catch { }
			try { HorizontalAlignment = (HorizontalAlignment)info.GetValue("HorizontalAlignment", typeof(HorizontalAlignment)); }
			catch { }
			try { VerticalAlignment = (VerticalAlignment)info.GetValue("VerticalAlignment", typeof(VerticalAlignment)); }
			catch { }
			try { ForceTopMost = info.GetBoolean("ForceTopMost"); }
			catch { }
			try { HideWhenPause = info.GetBoolean("HideWhenPause"); }
			catch { }
			try { DesktopLyricsScreen = info.GetString("DesktopLyricsScreen"); }
			catch { }
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (FontFamily != null)
			{
				info.AddValue("FontFamily", FontFamily.ToString());
			}
			info.AddValue("FontSize", FontSize);
			info.AddValue("FontWeight", FontWeight);
			info.AddValue("StrokeWeight", StrokeWeight);
			info.AddValue("TopMargin", TopMargin);
			info.AddValue("BottomMargin", BottomMargin);
			info.AddValue("LeftMargin", LeftMargin);
			info.AddValue("RightMargin", RightMargin);
			info.AddValue("EnableDesktopLyrics", EnableDesktopLyrics);
			info.AddValue("EnableEmbeddedLyrics", EnableEmbeddedLyrics);
			info.AddValue("Opacity", Opacity);
			info.AddValue("Foreground", Foreground.ToString());
			info.AddValue("StrokeColor", StrokeColor.ToString());
			info.AddValue("ShadowColor", ShadowColor.ToString());
			info.AddValue("AutoForeground", AutoForeground);
			info.AddValue("SingleLineLyrics", SingleLineLyrics);
			info.AddValue("HorizontalAlignment", HorizontalAlignment);
			info.AddValue("VerticalAlignment", VerticalAlignment);
			info.AddValue("ForceTopMost", ForceTopMost);
			info.AddValue("HideWhenPause", HideWhenPause);
			info.AddValue("DesktopLyricsScreen", DesktopLyricsScreen);
		}
	}
}