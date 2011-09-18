using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Controls;

namespace DoubanFM
{
	public partial class Style : System.Windows.ResourceDictionary
	{
	
		private void Link_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			// 在此处添加事件处理程序实现。
			Process.Start((string)((Button)sender).Tag);
		}
	}
}
