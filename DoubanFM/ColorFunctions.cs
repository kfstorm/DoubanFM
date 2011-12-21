/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System;
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
        public readonly static double CoverColorDiff = 0.2;
        /// <summary>
        /// 亮度调整幅度
        /// </summary>
        public readonly static double ReviseParameter = 0.1;
        /// <summary>
        /// 用于调整封面右边取色区域的大小，具体表示右边区域宽度占图片宽度的百分比
        /// </summary>
        public readonly static double RightSideWidth = 0.1;
        /// <summary>
        /// 判断颜色是否太暗
        /// </summary>
        public readonly static double TooDark = 0.15;
        /// <summary>
        /// 判断颜色是否太暗亮
        /// </summary>
        public readonly static double TooBright = 0.4;
        /// <summary>
        /// 色相偏移
        /// </summary>
        public readonly static double HueOffset = 10;
        /// <summary>
        /// 进度条颜色调整参数
        /// </summary>
        public readonly static double ProgressBarReviseParameter = 0.2;
        /// <summary>
        /// 判断颜色饱和度是否太低
        /// </summary>
        public readonly static double NotSaturateEnough = 0.4;
        /// <summary>
        /// 判断颜色饱和度是否接近0
        /// </summary>
        public readonly static double AlmostZeroSaturation = 0.001;

        /// <summary>
        /// 从图片中获取背景颜色
        /// </summary>
        /// <param name="image">图片</param>
        /// <returns>背景色</returns>
        public static Color GetImageColorForBackground(BitmapImage image)
        {
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            System.Drawing.Bitmap bitmap = null;
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(ms);
                bitmap = new System.Drawing.Bitmap(ms);
            }
            int width = bitmap.Width;
            int height = bitmap.Height;
            int sum = width * height;
            int r = 0, g = 0, b = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var c = bitmap.GetPixel(i, j);
                    r += c.R;
                    g += c.G;
                    b += c.B;
                }
            }
            r = r / sum;
            g = g / sum;
            b = b / sum;
            Color color1 = Color.FromRgb((byte)r, (byte)g, (byte)b);

            r = 0; g = 0; b = 0;
            int istart = (int)(width * (1 - RightSideWidth));
            sum = (width - istart) * width;
            for (int i = istart; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var c = bitmap.GetPixel(i, j);
                    r += c.R;
                    g += c.G;
                    b += c.B;
                }
            }
            r = r / sum;
            g = g / sum;
            b = b / sum;
            Color color2 = Color.FromRgb((byte)r, (byte)g, (byte)b);
            HSLColor hsl1 = new HSLColor(color1);
            HSLColor hsl2 = new HSLColor(color2);
            hsl1.Hue += 10;
            if (IsNotSaturateEnough(hsl1) && !IsAlmostZeroSaturation(hsl1))
                hsl1.Saturation += 0.2;
            return Revise(hsl1, hsl2).ToRGB();
        }
        /// <summary>
        /// 两种颜色差异是否足够大
        /// </summary>
        /// <param name="c1">颜色1</param>
        /// <param name="c2">颜色2</param>
        /// <returns>Boolean值</returns>
        public static bool IsTooMuchDiff(HSLColor c1, HSLColor c2)
        {
            return Difference(c1, c2) > CoverColorDiff;
            //return Math.Abs((c1.R - c2.R) * (c1.R - c2.R) + (c1.G - c2.G) * (c1.G - c2.G) + (c1.B - c2.B) * (c1.B - c2.B)) > CoverColorDiff;
        }
        /// <summary>
        /// 计算两种颜色的差异。0为无差异，4.2为差异最大值
        /// </summary>
        /// <param name="c1">颜色1</param>
        /// <param name="c2">颜色2</param>
        /// <returns>差异值</returns>
        public static double Difference(HSLColor c1, HSLColor c2)
        {
            double AlphaDiff = Math.Abs(c1.Alpha - c2.Alpha);
            double HueDiff = Math.Min(Math.Abs(c1.Hue - c2.Hue), Math.Min(Math.Abs(c1.Hue + 360 - c2.Hue), Math.Abs(c1.Hue - c2.Hue - 360)));
            double SaturationDiff = Math.Abs(c1.Saturation - c2.Saturation);
            double LightnessDiff = Math.Abs(c1.Lightness - c2.Lightness);
            if (AlphaDiff + SaturationDiff + LightnessDiff > CoverColorDiff)
                return SaturationDiff + LightnessDiff;
            else
            {
                return HueDiff / 150 * Math.Min(c1.Saturation, c2.Saturation) * (0.5 - Math.Max(Math.Abs(c1.Lightness - 0.5), Math.Abs(c2.Lightness - 0.5))) * 2;
            }
            //return (Math.Abs(c1.Alpha - c2.Alpha) + HueDiff / 150 + Math.Abs(c1.Saturation - c2.Saturation) + Math.Abs(c1.Lightness - c2.Lightness));
        }
        /// <summary>
        /// 颜色饱和度是否太低
        /// </summary>
        /// <param name="hsvColor">颜色</param>
        /// <returns>Boolean值</returns>
        public static bool IsNotSaturateEnough(HSLColor color)
        {
            return color.Saturation < NotSaturateEnough;
        }
        /// <summary>
        /// 颜色饱和度是否接近0
        /// </summary>
        /// <param name="hsvColor">颜色</param>
        /// <returns>Boolean值</returns>
        public static bool IsAlmostZeroSaturation(HSLColor color)
        {
            return color.Saturation < AlmostZeroSaturation;
        }
        /// <summary>
        /// 颜色是否太暗
        /// </summary>
        /// <param name="hsvColor">颜色</param>
        /// <returns>Boolean值</returns>
        public static bool IsTooDark(HSLColor color)
        {
            return color.Lightness < TooDark;
        }
        /// <summary>
        /// 颜色是否太亮
        /// </summary>
        /// <param name="hsvColor">颜色</param>
        /// <returns>Boolean值</returns>
        public static bool IsTooBright(HSLColor color)
        {
            return color.Lightness > TooBright;
        }
        /// <summary>
        /// 反色
        /// </summary>
        /// <param name="hsvColor">原色</param>
        /// <returns>反色</returns>
        public static HSLColor Reverse(HSLColor color)
        {
            Color RGB = color.ToRGB();
            return new HSLColor(Color.FromArgb(RGB.A, (byte)(255 - RGB.R), (byte)(255 - RGB.G), (byte)(255 - RGB.B)));
            //return new HSLColor(hsvColor.Alpha, hsvColor.Hue + 180, 1 - hsvColor.Saturation, 1 - hsvColor.Lightness);
        }
        /// <summary>
        /// 颜色修正
        /// </summary>
        /// <param name="color1">待修正色</param>
        /// <param name="color2">参照色</param>
        /// <returns>修正色</returns>
        public static HSLColor Revise(HSLColor color1, HSLColor color2)
        {
            HSLColor newcolor = new HSLColor(color1.ToRGB());
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
        /// <param name="hsvColor">待修正色</param>
        /// <returns>修正色</returns>
        public static HSLColor Revise(HSLColor color)
        {
            if (IsTooDark(color))
                return ReviseBrighter(color);
            else
            {
                HSLColor newcolor = ReviseDarker(color);
                if (IsTooDark(newcolor))
                    return ReviseVeryDark(color);
                else
                    return newcolor;
            }
        }
        /// <summary>
        /// 将颜色调整到能够接受的最高亮度
        /// </summary>
        /// <param name="hsvColor">待修正色</param>
        /// <returns>修正色</returns>
        public static HSLColor ReviseVeryBright(HSLColor color)
        {
            return ReviseBrighter(color, TooBright - color.Lightness);
        }
        /// <summary>
        /// 将颜色调整到能够接受的最低亮度
        /// </summary>
        /// <param name="hsvColor">待修正色</param>
        /// <returns>修正色</returns>
        public static HSLColor ReviseVeryDark(HSLColor color)
        {
            return ReviseDarker(color, color.Lightness - TooDark);
        }
        /// <summary>
        /// 将颜色调亮特定亮度
        /// </summary>
        /// <param name="hsvColor">待修正色</param>
        /// <param name="brigher">调整的亮度</param>
        /// <returns>修正色</returns>
        public static HSLColor ReviseBrighter(HSLColor color, double brigher)
        {
            return new HSLColor(color.Alpha, color.Hue, color.Saturation, color.Lightness + brigher);
            //return Color.FromRgb(ReviseByteBigger(hsvColor.R), ReviseByteBigger(hsvColor.G), ReviseByteBigger(hsvColor.B));
        }
        /// <summary>
        /// 将颜色调亮一些
        /// </summary>
        /// <param name="hsvColor">待修正色</param>
        /// <returns>修正色</returns>
        public static HSLColor ReviseBrighter(HSLColor color)
        {
            return ReviseBrighter(color, ReviseParameter);
        }
        /// <summary>
        /// 将颜色调暗特定亮度
        /// </summary>
        /// <param name="hsvColor">待修正色</param>
        /// <param name="darker">调整的亮度</param>
        /// <returns>修正色</returns>
        public static HSLColor ReviseDarker(HSLColor color, double darker)
        {
            return new HSLColor(color.Alpha, color.Hue, color.Saturation, color.Lightness - darker);
        }
        /// <summary>
        /// 将颜色调暗一些
        /// </summary>
        /// <param name="hsvColor">待修正色</param>
        /// <returns>修正色</returns>
        public static HSLColor ReviseDarker(HSLColor color)
        {
            return ReviseDarker(color, ReviseParameter);
        }
    }
    /// <summary>
    /// HSL色彩空间的颜色类
    /// </summary>
    public class HSLColor
    {
        private double a, h, s, l;


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
        /// 亮度。范围：0~1。0为黑色，1为彩色
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
        public HSLColor()
            : this(1, 0, 0, 0)
        {
        }
        /// <summary>
        /// HSL构造函数
        /// </summary>
        /// <param name="Hue">色相</param>
        /// <param name="Saturation">饱和度</param>
        /// <param name="Lightness">亮度</param>
        public HSLColor(double Hue, double Saturation, double Lightness)
            : this(1, Hue, Saturation, Lightness)
        {
        }
        /// <summary>
        /// AHSL构造函数
        /// </summary>
        /// <param name="Alpha">Alpha通道</param>
        /// <param name="Hue">色相</param>
        /// <param name="Saturation">饱和度</param>
        /// <param name="Lightness">亮度</param>
        public HSLColor(double Alpha, double Hue, double Saturation, double Lightness)
        {
            this.Alpha = Alpha;
            this.Hue = Hue;
            this.Saturation = Saturation;
            this.Lightness = Lightness;
        }
        /// <summary>
        /// 由RGB颜色类Color构造一个HSLColor的实例
        /// </summary>
        /// <param name="hsvColor">RGB颜色</param>
        public HSLColor(Color color)
        {
            this.FromRGB(color);
        }
        /// <summary>
        /// RGB色彩空间转换
        /// </summary>
        /// <param name="hsvColor">RGB颜色</param>
        public void FromRGB(Color color)
        {
            a = (double)color.A / 255;
            double r = (double)color.R / 255;
            double g = (double)color.G / 255;
            double b = (double)color.B / 255;

            double min = Math.Min(r, Math.Min(g, b));
            double max = Math.Max(r, Math.Max(g, b));
            double distance = max - min;

            l = (max + min) / 2;
            double hT = 0;
            s = 0;
            if (distance > 0)
            {
                s = l < 0.5 ? distance / (max + min) : distance / ((2 - max) - min);
                double tempR = (((max - r) / 6) + (distance / 2)) / distance;
                double tempG = (((max - g) / 6) + (distance / 2)) / distance;
                double tempB = (((max - b) / 6) + (distance / 2)) / distance;
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
        private static Color ToRGB(HSLColor hsl)
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
                r = (byte)Math.Round(255 * HueToRGB(v1, v2, vH + 1.0 / 3));
                g = (byte)Math.Round(255 * HueToRGB(v1, v2, vH));
                b = (byte)Math.Round(255 * HueToRGB(v1, v2, vH - 1.0 / 3));
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
        private static double HueToRGB(double v1, double v2, double vH)
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
        public Color ToRGB()
        {
            return HSLColor.ToRGB(this);
        }
        /// <summary>
        /// ToString()方法，已重写
        /// </summary>
        /// <returns>ARGB颜色值</returns>
        public override string ToString()
        {
            return ToRGB().ToString();
        }
    }
}