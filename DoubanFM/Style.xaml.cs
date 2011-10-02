using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace DoubanFM
{
	public partial class Style : ResourceDictionary
	{
	
		private void Link_Click(object sender, RoutedEventArgs e)
		{
			// 在此处添加事件处理程序实现。
			Process.Start((string)((Button)sender).Tag);
		}

		private void WindowBaseStyle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			//如果不加try catch语句，在点击封面打开资料页面时很容易报错
			try
			{
				(sender as Window).DragMove();
			}
			catch { }
		}

		private void ChildWindowBaseStyle_ButtonClose_Click(object sender, RoutedEventArgs e)
		{
			((sender as FrameworkElement).TemplatedParent as Window).Close();
		}
	}
}
