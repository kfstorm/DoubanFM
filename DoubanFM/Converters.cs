/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System.Collections.Generic;
using System.Windows.Data;
using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using DoubanFM.Core;

namespace DoubanFM
{
	/// <summary>
	/// 音乐进度条值转换器
	/// </summary>
	public class SliderThumbWidthConverter : IMultiValueConverter
	{
		/// <summary>
		/// 由Slider的Value值确定Slider的Thumb宽度
		/// </summary>
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			double Value = (double)values[0];
			double Minimum = (double)values[1];
			double Maximum = (double)values[2];
			double ret;
			if (Math.Abs(Maximum - Minimum) < 1e-5)
				ret = 0;
			else
				ret = (Value - Minimum) / (Maximum - Minimum);
			if (ret < 0)
				ret = 0;
			if (ret > 1)
				ret = 1;
			return ret;
		}
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	/// <summary>
	/// 时间信息值转换器
	/// </summary>
	public class TimeSpanToStringConverter : IValueConverter
	{
		private static TimeSpanToStringConverter converter = new TimeSpanToStringConverter();
		/// <summary>
		/// 由TimeSpan值转换为字符串，用于时间显示
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			TimeSpan timespan = (TimeSpan)value;
			return (timespan.Hours > 0 ? timespan.Hours.ToString() + ":" : "") + (timespan.Minutes < 10 ? "0" : "") + timespan.Minutes + ":" + (timespan.Seconds < 10 ? "0" : "") + timespan.Seconds;
		}
		/// <summary>
		/// 由时间字符串转换回TimeSpan值
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string value2 = "00:" + (string)value;
			return TimeSpan.Parse((string)value2);
		}
		/// <summary>
		/// 从时间到字符串的静态转换方法
		/// </summary>
		/// <param name="value">时间</param>
		/// <returns>格式化后的字符串</returns>
		public static string QuickConvert(TimeSpan value)
		{
			return (string)converter.Convert(value, typeof(string), null, null);
		}
		/// <summary>
		/// 从字符串到时间的静态转换方法
		/// </summary>
		/// <param name="value">格式化后的字符串</param>
		/// <returns>时间</returns>
		public static TimeSpan QuickConvertBack(string value)
		{
			return (TimeSpan)converter.ConvertBack(value, typeof(TimeSpan), null, null);
		}
	}
	/// <summary>
	/// 窗口背景到进度条前景的值转换器
	/// </summary>
	public class WindowBackgroundToSliderForegroundConverter : IValueConverter
	{
		/// <summary>
		/// 由窗口背景转换为进度条前景
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is SolidColorBrush)
			{
				SolidColorBrush brush = new SolidColorBrush(ColorFunctions.ReviseBrighter(new HslColor(((SolidColorBrush)value).Color), ColorFunctions.ProgressBarReviseParameter).ToRgb());
				brush.Opacity = ((SolidColorBrush)value).Opacity;
				if (brush.CanFreeze) brush.Freeze();
				return brush;
			}
			return value;
		}
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// 将Bool值反转的转换器
	/// </summary>
	public class BoolReverseConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return !(bool)value;
		}
	}

	/// <summary>
	/// 窗口背景到桌面歌词前景的值转换器
	/// </summary>
	public class BackgroundToLyricsForegroundConverter : IValueConverter
	{
		/// <summary>
		/// 由窗口背景转换为桌面歌词前景
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is SolidColorBrush)
			{
				HslColor color = new HslColor(((SolidColorBrush)value).Color);
				color.Alpha = 1.0;
				if (color.Lightness < 0.7) color.Lightness = 0.7;
				SolidColorBrush brush = new SolidColorBrush(color.ToRgb());
				if (brush.CanFreeze) brush.Freeze();
				return brush;
			}
			return value;
		}
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// OpenTypeWeight到FontWeight类型的值转换器
	/// </summary>
	public class OpenTypeWeightToFontWeightConverter : IValueConverter
	{
		/// <summary>
		/// 由OpenTypeWeight转换为FontWeight类型
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ((int)value > FontWeights.UltraBold.ToOpenTypeWeight())
				return FontWeights.UltraBold;
			if ((int)value < FontWeights.Thin.ToOpenTypeWeight())
				return FontWeights.Thin;
			return FontWeight.FromOpenTypeWeight((int)value);
		}
		/// <summary>
		/// 由FontWeight类型转换为OpenTypeWeight
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return ((FontWeight)value).ToOpenTypeWeight();
		}
	}

	/// <summary>
	/// 将比例转换为值的转换器
	/// </summary>
	public class RatioToValueConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				double ratio = (double)values[0];
				double total = (double)values[1];
				return (double)total * (double)ratio;
			}
			catch
			{
				return 0;
			}
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// 将不透明度转换为透明度的转换器
	/// </summary>
	public class OpacityToTransparencyConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return 1.0 - (double)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return 1.0 - (double)value;
		}
	}
	/// <summary>
	/// 将透明度转换为不透明度的转换器，避免完全透明
	/// </summary>
	public class TransparencyToOpacityAndPreventFullTransparentConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ((double)value == 1)
				return 0.01;
			return 1.0 - (double)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ((double)value == 0)
				return 0.99;
			return 1.0 - (double)value;
		}
	}
	/// <summary>
	/// 将任意颜色转换为RGB值相同的透明色
	/// </summary>
	public class ToTransparentColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return Color.FromArgb(0, ((Color)value).R, ((Color)value).G, ((Color)value).B);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// 将HorizontalAlignment转换为RenderTransformOrigin
	/// </summary>
	public class HorizontalAlignmentToRenderTransformOriginConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			switch ((HorizontalAlignment)value)
			{
				case HorizontalAlignment.Center:
				case HorizontalAlignment.Stretch:
					return new Point(0.5, 0.5);
				case HorizontalAlignment.Left:
					return new Point(0, 0.5);
				case HorizontalAlignment.Right:
					return new Point(1, 0.5);
				default:
					return new Point(0.5, 0.5);
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// 将HorizontalAlignment转换为字符串
	/// </summary>
	public class HorizontalAlignmentToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			switch ((HorizontalAlignment)value)
			{
				case HorizontalAlignment.Center:
                    return DoubanFM.Resources.Resources.CenterAlignment;
				case HorizontalAlignment.Left:
                    return DoubanFM.Resources.Resources.LeftAlignment;
				case HorizontalAlignment.Right:
                    return DoubanFM.Resources.Resources.RightAlignment;
				case HorizontalAlignment.Stretch:
                    return DoubanFM.Resources.Resources.StretchAlignment;
				default:
					return string.Empty;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// 将VerticalAlignment转换为字符串
	/// </summary>
	public class VerticalAlignmentToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			switch ((VerticalAlignment)value)
			{
				case VerticalAlignment.Center:
                    return DoubanFM.Resources.Resources.CenterAlignment;
				case VerticalAlignment.Top:
                    return DoubanFM.Resources.Resources.TopAlignment;
				case VerticalAlignment.Bottom:
                    return DoubanFM.Resources.Resources.BottomAlignment;
				case VerticalAlignment.Stretch:
                    return DoubanFM.Resources.Resources.StretchAlignment;
				default:
                    return string.Empty;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// 将Share.Sites转换为字符串
	/// </summary>
	public class ShareSitesToStringConverter : IValueConverter
	{
        public static Dictionary<Core.Share.Sites, string> SiteName;

        static ShareSitesToStringConverter()
        {
            SiteName = new Dictionary<Share.Sites, string>
                           {
                                { Core.Share.Sites.Douban, Resources.Resources.ShareSiteDouban },
                                { Core.Share.Sites.Facebook, Resources.Resources.ShareSiteFacebook },
                                { Core.Share.Sites.Fanfou, Resources.Resources.ShareSiteFanfou },
                                { Core.Share.Sites.Kaixin, Resources.Resources.ShareSiteKaixin },
                                { Core.Share.Sites.Msn, Resources.Resources.ShareSiteMsn },
                                { Core.Share.Sites.Renren, Resources.Resources.ShareSiteRenren },
                                { Core.Share.Sites.TencentWeibo, Resources.Resources.ShareSiteTencentWeibo },
                                { Core.Share.Sites.Twitter, Resources.Resources.ShareSiteTwitter },
                                { Core.Share.Sites.Weibo, Resources.Resources.ShareSiteWeibo },
                                { Core.Share.Sites.Qzone, Resources.Resources.ShareSiteQzone },
                                { Core.Share.Sites.None, Resources.Resources.ShareSiteNone }
                           };
        }

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (Enum.IsDefined(typeof(Core.Share.Sites), value))
			{
				return SiteName[(Core.Share.Sites)value];
			}
			else
			{
				return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// 根据频道确定能否使用收藏功能
	/// </summary>
	public class ChannelToCanFavoriteVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Core.Channel channel = value as Core.Channel;
			if (channel == null)
			{
				channel = (value as Core.ChannelSearchItem).GetChannel();
			}
			if (channel == null)
			{
				return Visibility.Collapsed;
			}
			bool canFavorite = canFavorite = !channel.IsPersonal || channel.IsSpecial;
			return canFavorite ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class TextBoxHintConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool isFocused = (bool)values[0];
			string text = (string)values[1];
			bool canShow = !isFocused && string.IsNullOrEmpty(text);
			return canShow ? Visibility.Visible : Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// 根据一个Boolean值的真假选择两个值中的一个
	/// </summary>
	public class BooleanChooserConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ((bool)values[0])
			{
				return values[1];
			}
			else
			{
				return values[2];
			}
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}