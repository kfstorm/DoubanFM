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
			this.Loaded += (sender, e) =>
				{
					UpdateColor(Color, null);
				};
		}

		#region Color

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
			
			picker.A = picker.Color.A;
			picker.R = picker.Color.R;
			picker.G = picker.Color.G;
			picker.B = picker.Color.B;
			picker.HexString = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", picker.A, picker.R, picker.G, picker.B);
			
			picker.RaiseEvent(new RoutedPropertyChangedEventArgs<Color>((Color)e.OldValue, (Color)e.NewValue, ColorChangedEvent));

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

		#region A

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

		#region R

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

		#region G

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

		#region B

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

		#region HexString

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
					if (hex.Length != 8) return false;
					return hex.All(ch =>
						{
							return (ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f');
						});
				}));

		public static void OnHexStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ColorPicker picker = (ColorPicker)d;
			picker.UpdateColor(Color.FromArgb(
				byte.Parse(((string)e.NewValue).Substring(0,2), System.Globalization.NumberStyles.HexNumber),
				byte.Parse(((string)e.NewValue).Substring(2,2), System.Globalization.NumberStyles.HexNumber),
				byte.Parse(((string)e.NewValue).Substring(4,2), System.Globalization.NumberStyles.HexNumber),
				byte.Parse(((string)e.NewValue).Substring(6,2), System.Globalization.NumberStyles.HexNumber)),
				null);
			picker.RaiseEvent(new RoutedPropertyChangedEventArgs<string>((string)e.OldValue, (string)e.NewValue, HexStringChangedEvent));
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

		private HsvColor? lastPreciseColor;

		private Canvas shadeCanvas;
		private FrameworkElement shadeSelector;
		private ColorSpectrumSlider spectrumSlider;
		private TextBox tbA;
		private TextBox tbR;
		private TextBox tbG;
		private TextBox tbB;
		private TextBox tbHexString;
		
		#region logic

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			shadeCanvas = this.Template.FindName("PART_ColorShadeCanvas", this) as Canvas;
			shadeSelector = this.Template.FindName("PART_ColorShadeSelector", this) as FrameworkElement;
			spectrumSlider = this.Template.FindName("PART_ColorSpectrumSlider", this) as ColorSpectrumSlider;
			tbA = this.Template.FindName("PART_TextBoxA", this) as TextBox;
			tbR = this.Template.FindName("PART_TextBoxR", this) as TextBox;
			tbG = this.Template.FindName("PART_TextBoxG", this) as TextBox;
			tbB = this.Template.FindName("PART_TextBoxB", this) as TextBox;
			tbHexString = this.Template.FindName("PART_TextBoxHexString", this) as TextBox;
			
			shadeCanvas.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(shadeCanvas_PreviewMouseLeftButtonDown);
			shadeCanvas.MouseMove += new MouseEventHandler(shadeCanvas_MouseMove);
			shadeCanvas.MouseLeftButtonUp += new MouseButtonEventHandler(shadeCanvas_MouseLeftButtonUp);
			spectrumSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(spectrumSlider_ValueChanged);

			UpdateColor(Color, null);
		}

		void spectrumSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			HsvColor hsvColor = lastPreciseColor.HasValue ? lastPreciseColor.Value : HsvColor.FromArgb(Color);
			hsvColor.H = spectrumSlider.Value;
			UpdateColor(hsvColor.ToArgb(), hsvColor);
		}

		void shadeCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point p = e.GetPosition(shadeCanvas);
			UpdateColorFromPoint(p);
			shadeCanvas.CaptureMouse();
			e.Handled = true;
		}

		void shadeCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			shadeCanvas.ReleaseMouseCapture();
		}

		void shadeCanvas_MouseMove(object sender, MouseEventArgs e)
		{
			if (shadeCanvas.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
			{
				Point p = e.GetPosition(shadeCanvas);
				UpdateColorFromPoint(p);
			}
		}

		protected void UpdateColorFromPoint(Point p)
		{
			if (p.X < 0) p.X = 0;
			if (p.X > shadeCanvas.ActualWidth) p.X = shadeCanvas.ActualWidth;
			if (p.Y < 0) p.Y = 0;
			if (p.Y > shadeCanvas.ActualHeight) p.Y = shadeCanvas.ActualHeight;

			HsvColor hsvColor = lastPreciseColor.HasValue ? lastPreciseColor.Value : HsvColor.FromArgb(spectrumSlider.SelectedColor);
			if (!lastPreciseColor.HasValue)
			{
				hsvColor.A = (double)Color.A / 255;
			}
			hsvColor.S = p.X / shadeCanvas.ActualWidth;
			hsvColor.V = 1 - p.Y / shadeCanvas.ActualHeight;
			UpdateColor(hsvColor.ToArgb(), hsvColor);
		}

		bool updatingColor = false;

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

		#endregion
	}
}