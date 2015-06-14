/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DoubanFM
{
	/// <summary>
	/// 颜色选择器控件
	/// </summary>
	[TemplatePart(Name = "PART_ColorShadeCanvas", Type = typeof(Canvas))]
	[TemplatePart(Name = "PART_ColorShadeSelector", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_ColorSpectrumSlider", Type = typeof(ColorSpectrumSlider))]
	[TemplatePart(Name = "PART_TextBoxA", Type = typeof(TextBox))]
	[TemplatePart(Name = "PART_TextBoxR", Type = typeof(TextBox))]
	[TemplatePart(Name = "PART_TextBoxG", Type = typeof(TextBox))]
	[TemplatePart(Name = "PART_TextBoxB", Type = typeof(TextBox))]
	[TemplatePart(Name = "PART_TextBoxHexString", Type = typeof(TextBox))]
	public class ColorPicker : Control
	{
		static ColorPicker()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
		}

		public ColorPicker()
		{
			//在控件加载后更新一次颜色
			this.Loaded += (sender, e) =>
				{
					UpdateColor(Color, null);
				};
		}

		#region Color属性

		/// <summary>
		/// 选择的颜色
		/// </summary>
		public Color Color
		{
			get { return (Color)GetValue(ColorProperty); }
			set { SetValue(ColorProperty, value); }
		}

		public static readonly DependencyProperty ColorProperty =
			DependencyProperty.Register("Color", typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnColorChanged)));

		public static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ColorPicker picker = (ColorPicker)d;
			
			//更新控件的其他属性
			picker.A = picker.Color.A;
			picker.R = picker.Color.R;
			picker.G = picker.Color.G;
			picker.B = picker.Color.B;
			if (picker.IsAlphaEnabled)
			{
				picker.HexString = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", picker.A, picker.R, picker.G, picker.B);
			}
			else
			{
				picker.HexString = string.Format("{0:X2}{1:X2}{2:X2}", picker.R, picker.G, picker.B);
			}
			
			picker.RaiseEvent(new RoutedPropertyChangedEventArgs<Color>((Color)e.OldValue, (Color)e.NewValue, ColorChangedEvent));

			//更新所有TextBox的内容
			picker.ForceUpdateTextBox();
		}

		public event RoutedPropertyChangedEventHandler<Color> ColorChanged
		{
			add
			{
				AddHandler(ColorChangedEvent, value);
			}
			remove
			{
				RemoveHandler(ColorChangedEvent, value);
			}
		}

		public static readonly RoutedEvent ColorChangedEvent = EventManager.RegisterRoutedEvent("ColorChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<Color>), typeof(ColorPicker));

		#endregion

		#region A属性

		/// <summary>
		/// 颜色的Alpha通道
		/// </summary>
		public byte A
		{
			get { return (byte)GetValue(AProperty); }
			set { SetValue(AProperty, value); }
		}

		public static readonly DependencyProperty AProperty =
			DependencyProperty.Register("A", typeof(byte), typeof(ColorPicker), new FrameworkPropertyMetadata((byte)255, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnAChanged)));

		public static void OnAChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ColorPicker picker = (ColorPicker)d;
			picker.UpdateColor(Color.FromArgb((byte)e.NewValue, picker.Color.R, picker.Color.G, picker.Color.B), null);
			picker.RaiseEvent(new RoutedPropertyChangedEventArgs<byte>((byte)e.OldValue, (byte)e.NewValue, AChangedEvent));
		}

		public event RoutedPropertyChangedEventHandler<byte> AChanged
		{
			add
			{
				AddHandler(AChangedEvent, value);
			}
			remove
			{
				RemoveHandler(AChangedEvent, value);
			}
		}

		public static readonly RoutedEvent AChangedEvent = EventManager.RegisterRoutedEvent("AChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<byte>), typeof(ColorPicker));

		#endregion

		#region R属性

		/// <summary>
		/// 颜色的Red通道
		/// </summary>
		public byte R
		{
			get { return (byte)GetValue(RProperty); }
			set { SetValue(RProperty, value); }
		}

		public static readonly DependencyProperty RProperty =
			DependencyProperty.Register("R", typeof(byte), typeof(ColorPicker), new FrameworkPropertyMetadata((byte)255, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnRChanged)));

		public static void OnRChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ColorPicker picker = (ColorPicker)d;
			picker.UpdateColor(Color.FromArgb(picker.Color.A, (byte)e.NewValue, picker.Color.G, picker.Color.B), null);
			picker.RaiseEvent(new RoutedPropertyChangedEventArgs<byte>((byte)e.OldValue, (byte)e.NewValue, RChangedEvent));
		}

		public event RoutedPropertyChangedEventHandler<byte> RChanged
		{
			add
			{
				AddHandler(RChangedEvent, value);
			}
			remove
			{
				RemoveHandler(RChangedEvent, value);
			}
		}

		public static readonly RoutedEvent RChangedEvent = EventManager.RegisterRoutedEvent("RChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<byte>), typeof(ColorPicker));

		#endregion

		#region G属性

		/// <summary>
		/// 颜色的Green通道
		/// </summary>
		public byte G
		{
			get { return (byte)GetValue(GProperty); }
			set { SetValue(GProperty, value); }
		}

		public static readonly DependencyProperty GProperty =
			DependencyProperty.Register("G", typeof(byte), typeof(ColorPicker), new FrameworkPropertyMetadata((byte)255, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnGChanged)));

		public static void OnGChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ColorPicker picker = (ColorPicker)d;
			picker.UpdateColor(Color.FromArgb(picker.Color.A, picker.Color.R, (byte)e.NewValue, picker.Color.B), null);
			picker.RaiseEvent(new RoutedPropertyChangedEventArgs<byte>((byte)e.OldValue, (byte)e.NewValue, GChangedEvent));
		}

		public event RoutedPropertyChangedEventHandler<byte> GChanged
		{
			add
			{
				AddHandler(GChangedEvent, value);
			}
			remove
			{
				RemoveHandler(GChangedEvent, value);
			}
		}

		public static readonly RoutedEvent GChangedEvent = EventManager.RegisterRoutedEvent("GChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<byte>), typeof(ColorPicker));

		#endregion

		#region B属性

		/// <summary>
		/// 颜色的Blue通道
		/// </summary>
		public byte B
		{
			get { return (byte)GetValue(BProperty); }
			set { SetValue(BProperty, value); }
		}

		public static readonly DependencyProperty BProperty =
			DependencyProperty.Register("B", typeof(byte), typeof(ColorPicker), new FrameworkPropertyMetadata((byte)255, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnBChanged)));

		public static void OnBChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ColorPicker picker = (ColorPicker)d;
			picker.UpdateColor(Color.FromArgb(picker.Color.A, picker.Color.R, picker.Color.G, (byte)e.NewValue), null);
			picker.RaiseEvent(new RoutedPropertyChangedEventArgs<byte>((byte)e.OldValue, (byte)e.NewValue, BChangedEvent));
		}

		public event RoutedPropertyChangedEventHandler<byte> BChanged
		{
			add
			{
				AddHandler(BChangedEvent, value);
			}
			remove
			{
				RemoveHandler(BChangedEvent, value);
			}
		}

		public static readonly RoutedEvent BChangedEvent = EventManager.RegisterRoutedEvent("BChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<byte>), typeof(ColorPicker));

		#endregion

		#region HexString属性

		/// <summary>
		/// 颜色的十六进制字符串表示
		/// </summary>
		public string HexString
		{
			get { return (string)GetValue(HexStringProperty); }
			set { SetValue(HexStringProperty, value); }
		}

		public static readonly DependencyProperty HexStringProperty =
			DependencyProperty.Register("HexString", typeof(string), typeof(ColorPicker), new FrameworkPropertyMetadata("FFFFFFFF", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnHexStringChanged)),
			new ValidateValueCallback(o =>
				{
					string hex = o as string;
					if (hex == null) return false;
					if (hex.Length != 8 && hex.Length != 6) return false;
					return hex.All(ch =>
						{
							return (ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f');
						});
				}));

		public static void OnHexStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ColorPicker picker = (ColorPicker)d;

			if (((string)e.NewValue).Length == 8 && picker.IsAlphaEnabled)
			{
				picker.UpdateColor(Color.FromArgb(
					byte.Parse(((string)e.NewValue).Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
					byte.Parse(((string)e.NewValue).Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
					byte.Parse(((string)e.NewValue).Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
					byte.Parse(((string)e.NewValue).Substring(6, 2), System.Globalization.NumberStyles.HexNumber)),
					null);
				picker.RaiseEvent(new RoutedPropertyChangedEventArgs<string>((string)e.OldValue, (string)e.NewValue, HexStringChangedEvent));
			}
			else if (((string)e.NewValue).Length == 6 && !picker.IsAlphaEnabled)
			{
				picker.UpdateColor(Color.FromRgb(
					byte.Parse(((string)e.NewValue).Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
					byte.Parse(((string)e.NewValue).Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
					byte.Parse(((string)e.NewValue).Substring(4, 2), System.Globalization.NumberStyles.HexNumber)),
					null);
			}
		}

		public event RoutedPropertyChangedEventHandler<string> HexStringChanged
		{
			add
			{
				AddHandler(HexStringChangedEvent, value);
			}
			remove
			{
				RemoveHandler(HexStringChangedEvent, value);
			}
		}

		public static readonly RoutedEvent HexStringChangedEvent = EventManager.RegisterRoutedEvent("HexStringChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<string>), typeof(ColorPicker));

		#endregion

		#region IsAlphaEnabled属性

		/// <summary>
		/// 颜色中是否包含Alpha通道
		/// </summary>
		public bool IsAlphaEnabled
		{
			get { return (bool)GetValue(IsAlphaEnabledProperty); }
			set { SetValue(IsAlphaEnabledProperty, value); }
		}

		public static readonly DependencyProperty IsAlphaEnabledProperty =
			DependencyProperty.Register("IsAlphaEnabled", typeof(bool), typeof(ColorPicker), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsAlphaEnabledChanged)));

		public static void OnIsAlphaEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ColorPicker picker = (ColorPicker)d;
			if (!(bool)e.NewValue)
			{
				Color color = picker.Color;
				color.A = 255;
				picker.HexString = string.Format("{0:X2}{1:X2}{2:X2}", picker.R, picker.G, picker.B);
				picker.UpdateColor(color, null);
			}
			else
			{
				picker.HexString = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", picker.A, picker.R, picker.G, picker.B);
			}
			picker.RaiseEvent(new RoutedPropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue, IsAlphaEnabledChangedEvent));
		}

		public event RoutedPropertyChangedEventHandler<bool> IsAlphaEnabledChanged
		{
			add
			{
				AddHandler(IsAlphaEnabledChangedEvent, value);
			}
			remove
			{
				RemoveHandler(IsAlphaEnabledChangedEvent, value);
			}
		}

		public static readonly RoutedEvent IsAlphaEnabledChangedEvent = EventManager.RegisterRoutedEvent("IsAlphaEnabledChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(ColorPicker));

		#endregion

		#region 字段
		/// <summary>
		/// 最后一次设置颜色时的精确颜色
		/// </summary>
		private HsvColor? lastPreciseColor;
		/// <summary>
		/// 指示是否正在更新颜色
		/// </summary>
		bool updatingColor = false;

		/// <summary>
		/// 渐变画布
		/// </summary>
		private Canvas shadeCanvas;
		/// <summary>
		/// 渐变选择器
		/// </summary>
		private FrameworkElement shadeSelector;
		/// <summary>
		/// 频谱选择器
		/// </summary>
		private ColorSpectrumSlider spectrumSlider;
		/// <summary>
		/// 代表A值的TextBox
		/// </summary>
		private TextBox tbA;
		/// <summary>
		/// 代表R值的TextBox
		/// </summary>
		private TextBox tbR;
		/// <summary>
		/// 代表G值的TextBox
		/// </summary>
		private TextBox tbG;
		/// <summary>
		/// 代表B值的TextBox
		/// </summary>
		private TextBox tbB;
		/// <summary>
		/// 代表HexString值的TextBox
		/// </summary>
		private TextBox tbHexString;
		#endregion

		#region 控件逻辑部分

		/// <summary>
		/// 在派生类中重写后，每当应用程序代码或内部进程调用 <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>，都将调用此方法。
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			//初始化控件部件
			shadeCanvas = this.Template.FindName("PART_ColorShadeCanvas", this) as Canvas;
			shadeSelector = this.Template.FindName("PART_ColorShadeSelector", this) as FrameworkElement;
			spectrumSlider = this.Template.FindName("PART_ColorSpectrumSlider", this) as ColorSpectrumSlider;
			tbA = this.Template.FindName("PART_TextBoxA", this) as TextBox;
			tbR = this.Template.FindName("PART_TextBoxR", this) as TextBox;
			tbG = this.Template.FindName("PART_TextBoxG", this) as TextBox;
			tbB = this.Template.FindName("PART_TextBoxB", this) as TextBox;
			tbHexString = this.Template.FindName("PART_TextBoxHexString", this) as TextBox;
			
			//为控件部件添加事件处理程序
			shadeCanvas.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(shadeCanvas_PreviewMouseLeftButtonDown);
			shadeCanvas.MouseMove += new MouseEventHandler(shadeCanvas_MouseMove);
			shadeCanvas.MouseLeftButtonUp += new MouseButtonEventHandler(shadeCanvas_MouseLeftButtonUp);
			spectrumSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(spectrumSlider_ValueChanged);

			//更新颜色
			UpdateColor(Color, null);
		}

		/// <summary>
		/// 频谱选择器的选择改变时触发
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedPropertyChangedEventArgs&lt;System.Double&gt;"/> instance containing the event data.</param>
		void spectrumSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			//频谱选择已改变，需更新颜色
			HsvColor hsvColor = lastPreciseColor.HasValue ? lastPreciseColor.Value : HsvColor.FromArgb(Color);
			hsvColor.H = spectrumSlider.Value;
			UpdateColor(hsvColor.ToArgb(), hsvColor);
		}

		/// <summary>
		/// 渐变选择器中按下鼠标左键时触发
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
		void shadeCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point p = e.GetPosition(shadeCanvas);
			UpdateColorFromPoint(p);
			shadeCanvas.CaptureMouse();
			e.Handled = true;
		}

		/// <summary>
		/// 渐变选择器中松开鼠标左键时触发
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
		void shadeCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			shadeCanvas.ReleaseMouseCapture();
		}

		/// <summary>
		/// 渐变选择器中移动鼠标时触发
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
		void shadeCanvas_MouseMove(object sender, MouseEventArgs e)
		{
			if (shadeCanvas.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
			{
				Point p = e.GetPosition(shadeCanvas);
				UpdateColorFromPoint(p);
			}
		}

		/// <summary>
		/// 根据渐变选择器在渐变画布中的位置改变颜色
		/// </summary>
		/// <param name="p">渐变选择器的中心相对于渐变画布的坐标</param>
		protected void UpdateColorFromPoint(Point p)
		{
			if (p.X < 0) p.X = 0;
			if (p.X > shadeCanvas.ActualWidth) p.X = shadeCanvas.ActualWidth;
			if (p.Y < 0) p.Y = 0;
			if (p.Y > shadeCanvas.ActualHeight) p.Y = shadeCanvas.ActualHeight;

			//计算新的颜色
			HsvColor hsvColor = lastPreciseColor.HasValue ? lastPreciseColor.Value : HsvColor.FromArgb(spectrumSlider.SelectedColor);
			if (!lastPreciseColor.HasValue)
			{
				hsvColor.A = (double)Color.A / 255;
			}
			hsvColor.S = p.X / shadeCanvas.ActualWidth;
			hsvColor.V = 1 - p.Y / shadeCanvas.ActualHeight;
			//更新颜色
			UpdateColor(hsvColor.ToArgb(), hsvColor);
		}

		/// <summary>
		/// 更新颜色
		/// </summary>
		/// <param name="newColor">新的颜色</param>
		/// <param name="preciseColor">如果新的颜色是由一个精确的颜色得来的，则应传递精确颜色值，否则传递null</param>
		protected void UpdateColor(Color newColor, HsvColor? preciseColor)
		{
			if (updatingColor) return;
			updatingColor = true;

			HsvColor hsvColor = preciseColor.HasValue ? preciseColor.Value : HsvColor.FromArgb(newColor);
			lastPreciseColor = preciseColor;

			if (shadeCanvas != null)
			{
				if (preciseColor == null && hsvColor.S == 0 && hsvColor.V == 0)
				{
					Canvas.SetLeft(shadeSelector, shadeCanvas.ActualWidth - shadeSelector.ActualWidth / 2);
				}
				else
				{
					Canvas.SetLeft(shadeSelector, hsvColor.S * shadeCanvas.ActualWidth - shadeSelector.ActualWidth / 2);
				}
				Canvas.SetTop(shadeSelector, (1 - hsvColor.V) * shadeCanvas.ActualHeight - shadeSelector.ActualHeight / 2);
				if (hsvColor.S > 0)
				{
					spectrumSlider.Value = hsvColor.H;
				}
			}
			if (Color == newColor)
			{
				Color = newColor;
				ForceUpdateTextBox();
			}
			else
			{
				Color = newColor;
			}

			updatingColor = false;
		}

		/// <summary>
		/// 强制更新TextBox中的内容
		/// </summary>
		public void ForceUpdateTextBox()
		{
			if (tbA != null)
			{
				BindingExpression expression = tbA.GetBindingExpression(TextBox.TextProperty);
				if (expression != null)
					expression.UpdateTarget();
			}

			if (tbR != null)
			{
				BindingExpression expression = tbR.GetBindingExpression(TextBox.TextProperty);
				if (expression != null)
					expression.UpdateTarget();
			}

			if (tbG != null)
			{
				BindingExpression expression = tbG.GetBindingExpression(TextBox.TextProperty);
				if (expression != null)
					expression.UpdateTarget();
			}

			if (tbB != null)
			{
				BindingExpression expression = tbB.GetBindingExpression(TextBox.TextProperty);
				if (expression != null)
					expression.UpdateTarget();
			}

			if (tbHexString != null)
			{
				BindingExpression expression = tbHexString.GetBindingExpression(TextBox.TextProperty);
				if (expression != null)
					expression.UpdateTarget();
			}
		}

		#endregion
	}
}