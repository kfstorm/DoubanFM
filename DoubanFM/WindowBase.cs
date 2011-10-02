using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace DoubanFM
{
	/// <summary>
	/// 窗口基类
	/// </summary>
	public class WindowBase : Window
	{
		public WindowBase()
		{
			this.Resources.Add("ShadowGradientStops", new GradientStopCollection());
			this.Style = FindResource("WindowBaseStyle") as System.Windows.Style;

			this.Loaded += new RoutedEventHandler((o, e) =>
			{
				Panel mainPanel = this.Template.FindName("MainPanel", this) as Panel;
				if (!AllowsTransparency) mainPanel.Margin = new Thickness(1);
			});
			this.Activated += new EventHandler((o, e) =>
			{
				GradientStopCollection active = this.FindResource("ActiveShadowGradientStops") as GradientStopCollection;
				GradientStopCollection now = this.FindResource("ShadowGradientStops") as GradientStopCollection;
				now.Clear();
				foreach (var g in active)
					now.Add(g);
			});
			this.Deactivated += new EventHandler((o, e) =>
			{
				GradientStopCollection inactive = this.FindResource("InactiveShadowGradientStops") as GradientStopCollection;
				GradientStopCollection now = this.FindResource("ShadowGradientStops") as GradientStopCollection;
				now.Clear();
				foreach (var g in inactive)
					now.Add(g);
			});
		}
	}
}
