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
using System.Windows.Shapes;

namespace DoubanFM
{
	/// <summary>
	/// 界面设置窗口
	/// </summary>
	public partial class UISettingWindow : ChildWindowBase
	{
		public UISettingWindow()
		{
			InitializeComponent();
		}

		private void CheckBoxAlwaysShowNotifyIcon_IsCheckedChanged(object sender, RoutedEventArgs e)
		{
			if (CheckBoxAlwaysShowNotifyIcon.IsChecked == false)
				(Owner as DoubanFMWindow).NotifyIcon.Visibility = Owner.IsVisible ? Visibility.Hidden : Visibility.Visible;
			else (Owner as DoubanFMWindow).NotifyIcon.Visibility = System.Windows.Visibility.Visible;
		}

		private void BtnScaleTransformReset_Click(object sender, RoutedEventArgs e)
		{
			(FindResource("Player") as DoubanFM.Core.Player).Settings.ScaleTransform = 1.0;
		}

		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (e.OldValue == 1.0 && e.NewValue != 1.0)
				TextOptions.SetTextFormattingMode(this.Owner, TextFormattingMode.Ideal);
			else if (e.OldValue != 1.0 && e.NewValue == 1.0)
				TextOptions.SetTextFormattingMode(this.Owner, TextFormattingMode.Display);
		}
	}
}
