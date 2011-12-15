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
	[TemplatePart(Name="PART_Thumb", Type=typeof(System.Windows.Controls.Primitives.Thumb))]
	[TemplatePart(Name="PART_Spectrum", Type=typeof(FrameworkElement))]
	public class ColorSpectrumSlider : Slider
	{
		static ColorSpectrumSlider()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorSpectrumSlider), new FrameworkPropertyMetadata(typeof(ColorSpectrumSlider)));
		}

		public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorSpectrumSlider), new PropertyMetadata(Colors.Red));
		public Color SelectedColor
		{
			get { return (Color)GetValue(SelectedColorProperty); }
			set { SetValue(SelectedColorProperty, value); }
		}

		System.Windows.Controls.Primitives.Thumb thumb;
		FrameworkElement spectrum;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			thumb = this.Template.FindName("PART_Thumb", this) as System.Windows.Controls.Primitives.Thumb;
			spectrum = this.Template.FindName("PART_Spectrum", this) as FrameworkElement;
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			Point p = e.GetPosition(spectrum);
			Value = (p.Y / spectrum.ActualHeight) * (this.Maximum - this.Minimum) + this.Minimum;
			this.CaptureMouse();
			this.Focus();
			e.Handled = true;

			base.OnPreviewMouseLeftButtonDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (this.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
			{
				Point p = e.GetPosition(spectrum);
				Value = (p.Y / spectrum.ActualHeight) * (this.Maximum - this.Minimum) + this.Minimum;
			}
			base.OnMouseMove(e);
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			this.ReleaseMouseCapture();
			base.OnMouseLeftButtonUp(e);
		}

		protected override void OnValueChanged(double oldValue, double newValue)
		{
			base.OnValueChanged(oldValue, newValue);

			SelectedColor = new HsvColor(1, newValue, 1, 1).ToArgb();
		}

	}
}
