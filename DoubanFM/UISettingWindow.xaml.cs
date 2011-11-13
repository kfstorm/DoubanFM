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
	/// UISettingWindow.xaml 的交互逻辑
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
				(Owner as DoubanFMWindow)._notifyIcon.Visible = !this.IsVisible;
			else (Owner as DoubanFMWindow)._notifyIcon.Visible = true;
		}

		private void BtnScaleTransformReset_Click(object sender, RoutedEventArgs e)
		{
			(FindResource("Player") as DoubanFM.Core.Player).Settings.ScaleTransform = 1.0;
		}
	}
}
