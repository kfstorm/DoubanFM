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
        /// <param name="values"><see cref="T:System.Windows.Data.MultiBinding"/> 中源绑定生成的值的数组。值 <see cref="F:System.Windows.DependencyProperty.UnsetValue"/> 表示源绑定没有要提供以进行转换的值。</param>
        /// <param name="targetType">绑定目标属性的类型。</param>
        /// <param name="parameter">要使用的转换器参数。</param>
        /// <param name="culture">要用在转换器中的区域性。</param>
        /// <returns>
        /// 转换后的值。如果该方法返回 null，则使用有效的 null 值。<see cref="T:System.Windows.DependencyProperty"/>.<see cref="F:System.Windows.DependencyProperty.UnsetValue"/> 的返回值表示转换器没有生成任何值，且绑定将使用 <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/>（如果可用），否则将使用默认值。<see cref="T:System.Windows.Data.Binding"/>.<see cref="F:System.Windows.Data.Binding.DoNothing"/> 的返回值表示绑定不传输值，或不使用 <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> 或默认值。
        /// </returns>
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
        /// <summary>
        /// 不支持
        /// </summary>
        /// <param name="value">绑定目标生成的值。</param>
        /// <param name="targetTypes">要转换到的类型数组。数组长度指示为要返回的方法所建议的值的数量与类型。</param>
        /// <param name="parameter">要使用的转换器参数。</param>
        /// <param name="culture">要用在转换器中的区域性。</param>
        /// <returns>
        /// 从目标值转换回源值的值的数组。
        /// </returns>
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
        /// <param name="value">绑定源生成的值。</param>
        /// <param name="targetType">绑定目标属性的类型。</param>
        /// <param name="parameter">要使用的转换器参数。</param>
        /// <param name="culture">要用在转换器中的区域性。</param>
        /// <returns>
        /// 转换后的值。如果该方法返回 null，则使用有效的 null 值。
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan timespan = (TimeSpan)value;
            return (timespan.Hours > 0 ? timespan.Hours.ToString() + ":" : "") + (timespan.Minutes < 10 ? "0" : "") + timespan.Minutes + ":" + (timespan.Seconds < 10 ? "0" : "") + timespan.Seconds;
        }
        /// <summary>
        /// 由时间字符串转换回TimeSpan值
        /// </summary>
        /// <param name="value">绑定目标生成的值。</param>
        /// <param name="targetType">要转换到的类型。</param>
        /// <param name="parameter">要使用的转换器参数。</param>
        /// <param name="culture">要用在转换器中的区域性。</param>
        /// <returns>
        /// 转换后的值。如果该方法返回 null，则使用有效的 null 值。
        /// </returns>
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
        /// <param name="value">绑定源生成的值。</param>
        /// <param name="targetType">绑定目标属性的类型。</param>
        /// <param name="parameter">要使用的转换器参数。</param>
        /// <param name="culture">要用在转换器中的区域性。</param>
        /// <returns>
        /// 转换后的值。如果该方法返回 null，则使用有效的 null 值。
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is SolidColorBrush)
            {
                return new SolidColorBrush(ColorFunctions.ReviseBrighter(new HSLColor(((SolidColorBrush)value).Color), ColorFunctions.ProgressBarReviseParameter).ToRGB());
                //return new SolidColorBrush(ColorFunctions.ReviseBrighter(ColorFunctions.ReviseBrighter(new HSLColor(((SolidColorBrush)value).Color))).ToRGB());
            }
            return value;
        }
        /// <summary>
        /// 不支持
        /// </summary>
        /// <param name="value">绑定目标生成的值。</param>
        /// <param name="targetType">要转换到的类型。</param>
        /// <param name="parameter">要使用的转换器参数。</param>
        /// <param name="culture">要用在转换器中的区域性。</param>
        /// <returns>
        /// 转换后的值。如果该方法返回 null，则使用有效的 null 值。
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}