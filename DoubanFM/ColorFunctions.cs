/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System.Configuration;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using System.Threading;
namespace DoubanFM
{
    /// <summary>
    /// 有关颜色的方法类
    /// 主要用于计算窗口背景
    /// </summary>
    public class ColorFunctions
    {
        /// <summary>
        /// 两种颜色不同的阀值
        /// </summary>
        public readonly static double CoverColorDiff = double.Parse(ConfigurationManager.AppSettings["ColorFunctions.CoverColorDiff"]);
        /// <summary>
        /// 亮度调整幅度
        /// </summary>
        public readonly static double ReviseParameter = double.Parse(ConfigurationManager.AppSettings["ColorFunctions.ReviseParameter"]);
        /// <summary>
        /// 用于调整封面右边取色区域的大小，具体表示右边区域宽度占图片宽度的百分比
        /// </summary>
        public readonly static double RightSideWidth = double.Parse(ConfigurationManager.AppSettings["ColorFunctions.RightSideWidth"]);
        /// <summary>
        /// 判断颜色是否太暗
        /// </summary>
        public readonly static double TooDark = double.Parse(ConfigurationManager.AppSettings["ColorFunctions.TooDark"]);
        /// <summary>
        /// 判断颜色是否太暗亮
        /// </summary>
        public readonly static double TooBright = double.Parse(ConfigurationManager.AppSettings["ColorFunctions.TooBright"]);
        /// <summary>
        /// 色相偏移
        /// </summary>
        public readonly static double HueOffset = double.Parse(ConfigurationManager.AppSettings["ColorFunctions.HueOffset"]);
        /// <summary>
        /// 进度条颜色调整参数
        /// </summary>
        public readonly static double ProgressBarReviseParameter = double.Parse(ConfigurationManager.AppSettings["ColorFunctions.ProgressBarReviseParameter"]);
        /// <summary>
        /// 判断颜色饱和度是否太低
        /// </summary>
        public readonly static double NotSaturateEnough = double.Parse(ConfigurationManager.AppSettings["ColorFunctions.NotSaturateEnough"]);
        /// <summary>
        /// 判断颜色饱和度是否接近0
        /// </summary>
        public readonly static double AlmostZeroSaturation = double.Parse(ConfigurationManager.AppSettings["ColorFunctions.AlmostZeroSaturation"]);
        /// <summary>
        /// 计算图片平均色时是否使用颜色的饱和度作权重
        /// </summary>
        public readonly static bool EnableColorWeight = bool.Parse(ConfigurationManager.AppSettings["ColorFunctions.EnableColorWeight"]);

        /// <summary>
        /// 人脸的颜色
        /// </summary>
        public static readonly Color FaceColor =
            (Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["ColorFunctions.FaceColor"]);

        /// <summary>
        /// 人脸色0权重的颜色差异阈值
        /// </summary>
        public static readonly double ZeroWeightFaceColorDifference =
            double.Parse(ConfigurationManager.AppSettings["ColorFunctions.ZeroWeightFaceColorDifference"]);

        /// <summary>
        /// 最低权重
        /// </summary>
        public static readonly double MinWeight =
            double.Parse(ConfigurationManager.AppSettings["ColorFunctions.MinWeight"]);

        public delegate void ComputeCompleteCallback(Color color);

        /// <summary>
        /// 异步从图片中获取背景颜色
        /// </summary>
        /// <param name="image">图片</param>
        /// <param name="callback">计算完成后调用的委托</param>
        public static void GetImageColorForBackgroundAsync(BitmapSource image, ComputeCompleteCallback callback)
        {
            FormatConvertedBitmap bitmap = null;
            bool isFrozen = image.IsFrozen;
            if (!isFrozen)
            {
                //由于image没有冻结，所以image不能跨线程使用，这时要在当前线程中将image转换为另一个位图
                bitmap = new FormatConvertedBitmap(image, PixelFormats.Rgb24, BitmapPalettes.WebPalette, 0);
            }
            ThreadPool.QueueUserWorkItem(state =>
            {
                if (isFrozen)
                {
                    //由于image已经冻结，所以image可以跨线程使用，这时在新线程中将image转换为另一个位图
                    bitmap = new FormatConvertedBitmap(image, PixelFormats.Rgb24, BitmapPalettes.WebPalette, 0);
                }
                callback(GetImageColorForBackground(bitmap));
            });
        }

