using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DoubanFM
{
	/// <summary>
	/// 用HSV色彩空间表示的颜色
	/// </summary>
	public struct HsvColor
	{
		/// <summary>
		/// 不透明度
		/// </summary>
		public double A;
		/// <summary>
		/// 色相
		/// </summary>
		public double H;
		/// <summary>
		/// 饱和度
		/// </summary>
		public double S;
		/// <summary>
		/// 亮度
		/// </summary>
		public double V;

		/// <summary>
		/// 生成 <see cref="HsvColor"/> struct 的新实例。
		/// </summary>
		/// <param name="a">不透明度</param>
		/// <param name="h">色相</param>
		/// <param name="s">饱和度</param>
		/// <param name="v">亮度</param>
		public HsvColor(double a, double h, double s, double v)
		{
			A = a;
			H = h;
			S = s;
			V = v;
		}

		/// <summary>
		/// 从Color类的实例生成<see cref="HsvColor"/>的新实例
		/// </summary>
		/// <param name="argb">Color类的实例</param>
		/// <returns>
		///   <see cref="HsvColor"/>的新实例
		/// </returns>
		public static HsvColor FromArgb(Color argb)
		{
			double r = argb.R;
			double g = argb.G;
			double b = argb.B;

			double delta, min;
			double h = 0, s, v;

			min = Math.Min(Math.Min(r, g), b);
			v = Math.Max(Math.Max(r, g), b);
			delta = v - min;

			if (v == 0.0)
			{
				s = 0;
			}
			else
				s = delta / v;

			if (s == 0)
				h = 0.0;

			else
			{
				if (r == v)
					h = (g - b) / delta;
				else if (g == v)
					h = 2 + (b - r) / delta;
				else if (b == v)
					h = 4 + (r - g) / delta;

				h *= 60;
				if (h < 0.0)
					h = h + 360;

			}

			return new HsvColor { A = (double)argb.A / 255, H = h, S = s, V = v / 255 };
		}

		/// <summary>
		/// 转换为Color类的新实例
		/// </summary>
		/// <returns>
		/// Color类的新实例
		/// </returns>
		public Color ToArgb()
		{
			double h = H;
			double s = S;
			double v = V;

			double r = 0, g = 0, b = 0;

			if (s == 0)
			{
				r = v;
				g = v;
				b = v;
			}
			else
			{
				int i;
				double f, p, q, t;

				if (h == 360)
					h = 0;
				else
					h = h / 60;

				i = (int)Math.Truncate(h);
				f = h - i;

				p = v * (1.0 - s);
				q = v * (1.0 - (s * f));
				t = v * (1.0 - (s * (1.0 - f)));

				switch (i)
				{
					case 0:
						{
							r = v;
							g = t;
							b = p;
							break;
						}
					case 1:
						{
							r = q;
							g = v;
							b = p;
							break;
						}
					case 2:
						{
							r = p;
							g = v;
							b = t;
							break;
						}
					case 3:
						{
							r = p;
							g = q;
							b = v;
							break;
						}
					case 4:
						{
							r = t;
							g = p;
							b = v;
							break;
						}
					default:
						{
							r = v;
							g = p;
							b = q;
							break;
						}
				}

			}

			return Color.FromArgb((byte)(A * 255), (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
		}
	}
}
