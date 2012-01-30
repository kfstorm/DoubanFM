/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace DoubanFM
{
	/// <summary>
	/// 歌词设置
	/// </summary>
	[Serializable]
	public class LyricsSetting : DependencyObject, ISerializable
	{
		public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(LyricsSetting), new PropertyMetadata(new FontFamily("微软雅黑,华文新魏,楷体")));
		public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(LyricsSetting), new PropertyMetadata(32.0));
		public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register("FontWeight", typeof(int), typeof(LyricsSetting), new PropertyMetadata(System.Windows.FontWeights.ExtraBlack.ToOpenTypeWeight()));
		public static readonly DependencyProperty StrokeWeightProperty = DependencyProperty.Register("StrokeWeight", typeof(double), typeof(LyricsSetting), new PropertyMetadata(2.0));
		public static readonly DependencyProperty BottomMarginProperty = DependencyProperty.Register("BottomMargin", typeof(double), typeof(LyricsSetting), new PropertyMetadata(0.0));
		public static readonly DependencyProperty EnableDesktopLyricsProperty = DependencyProperty.Register("EnableDesktopLyrics", typeof(bool), typeof(LyricsSetting), new PropertyMetadata(true));
		public static readonly DependencyProperty EnableEmbeddedLyricsProperty = DependencyProperty.Register("EnableEmbeddedLyrics", typeof(bool), typeof(LyricsSetting));
		public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register("Opacity", typeof(double), typeof(LyricsSetting), new PropertyMetadata(1.0));
		public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Color), typeof(LyricsSetting), new PropertyMetadata(Color.FromArgb(0x40, 0, 0, 0)));
		public static readonly DependencyProperty StrokeColorProperty = DependencyProperty.Register("StrokeColor", typeof(Color), typeof(LyricsSetting), new PropertyMetadata(Color.FromArgb(0xFF, 0, 0xFF, 0xFF)));
		public static readonly DependencyProperty ShadowColorProperty = DependencyProperty.Register("ShadowColor", typeof(Color), typeof(LyricsSetting), new PropertyMetadata(Color.FromArgb(0xFF, 0, 0, 0xFF)));
		public static readonly DependencyProperty AutoForegroundProperty = DependencyProperty.Register("AutoForeground", typeof(bool), typeof(LyricsSetting), new PropertyMetadata(false));
		public static readonly DependencyProperty SingleLineLyricsProperty = DependencyProperty.Register("SingleLineLyrics", typeof(bool), typeof(LyricsSetting), new PropertyMetadata(false));
		
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
		/// 底部边距
		/// </summary>
		public double BottomMargin
		{
			get { return (double)GetValue(BottomMarginProperty); }
			set { SetValue(BottomMarginProperty, value); }
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
				using (FileStream stream = File.OpenWrite(Path.Combine(_dataFolder,"LyricsSetting.dat")))
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
			try
			{
				FontFamily = new FontFamily(info.GetString("FontFamily"));
			}
			catch
			{
				FontFamily = def.FontFamily;
			}
			try
			{
				FontSize = info.GetDouble("FontSize");
			}
			catch
			{
				FontSize = def.FontSize;
			}
			try
			{
				FontWeight = info.GetInt32("FontWeight");
			}
			catch
			{
				FontWeight = def.FontWeight;
			}
			try
			{
				StrokeWeight = info.GetDouble("StrokeWeight");
			}
			catch
			{
				StrokeWeight = def.StrokeWeight;
			}
			try
			{
				BottomMargin = info.GetDouble("BottomMargin");
			}
			catch
			{
				BottomMargin = def.BottomMargin;
			}
			try
			{
				EnableDesktopLyrics = info.GetBoolean("EnableDesktopLyrics");
			}
			catch
			{
				EnableDesktopLyrics = def.EnableDesktopLyrics;
			}
			try
			{
				EnableEmbeddedLyrics = info.GetBoolean("EnableEmbeddedLyrics");
			}
			catch
			{
				EnableEmbeddedLyrics = def.EnableEmbeddedLyrics;
			}
			try
			{
				Opacity = info.GetDouble("Opacity");
			}
			catch
			{
				Opacity = def.Opacity;
			}
			try
			{
				Foreground = (Color)ColorConverter.ConvertFromString(info.GetString("Foreground"));
			}
			catch
			{
				Foreground = def.Foreground;
			}
			try
			{
				StrokeColor = (Color)ColorConverter.ConvertFromString(info.GetString("StrokeColor"));
			}
			catch
			{
				StrokeColor = def.StrokeColor;
			}
			try
			{
				ShadowColor = (Color)ColorConverter.ConvertFromString(info.GetString("ShadowColor"));
			}
			catch
			{
				ShadowColor = def.ShadowColor;
			}
			try
			{
				AutoForeground = info.GetBoolean("AutoForeground");
			}
			catch
			{
				AutoForeground = def.AutoForeground;
			}
			try
			{
				SingleLineLyrics = info.GetBoolean("SingleLineLyrics");
			}
			catch
			{
				SingleLineLyrics = def.SingleLineLyrics;
			}
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
			info.AddValue("BottomMargin", BottomMargin);
			info.AddValue("EnableDesktopLyrics", EnableDesktopLyrics);
			info.AddValue("EnableEmbeddedLyrics", EnableEmbeddedLyrics);
			info.AddValue("Opacity", Opacity);
			info.AddValue("Foreground", Foreground.ToString());
			info.AddValue("StrokeColor", StrokeColor.ToString());
			info.AddValue("ShadowColor", ShadowColor.ToString());
			info.AddValue("AutoForeground", AutoForeground);
			info.AddValue("SingleLineLyrics", SingleLineLyrics);
		}
	}
}