        /// <summary>
        /// 从图片中获取背景颜色
        /// </summary>
        /// <param name="bitmap">图片</param>
        public static Color GetImageColorForBackground(FormatConvertedBitmap bitmap)
        {
            const int bytesPerPixel = 3;

            if (bitmap.CanFreeze) bitmap.Freeze();
            var pixels = new byte[bitmap.PixelHeight * bitmap.PixelWidth * bytesPerPixel];
            bitmap.CopyPixels(pixels, bitmap.PixelWidth * bytesPerPixel, 0);
            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;
            
            //计算颜色的均值
            Color color = GetColorOfRegion(pixels, width, height, 0, width, 0, height);

            var hsl = new HslColor(color);
            if (IsNotSaturateEnough(hsl) && !IsAlmostZeroSaturation(hsl))
                hsl.Saturation += 0.2;

            return Revise(hsl).ToRgb();
        }

        /// <summary>
        /// 从图片中获取指定区域的背景颜色
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="top">The top.</param>
        /// <param name="bottom">The bottom.</param>
        /// <param name="forceDisableColorWeight">if set to <c>true</c> [force disable color weight].</param>
        /// <returns></returns>
        public static Color GetColorOfRegion(byte[] pixels, int width, int height, int left, int right, int top,
            int bottom, bool forceDisableColorWeight = false, bool removeFaceColor = true)
        {
            const int bytesPerPixel = 3;
            double sr = 0, sg = 0, sb = 0;
            double totalweight = 0;
            for (int i = top; i < bottom; i++)
            {
                for (int j = left; j < right; j++)
                {
                    byte r = pixels[(i * width + j) * bytesPerPixel + 0];
                    byte g = pixels[(i * width + j) * bytesPerPixel + 1];
                    byte b = pixels[(i * width + j) * bytesPerPixel + 2];
                    double weight;
                    if (!forceDisableColorWeight && EnableColorWeight)
                    {
                        var color = Color.FromRgb(r, g, b);
                        var hslColor = new HslColor(color);
                        weight = (1 - Math.Abs(1 - 2*hslColor.Lightness))*hslColor.Saturation;
                        if (weight < MinWeight)
                        {
                            weight = 0;
                        }
                        if (removeFaceColor)
                        {
                            var difference = Math.Abs(new HslColor(FaceColor).Hue - hslColor.Hue)/360;
                            if (difference <= ZeroWeightFaceColorDifference)
                            {
                                weight = 0;
                            }
                            else
                            {
                                weight = weight*difference;
                            }
                        }
                    }
                    else
                    {
                        weight = 1;
                    }
                    totalweight += weight;
                    sr += r * weight;
                    sg += g * weight;
                    sb += b * weight;
                }
            }

            if (totalweight <= 0)
            {
                if (removeFaceColor)
                {
                    //当去除人脸色彩后总权重为0时，禁用去除人脸色彩
                    return GetColorOfRegion(pixels, width, height, left, right, top, bottom, false, false);
                }
                else
                {
                    //纯灰度图片不能使用权重
                    var newColor = GetColorOfRegion(pixels, width, height, left, right, top, bottom, true);
                    var newHslColor = new HslColor(newColor);
                    newHslColor.Saturation = 0;
                    return newHslColor.ToRgb();
                }
            }
            else
            {
                sr = sr/totalweight;
                sg = sg/totalweight;
                sb = sb/totalweight;
                return Color.FromRgb((byte) sr, (byte) sg, (byte) sb);
            }
        }
        /// <summary>
        /// 两种颜色差异是否足够大
        /// </summary>
        /// <param name="c1">颜色1</param>
        /// <param name="c2">颜色2</param>
        /// <returns>Boolean值</returns>
        public static bool IsTooMuchDiff(HslColor c1, HslColor c2)
        {
            return Difference(c1, c2) > CoverColorDiff;
            //return Math.Abs((c1.R - c2.R) * (c1.R - c2.R) + (c1.G - c2.G) * (c1.G - c2.G) + (c1.B - c2.B) * (c1.B - c2.B)) > CoverColorDiff;
        }
        /// <summary>
        /// 计算两种颜色的差异。0为无差异，1为差异最大值
        /// </summary>
        /// <param name="c1">颜色1</param>
        /// <param name="c2">颜色2</param>
        /// <returns>差异值</returns>
        public static double Difference(HslColor c1, HslColor c2)
        {
            return Difference(c1.ToRgb(), c2.ToRgb());
        }

