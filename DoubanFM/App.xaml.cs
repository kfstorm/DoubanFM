/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Windows.Markup;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using DoubanFM.Core;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DoubanFM
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			Debug.AutoFlush = true;
			Debug.Listeners.Add(new TextWriterTraceListener("DoubanFM.log"));
			Debug.WriteLine(string.Empty);
			Debug.WriteLine("**********************************************************************");
			Debug.WriteLine("豆瓣电台启动时间：" + App.GetPreciseTime(DateTime.Now));
			Debug.WriteLine("**********************************************************************");
			Debug.WriteLine(string.Empty);

			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, e) =>
			{
				Debug.WriteLine("**********************************************************************");
				Debug.WriteLine("豆瓣电台出现错误：" + App.GetPreciseTime(DateTime.Now));
				Debug.WriteLine("**********************************************************************");
				Debug.WriteLine("错误信息：");
				Exception ex = e.ExceptionObject as Exception;
				while (ex != null)
				{
					Debug.WriteLine(ex.Message);
					ex = ex.InnerException;
				}
				Debug.WriteLine(e.ExceptionObject.ToString());
			});

			App.Current.Exit += new ExitEventHandler((sender, e) =>
			{
				Debug.WriteLine(App.GetPreciseTime(DateTime.Now) + " 程序结束，返回代码为" + e.ApplicationExitCode);
			});

			//System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

			/* 这句话可以使Global User Interface这个默认的组合字体按当前系统的区域信息选择合适的字形。
			 * 只对FrameworkElement有效。对于FlowDocument，由于是从FrameworkContentElement继承，
			 * 而且FrameworkContentElement.LanguageProperty.OverrideMetadata()无法再次执行，
			 * 目前我知道的最好的办法是在使用了FlowDocument的XAML的根元素上加上xml:lang="zh-CN"，
			 * 这样就能强制Global User Interface在FlowDocument上使用大陆的字形。
			 * */
			FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));

			Channel channel = Channel.FromCommandLineArgs(System.Environment.GetCommandLineArgs().ToList());
			//只允许运行一个实例
			if (HasAnotherInstance())
			{
				if (channel != null) WriteChannelToMappedFile(channel);
				Debug.WriteLine("检测到已有一个豆瓣电台在运行，程序将关闭");
				App.Current.Shutdown(0);
				return;
			}
			else
			{
				SplashScreen splashScreen = new SplashScreen("Images/SplashScreen.png");
				splashScreen.Show(true, true);
			}
		}

		public static string GetPreciseTime(DateTime time)
		{
			return time.ToString() + " " + time.Millisecond + "ms";
		}

		/// <summary>
		/// 内存映射文件的文件名
		/// </summary>
		public static string _mappedFileName = "{04EFCEB4-F10A-403D-9824-1E685C4B7961}";
		
		/// <summary>
		/// 检测是否有另一个实例正在运行
		/// </summary>
		protected bool HasAnotherInstance()
		{
			try
			{
				MemoryMappedFile mappedFile = MemoryMappedFile.OpenExisting(_mappedFileName);
				return mappedFile != null;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// 将频道写入内存映射文件
		/// </summary>
		protected void WriteChannelToMappedFile(Channel channel)
		{
			if (channel != null)
				try
				{
					using (MemoryMappedFile mappedFile = MemoryMappedFile.OpenExisting(_mappedFileName))
					using (Stream stream = mappedFile.CreateViewStream())
					{
						BinaryFormatter formatter = new BinaryFormatter();
						formatter.Serialize(stream, channel);
					}
				}
				catch { }
		}
	}
}