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
using System.Threading;
using DoubanFM.Core;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using DoubanFM.Wmi;

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
					ExceptionMessage = exceptionObject == null ? string.Empty : GetExceptionMessage(exceptionObject);
					TbExceptionMessage.Text = string.Format("系统信息：\r\n{0}\r\n异常信息：\r\n{1}", SystemInformation, ExceptionMessage);
					TbShortExceptionMessage.Text = exceptionObject == null ? string.Empty : GetShortExceptionMessage(exceptionObject);
					if (exceptionObject != null)
						Debug.WriteLine(TbExceptionMessage.Text);
				}
			}
		}

		public string ExceptionMessage;
		public string SystemInformation = GetSystemInformation();

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

		public static string GetShortExceptionMessage(object exceptionObject)
		{
			if (exceptionObject is Exception)
			{
				StringBuilder sb = new StringBuilder();
				var ex = exceptionObject as Exception;
				while (ex != null)
				{
					sb.AppendLine(ex.Message);
					ex = ex.InnerException;
				}
				return sb.ToString();
			}
			return string.Empty;
		}

		public static string GetSystemInformation()
		{
			StringBuilder sb = new StringBuilder();
			try
			{
				var os = DoubanFM.Wmi.OperatingSystem.GetInstance();
				sb.AppendLine("操作系统：" + string.Format("{0} ({1})", os.Caption, Environment.OSVersion));
			}
			catch { }
			try
			{
				sb.AppendLine("工作目录：" + Environment.CurrentDirectory);
			}
			catch { }
			try
			{
				sb.AppendLine("命令行：" + Environment.CommandLine);
			}
			catch { }
			try
			{
				sb.AppendLine("Temp文件夹：" + System.IO.Path.GetTempPath());
			}
			catch { }
			try
			{
				sb.AppendLine("是否是64位操作系统：" + (Environment.Is64BitOperatingSystem ? "是" : "否"));
			}
			catch { }
			try
			{
				sb.AppendLine("是否是64位进程：" + (Environment.Is64BitProcess ? "是" : "否"));
			}
			catch { }
			try
			{
				sb.AppendLine("处理器数：" + Environment.ProcessorCount);
			}
			catch { }
			try
			{
				sb.AppendLine("系统目录：" + Environment.SystemDirectory);
			}
			catch { }
			try
			{
				sb.AppendLine("CLR版本：" + Environment.Version);
			}
			catch { }
			try
			{
				sb.AppendLine("工作集大小：" + Environment.WorkingSet);
			}
			catch { }
			try
			{
				sb.AppendLine("屏幕分辨率：" + SystemParameters.PrimaryScreenWidth + "x" + SystemParameters.PrimaryScreenHeight);
			}
			catch { }
			try
			{
				sb.AppendLine("工作区大小：" + SystemParameters.WorkArea.Width + "x" + SystemParameters.WorkArea.Height);
			}
			catch { }
			try
			{
				Processor[] processors = Processor.GetInstances();
				foreach (var processor in processors)
				{
					sb.AppendLine("处理器：" + processor.Name);
				}
			}
			catch { }
			try
			{
				PhysicalMemory[] memories = PhysicalMemory.GetInstances();
				ulong memorySize = 0;
				foreach (var memory in memories)
				{
					memorySize += memory.Capacity;
				}
				sb.AppendLine("内存：" + PhysicalMemory.FormatBytes(memorySize));
			}
			catch { }
			try
			{
				DisplayConfiguration[] displayes = DisplayConfiguration.GetInstances();
				foreach (var display in displayes)
				{
					sb.AppendLine("显卡：" + display.DeviceName);
				}
			}
			catch { }
			try
			{
				SoundDevice[] devices = SoundDevice.GetInstances();
				foreach (var device in devices)
				{
					sb.AppendLine("声卡：" + device.Name);
				}
			}
			catch { }

			return sb.ToString();
		}

		public ExceptionWindow()
		{
			InitializeComponent();

#if DEBUG || TEST
			BtnViewExceptionMessage.Visibility = System.Windows.Visibility.Visible;
			BtnOK.Visibility = System.Windows.Visibility.Collapsed;
			UserMessagePanel.Visibility = Visibility.Collapsed;
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
			TbUserMessage.IsReadOnly = true;
			if (exceptionObject != null)
			{
				PbSending.Visibility = System.Windows.Visibility.Visible;
				string userMessage = TbUserMessage.Text;
				ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
				{
					Assembly assembly = Assembly.GetEntryAssembly();
					string productName = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute))).Product;
					string versionNumber = assembly.GetName().Version.ToString();
					
					Parameters parameters = new Parameters();
					parameters.Add(new UrlParameter("ProductName", productName));
					parameters.Add(new UrlParameter("VersionNumber", versionNumber));
					parameters.Add(new UrlParameter("SystemInformation", SystemInformation));
					parameters.Add(new UrlParameter("Exception", ExceptionMessage));
					parameters.Add(new UrlParameter("UserMessage", userMessage));
					string result = new ConnectionBase().Post("http://www.kfstorm.com/products/errorfeedback.php", Encoding.UTF8.GetBytes(parameters.ToString()));
					//string result = new ConnectionBase().Post("http://localhost/phpStudy/ProductManager/errorfeedback.php", Encoding.UTF8.GetBytes(parameters.ToString()));
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

		private void BtnViewExceptionMessage_Click(object sender, RoutedEventArgs e)
		{
			ErrorPicture.Visibility = System.Windows.Visibility.Collapsed;
			TbExceptionMessage.Visibility = System.Windows.Visibility.Visible;
			ShortMessagePanel.Visibility = System.Windows.Visibility.Collapsed;
		}
	}
}