        /// <summary>
        /// 计算两种颜色的差异。0为无差异，1为差异最大值
        /// </summary>
        /// <param name="c1">颜色1</param>
        /// <param name="c2">颜色2</param>
        /// <param name="compareAlpha">是否比较Alpha通道</param>
        /// <returns>
        /// 差异值
        /// </returns>
        public static double Difference(Color c1, Color c2, bool compareAlpha = true)
        {
            if (compareAlpha)
            {
                return
                    Math.Sqrt((Math.Pow(c1.A - c2.A, 2) + Math.Pow(c1.R - c2.R, 2) + Math.Pow(c1.G - c2.G, 2) +
                               Math.Pow(c1.B - c2.B, 2))/4/255/255);
            }
            return Math.Sqrt((Math.Pow(c1.R - c2.R, 2) + Math.Pow(c1.G - c2.G, 2) + Math.Pow(c1.B - c2.B, 2))/3/255/255);
        }

        /// <summary>
        /// 颜色饱和度是否太低
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>Boolean值</returns>
        public static bool IsNotSaturateEnough(HslColor color)
        {
            return color.Saturation < NotSaturateEnough;
        }
        /// <summary>
        /// 颜色饱和度是否接近0
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>Boolean值</returns>
        public static bool IsAlmostZeroSaturation(HslColor color)
        {
            return color.Saturation < AlmostZeroSaturation;
        }
        /// <summary>
        /// 颜色是否太暗
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>Boolean值</returns>
        public static bool IsTooDark(HslColor color)
        {
            return color.Lightness < TooDark;
        }
        /// <summary>
        /// 颜色是否太亮
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>Boolean值</returns>
        public static bool IsTooBright(HslColor color)
        {
            return color.Lightness > TooBright;
        }
        /// <summary>
        /// 反色
        /// </summary>
        /// <param name="color">原色</param>
        /// <returns>反色</returns>
        public static HslColor Reverse(HslColor color)
        {
            Color rgb = color.ToRgb();
            return new HslColor(Color.FromArgb(rgb.A, (byte)(255 - rgb.R), (byte)(255 - rgb.G), (byte)(255 - rgb.B)));
            //return new HSLColor(hsvColor.Alpha, hsvColor.Hue + 180, 1 - hsvColor.Saturation, 1 - hsvColor.Lightness);
        }
        /// <summary>
        /// 颜色修正
        /// </summary>
        /// <param name="color1">待修正色</param>
        /// <param name="color2">参照色</param>
        /// <returns>修正色</returns>
        public static HslColor Revise(HslColor color1, HslColor color2)
        {
            var newcolor = new HslColor(color1.ToRgb());
            while (IsTooBright(newcolor) || !IsTooMuchDiff(newcolor, color2) && !IsTooDark(newcolor) && newcolor.Lightness > 0)
                newcolor = ReviseDarker(newcolor);
            if (!IsTooDark(newcolor)) return newcolor;
            newcolor = ReviseBrighter(color1);
            while (IsTooDark(newcolor) || !IsTooMuchDiff(newcolor, color2) && !IsTooBright(newcolor) && newcolor.Lightness < 1)
                newcolor = ReviseBrighter(newcolor);
            if (!IsTooBright(newcolor)) return newcolor;
            if (IsTooBright(color1))
                return ReviseVeryBright(color1);
            if (IsTooDark(color1))
                return ReviseVeryDark(color1);
            return color1;

        }
        /// <summary>
        /// 无参照色时的颜色修正
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <returns>修正色</returns>
        public static HslColor Revise(HslColor color)
        {
            if (IsTooDark(color))
                return ReviseBrighter(color);
            if (IsTooBright(color))
                return ReviseVeryBright(color);
            //color = ReviseDarker(color);
            //if (IsTooDark(color))
                //return ReviseVeryDark(color);
            return color;
        }
        /// <summary>
        /// 将颜色调整到能够接受的最高亮度
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <returns>修正色</returns>
        public static HslColor ReviseVeryBright(HslColor color)
        {
            return ReviseBrighter(color, TooBright - color.Lightness);
        }
        /// <summary>
        /// 将颜色调整到能够接受的最低亮度
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <returns>修正色</returns>
        public static HslColor ReviseVeryDark(HslColor color)
        {
            return ReviseDarker(color, color.Lightness - TooDark);
        }
        /// <summary>
        /// 将颜色调亮特定亮度
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <param name="brigher">调整的亮度</param>
        /// <returns>修正色</returns>
        public static HslColor ReviseBrighter(HslColor color, double brigher)
        {
            return new HslColor(color.Alpha, color.Hue, color.Saturation, color.Lightness + brigher);
            //return Color.FromRgb(ReviseByteBigger(hsvColor.R), ReviseByteBigger(hsvColor.G), ReviseByteBigger(hsvColor.B));
        }
        /// <summary>
        /// 将颜色调亮一些
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <returns>修正色</returns>
        public static HslColor ReviseBrighter(HslColor color)
        {
            return ReviseBrighter(color, ReviseParameter);
        }
        /// <summary>
        /// 将颜色调暗特定亮度
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <param name="darker">调整的亮度</param>
        /// <returns>修正色</returns>
        public static HslColor ReviseDarker(HslColor color, double darker)
        {
            return new HslColor(color.Alpha, color.Hue, color.Saturation, color.Lightness - darker);
        }
        /// <summary>
        /// 将颜色调暗一些
        /// </summary>
        /// <param name="color">待修正色</param>
        /// <returns>修正色</returns>
        public static HslColor ReviseDarker(HslColor color)
        {
            return ReviseDarker(color, ReviseParameter);
        }
    }
    /// <summary>
    /// HSL色彩空间的颜色类
    /// </summary>
    public class HslColor
    {
        public double a, h, s, l;


