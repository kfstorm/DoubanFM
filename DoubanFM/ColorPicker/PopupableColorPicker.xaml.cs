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

		#endregion

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			popup.IsOpen = true;
		}
	}
}
