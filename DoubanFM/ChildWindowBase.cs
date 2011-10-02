using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DoubanFM
{
	/// <summary>
	/// DouanFMWindow的子窗口基类
	/// </summary>
	public class ChildWindowBase : WindowBase
	{
		public ChildWindowBase()
		{
			this.Style = FindResource("ChildWindowBaseStyle") as System.Windows.Style;
			
			Owner = App.Current.MainWindow;
			AllowsTransparency = Owner.AllowsTransparency;
			this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;

			this.Loaded += new RoutedEventHandler((o, e) =>
			{
				System.Windows.Data.Binding binding = new System.Windows.Data.Binding("Background");
				binding.Source = Owner;
				this.SetBinding(UpdateWindow.BackgroundProperty, binding);
			});
			this.Closing += new System.ComponentModel.CancelEventHandler((o, e) =>
			{
				Owner.Activate();
			});
		}
	}
}
