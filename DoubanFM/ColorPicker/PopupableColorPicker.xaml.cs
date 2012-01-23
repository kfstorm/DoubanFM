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
	/// PopupableColorPicker.xaml 的交互逻辑
	/// </summary>
	public partial class PopupableColorPicker : UserControl
	{
		public PopupableColorPicker()
		{
			InitializeComponent();
		}

		#region Color

		public Color Color
		{
			get { return (Color)GetValue(ColorProperty); }
			set { SetValue(ColorProperty, value); }
		}

		public static readonly DependencyProperty ColorProperty =
			DependencyProperty.Register("Color", typeof(Color), typeof(PopupableColorPicker), new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



		public bool IsAlphaEnabled
		{
			get { return (bool)GetValue(IsAlphaEnabledProperty); }
			set { SetValue(IsAlphaEnabledProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsAlphaEnabled.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsAlphaEnabledProperty =
			DependencyProperty.Register("IsAlphaEnabled", typeof(bool), typeof(PopupableColorPicker), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		#endregion

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			popup.IsOpen = true;
		}
	}
}
