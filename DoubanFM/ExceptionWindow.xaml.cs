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
using System.Threading;
using DoubanFM.Core;
using System.Reflection;
using System.Diagnostics;

namespace DoubanFM
{
	/// <summary>
	/// ExceptionWindow.xaml 的交互逻辑
	/// </summary>
	public partial class ExceptionWindow : ChildWindowBase
	{
		private object exceptionObject;
		public object ExceptionObject
		{
			get { return exceptionObject; }
			set
			{
				if (exceptionObject != value)
				{
					exceptionObject = value;
					TbErrorReport.Text = exceptionObject == null ? string.Empty : GetExceptionMessage(exceptionObject);
				}
			}
		}

		public static string GetExceptionMessage(object exceptionObject)
		{
			if (exceptionObject == null) return string.Empty;
			if (exceptionObject is Exception)
			{
				StringBuilder sb = new StringBuilder();
				var ex = exceptionObject as Exception;
				while (ex != null)
				{
					sb.AppendLine("************** Exception **************");
					sb.AppendLine(ex.ToString());
					ex = ex.InnerException;
				}
				return sb.ToString();
			}
			else
			{
				return exceptionObject.ToString();
			}
		}

		public ExceptionWindow()
		{
			InitializeComponent();

#if DEBUG || TEST
			BtnViewErrorReport.Visibility = System.Windows.Visibility.Visible;
			BtnOK.Visibility = System.Windows.Visibility.Collapsed;
#endif
			if (Owner == null)
			{
				ShowInTaskbar = true;
				WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
			}
		}

		private void BtnOK_Click(object sender, RoutedEventArgs e)
		{
			BtnOK.IsEnabled = false;
			if (exceptionObject != null)
			{
				PbSending.Visibility = System.Windows.Visibility.Visible;
				string exception = TbErrorReport.Text;
				ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
				{
					Assembly assembly = Assembly.GetEntryAssembly();
					string productName = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute))).Product;
					string versionNumber = assembly.GetName().Version.ToString();

					Parameters parameters = new Parameters();
					parameters.Add(new UrlParameter("ProductName", productName));
					parameters.Add(new UrlParameter("VersionNumber", versionNumber));
					parameters.Add(new UrlParameter("Exception", exception));
					string result = new ConnectionBase().Post("http://www.kfstorm.com/products/errorfeedback.php", Encoding.UTF8.GetBytes(parameters.ToString()));
					Debug.WriteLine(result);
					Dispatcher.BeginInvoke(new Action(() =>
						{
							this.Close();
						}));
				}));
			}
			else
			{
				this.Close();
			}
		}

		private void BtnCannel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void BtnViewErrorReport_Click(object sender, RoutedEventArgs e)
		{
			ErrorImage.Visibility = System.Windows.Visibility.Collapsed;
			Oops.Visibility = System.Windows.Visibility.Collapsed;
			TbErrorReport.Visibility = System.Windows.Visibility.Visible;
		}
	}
}