        /// <summary>
        /// 不透明度。范围：0~1，1为不透明，0为透明
        /// </summary>
        public double Alpha
        {
            get
            {
                return a;
            }
            set
            {
                if (value > 1)
                    a = 1;
                else if (value < 0)
                    a = 0;
                else a = value;
            }
        }

        /// <summary>
        /// 色相。范围：0~359.9999999。特殊颜色：红：0    黄：60    绿：120   青：180   蓝：240   洋红：300
        /// </summary>
        public double Hue
        {
            get
            {
                return h;
            }
            set
            {
                if (value >= 360)
                    h = value % 360;
                else if (value < 0)
                    h = value - Math.Floor(value / 360) * 360;
                else h = value;
            }
        }
        /// <summary>
        /// 饱和度。范围：0~1。亮度0.5时，0为灰色，1为彩色
        /// </summary>
        public double Saturation
        {
            get
            {
                return s;
            }
            set
            {
                if (value > 1)
                    s = 1;
                else if (value < 0)
                    s = 0;
                else s = value;
            }
        }

        /// <summary>
        /// 亮度。范围：0~1。0为黑色，0.5为彩色，1为白色
        /// </summary>
        public double Lightness
        {
            get
            {
                return l;
            }
            set
            {
                if (value > 1)
                    l = 1;
                else if (value < 0)
                    l = 0;
                else l = value;
            }
        }

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public HslColor()
            : this(1, 0, 0, 0)
        {
        }
        /// <summary>
        /// HSL构造函数
        /// </summary>
        /// <param name="hue">色相</param>
        /// <param name="saturation">饱和度</param>
        /// <param name="lightness">亮度</param>
        public HslColor(double hue, double saturation, double lightness)
            : this(1, hue, saturation, lightness)
        {
        }
        /// <summary>
        /// AHSL构造函数
        /// </summary>
        /// <param name="alpha">Alpha通道</param>
        /// <param name="hue">色相</param>
        /// <param name="saturation">饱和度</param>
        /// <param name="lightness">亮度</param>
        public HslColor(double alpha, double hue, double saturation, double lightness)
        {
            Alpha = alpha;
            Hue = hue;
            Saturation = saturation;
            Lightness = lightness;
        }
        /// <summary>
        /// 由RGB颜色类Color构造一个HSLColor的实例
        /// </summary>
        /// <param name="color">RGB颜色</param>
        public HslColor(Color color)
        {
            FromRgb(color);
        }
        /// <summary>
        /// RGB色彩空间转换
        /// </summary>
        /// <param name="color">RGB颜色</param>
        public void FromRgb(Color color)
        {
            a = (double)color.A / 255;
            double r = (double)color.R / 255;
            double g = (double)color.G / 255;
            double b = (double)color.B / 255;

            double min = Math.Min(r, Math.Min(g, b));
            double max = Math.Max(r, Math.Max(g, b));
            double distance = max - min;

            l = (max + min) / 2;
            s = 0;
            if (distance > 0)
            {
                s = l < 0.5 ? distance / (max + min) : distance / ((2 - max) - min);
                double tempR = (((max - r) / 6) + (distance / 2)) / distance;
                double tempG = (((max - g) / 6) + (distance / 2)) / distance;
                double tempB = (((max - b) / 6) + (distance / 2)) / distance;
                double hT;
                if (r == max)
                    hT = tempB - tempG;
                else if (g == max)
                    hT = (1.0 / 3 + tempR) - tempB;
                else
                    hT = (2.0 / 3 + tempG) - tempR;
                if (hT < 0)
                    hT += 1;
                if (hT > 1)
                    hT -= 1;
                h = hT * 360;
            }
        }
        /// <summary>
        /// 转换到RGB色彩空间
        /// </summary>
        /// <param name="hsl">HSL颜色</param>
        /// <returns>转换后的RGB颜色</returns>
        public static Color ToRgb(HslColor hsl)
        {
            byte a = (byte)Math.Round(hsl.Alpha * 255), r, g, b;
            if (hsl.Saturation == 0)
            {
                r = (byte)Math.Round(hsl.Lightness * 255);
                g = r;
                b = r;
            }
            else
            {
                double vH = hsl.Hue / 360;
                double v2 = hsl.Lightness < 0.5 ? hsl.Lightness * (1 + hsl.Saturation) : (hsl.Lightness + hsl.Saturation) - (hsl.Lightness * hsl.Saturation);
                double v1 = 2 * hsl.Lightness - v2;
                r = (byte)Math.Round(255 * HueToRgb(v1, v2, vH + 1.0 / 3));
                g = (byte)Math.Round(255 * HueToRgb(v1, v2, vH));
                b = (byte)Math.Round(255 * HueToRgb(v1, v2, vH - 1.0 / 3));
            }
            return Color.FromArgb(a, r, g, b);
        }
        /// <summary>
        /// 这个……我也不懂
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="vH"></param>
        /// <returns></returns>
        public static double HueToRgb(double v1, double v2, double vH)
        {
            if (vH < 0) vH += 1;
            if (vH > 1) vH -= 1;
            if (6 * vH < 1) return v1 + ((v2 - v1) * 6 * vH);
            if (2 * vH < 1) return v2;
            if (3 * vH < 2) return v1 + (v2 - v1) * (2.0 / 3 - vH) * 6;
            return v1;
        }
        /// <summary>
        /// 转换到RGB色彩空间
        /// </summary>
        /// <returns>RGB颜色</returns>
        public Color ToRgb()
        {
            return ToRgb(this);
        }
        /// <summary>
        /// ToString()方法，已重写
        /// </summary>
        /// <returns>ARGB颜色值</returns>
        public override string ToString()
        {
            return ToRgb().ToString();
        }
    }
}