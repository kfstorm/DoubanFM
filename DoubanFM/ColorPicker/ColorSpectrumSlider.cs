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
	/// 颜色频谱选择器
	/// </summary>
	[TemplatePart(Name="PART_Thumb", Type=typeof(System.Windows.Controls.Primitives.Thumb))]
	[TemplatePart(Name="PART_Spectrum", Type=typeof(FrameworkElement))]
	public class ColorSpectrumSlider : Slider
	{
		static ColorSpectrumSlider()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorSpectrumSlider), new FrameworkPropertyMetadata(typeof(ColorSpectrumSlider)));
		}

		public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorSpectrumSlider), new PropertyMetadata(Colors.Red));
		/// <summary>
		/// 选择的频谱颜色
		/// </summary>
		public Color SelectedColor
		{
			get { return (Color)GetValue(SelectedColorProperty); }
			set { SetValue(SelectedColorProperty, value); }
		}

		/// <summary>
		/// 用于选择频谱颜色的控件
		/// </summary>
		System.Windows.Controls.Primitives.Thumb thumb;
		/// <summary>
		/// 用于显示频谱的框架元素
		/// </summary>
		FrameworkElement spectrum;

		/// <summary>
		/// 生成 <see cref="T:System.Windows.Controls.Slider"/> 控件的可视化树。
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			thumb = this.Template.FindName("PART_Thumb", this) as System.Windows.Controls.Primitives.Thumb;
			spectrum = this.Template.FindName("PART_Spectrum", this) as FrameworkElement;
		}

		/// <summary>
		/// 按下鼠标左键时触发
		/// </summary>
		/// <param name="e">事件数据。</param>
		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			Point p = e.GetPosition(spectrum);
			Value = (p.Y / spectrum.ActualHeight) * (this.Maximum - this.Minimum) + this.Minimum;
			this.CaptureMouse();
			this.Focus();
			e.Handled = true;

			base.OnPreviewMouseLeftButtonDown(e);
		}

		/// <summary>
		/// 鼠标移动时触发
		/// </summary>
		/// <param name="e">包含事件数据的 <see cref="T:System.Windows.Input.MouseEventArgs"/>。</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (this.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
			{
				Point p = e.GetPosition(spectrum);
				Value = (p.Y / spectrum.ActualHeight) * (this.Maximum - this.Minimum) + this.Minimum;
			}
			base.OnMouseMove(e);
		}

		/// <summary>
		/// 松开鼠标左键时触发
		/// </summary>
		/// <param name="e">包含事件数据的 <see cref="T:System.Windows.Input.MouseButtonEventArgs"/>。事件数据将报告已释放了鼠标左键。</param>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			this.ReleaseMouseCapture();
			base.OnMouseLeftButtonUp(e);
		}

		/// <summary>
		/// 颜色更改时触发
		/// </summary>
		/// <param name="oldValue"><see cref="T:System.Windows.Controls.Slider"/> 的旧 <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value"/>。</param>
		/// <param name="newValue"><see cref="T:System.Windows.Controls.Slider"/> 的新 <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value"/>。</param>
		protected override void OnValueChanged(double oldValue, double newValue)
		{
			base.OnValueChanged(oldValue, newValue);

			SelectedColor = new HsvColor(1, newValue, 1, 1).ToArgb();
		}

	}
}
