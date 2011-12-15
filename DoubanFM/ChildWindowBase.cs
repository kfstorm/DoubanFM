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
	[TemplatePart(Name = "PART_Close", Type = typeof(Button))]
	public class ChildWindowBase : WindowBase
	{
		static ChildWindowBase()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ChildWindowBase), new FrameworkPropertyMetadata(typeof(ChildWindowBase)));
		}
		
		public ChildWindowBase()
		{
			Owner = Application.Current.MainWindow;
			AllowsTransparency = Owner.AllowsTransparency;
			this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			System.Windows.Data.Binding binding = new System.Windows.Data.Binding("Background");
			binding.Source = Owner;
			this.SetBinding(UpdateWindow.BackgroundProperty, binding);
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			Owner.Activate();

			base.OnClosing(e);
		}

		private Button btnClose;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			btnClose = this.Template.FindName("PART_Close", this) as Button;
			if (btnClose != null)
			{
				btnClose.Click += new RoutedEventHandler(btnClose_Click);
			}
		}

		void btnClose_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
