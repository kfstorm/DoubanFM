/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System.Windows.Data;
using System;
using System.Windows.Media;

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
				SolidColorBrush brush = new SolidColorBrush(ColorFunctions.ReviseBrighter(new HSLColor(((SolidColorBrush)value).Color), ColorFunctions.ProgressBarReviseParameter).ToRGB());
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
	/// 将Bool值转换为Visibility的转换器
	/// </summary>
	public class BoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!(bool)value)
			{
				if (parameter != null)
					return parameter;
				return System.Windows.Visibility.Hidden;
			}
			else
			{
				return System.Windows.Visibility.Visible;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			switch ((System.Windows.Visibility)value)
			{
				case System.Windows.Visibility.Collapsed:
				case System.Windows.Visibility.Hidden:
					return false;
				case System.Windows.Visibility.Visible:
					return true;
				default:
					return true;
			}
		}
	}
	/// <summary>
	/// 将Bool值反转并转换为Visibility的转换器
	/// </summary>
	public class BoolReverseToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ((bool)value)
			{
				if (parameter != null)
					return parameter;
				return System.Windows.Visibility.Hidden;
			}
			else
			{
				return System.Windows.Visibility.Visible;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			switch ((System.Windows.Visibility)value)
			{
				case System.Windows.Visibility.Collapsed:
				case System.Windows.Visibility.Hidden:
					return true;
				case System.Windows.Visibility.Visible:
					return false;
				default:
					return true;
			}
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
				HSLColor color = new HSLColor(((SolidColorBrush)value).Color);
				color.Alpha = 1.0;
				if (color.Lightness < 0.7) color.Lightness = 0.7;
				SolidColorBrush brush = new SolidColorBrush(color.ToRGB());
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
	/// OpenTypeWeight到System.Windows.FontWeight类型的值转换器
	/// </summary>
	public class OpenTypeWeightToFontWeightConverter : IValueConverter
	{
		/// <summary>
		/// 由OpenTypeWeight转换为System.Windows.FontWeight类型
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ((int)value > System.Windows.FontWeights.UltraBold.ToOpenTypeWeight())
				return System.Windows.FontWeights.UltraBold;
			if ((int)value < System.Windows.FontWeights.Thin.ToOpenTypeWeight())
				return System.Windows.FontWeights.Thin;
			return System.Windows.FontWeight.FromOpenTypeWeight((int)value);
		}
		/// <summary>
		/// 由System.Windows.FontWeight类型转换为OpenTypeWeight
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return ((System.Windows.FontWeight)value).ToOpenTypeWeight();
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
	/// 根据原始背景色和透明度确定最终背景
	/// </summary>
	public class OriginalBackgroundColorAndBackgroundTransparencyToBackgroundConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Color color = (Color)values[0];
			double transparency = (double)values[1];
			color.A = (byte)(int)(color.A * (1 - transparency));
			SolidColorBrush brush = new SolidColorBrush(color);
			if (brush.CanFreeze) brush.Freeze();
			return brush;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}