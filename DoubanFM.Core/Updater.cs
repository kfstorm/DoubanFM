/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;

namespace DoubanFM.Core
{
	/// <summary>
	/// 更新器
	/// </summary>
	public class Updater : DispatcherObject, IDisposable
	{
		#region 属性

		/// <summary>
		/// 状态
		/// </summary>
		public enum State { UnStarted, Checking, CheckFailed, HasNewVersion, NoNewVersion, Downloading, DownloadCompleted, DownloadFailed, Canneled };
		/// <summary>
		/// 当前状态
		/// </summary>
		public State Now
		{
			get { return _now; }
			private set
			{
				_now = value;
				switch (Now)
				{
					case State.NoNewVersion:
					case State.HasNewVersion:
						Settings.LastTimeCheckUpdate = DateTime.Now;
						break;
				}
				RaiseStateChangedEvent();
			}
		}
		/// <summary>
		/// 产品名称
		/// </summary>
		public string ProductName { get; private set; }
		/// <summary>
		/// 当前版本号
		/// </summary>
		public string VersionNumber { get; private set; }
		/// <summary>
		/// 更新的版本
		/// </summary>
		public Products NewerProducts { get; private set; }
		/// <summary>
		/// 新版本名称
		/// </summary>
		public string NewVersionName { get; private set; }
		/// <summary>
		/// 新版本发布日期
		/// </summary>
		public string NewVersionPublishTime { get; private set; }
		/// <summary>
		/// 下载链接
		/// </summary>
		public string DownloadLink { get; private set; }
		/// <summary>
		/// 当前工作
		/// </summary>
		public UpdateWork Work { get; private set; }
		/// <summary>
		/// 是否正在工作
		/// </summary>
		public bool Working
		{
			get { return Now == State.Checking || Now == State.Downloading; }
		}
		/// <summary>
		/// 用于下载
		/// </summary>
		private WebClient _client = new WebClient();
		/// <summary>
		/// 下载好的文件存放的路径
		/// </summary>
		public string DownloadedFilePath { get; private set; }
		/// <summary>
		/// 错误信息
		/// </summary>
		public Exception LastError { get; private set; }
		/// <summary>
		/// 设置
		/// </summary>
		public Settings Settings { get; private set; }
		/// <summary>
		/// 临时文件夹
		/// </summary>
		private string _tempPath = Path.GetTempPath() + "DoubanFM";

		#endregion

		private State _now = State.UnStarted;

		/// <summary>
		/// 当状态改变时发生
		/// </summary>
		public event EventHandler StateChanged;

		void RaiseStateChangedEvent()
		{
			if (StateChanged != null)
				StateChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// 当下载进度改变时发生
		/// </summary>
		public event DownloadProgressChangedEventHandler ProgressChanged;

		void RaiseProgressChangedEvent(DownloadProgressChangedEventArgs e)
		{
			if (ProgressChanged != null)
				ProgressChanged(this, e);
		}

		public Updater(Settings settings)
		{
			Settings = settings;
			Assembly assembly = Assembly.GetEntryAssembly();
			ProductName = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute))).Product;
			VersionNumber = assembly.GetName().Version.ToString();
			_client.DownloadFileCompleted += new AsyncCompletedEventHandler((o, e) =>
			{
				if (e.Cancelled) return;
				Dispatcher.BeginInvoke(new Action(() =>
				{
					if (e.Error != null)
						DownloadFailed(e.Error);
					else DownloadCompleted();
				}));
			});
			_client.DownloadProgressChanged += new DownloadProgressChangedEventHandler((o, e) =>
			{
				Dispatcher.BeginInvoke(new Action(() =>
					{
						RaiseProgressChangedEvent(e);
					}));
			});
		}

		#region 公共方法

		/// <summary>
		/// 开始检查更新
		/// </summary>
		public void Start()
		{
			if (Now != State.UnStarted)
				throw new InvalidOperationException();
			else Checking();
		}

		/// <summary>
		/// 取消更新
		/// </summary>
		public void Cancel()
		{
			Canceled();
		}

		/// <summary>
		/// 重试检查更新
		/// </summary>
		public void ReCheck()
		{
			if (Now != State.CheckFailed)
				throw new InvalidOperationException();
			else Checking();
		}

		/// <summary>
		/// 开始下载
		/// </summary>
		public void Download()
		{
			if (Now != State.HasNewVersion && Now != State.DownloadFailed)
				throw new InvalidOperationException();
			else Downloading();
		}

		public void Dispose()
		{
			if (Working)
			{
				if (Work != null && Work.Working)
				{
					Work.Dispose();
					Work = null;
				}
				if (_client != null && _client.IsBusy)
				{
					_client.CancelAsync();
				}
			}
			if (_client != null)
			{
				_client.Dispose();
				_client = null;
			}
		}

		#endregion

		#region 私有方法

		/// <summary>
		/// 正在检查更新
		/// </summary>
		void Checking()
		{
			Parameters parameters = new Parameters();
			parameters.Add(new UrlParameter("ProductName", ProductName));
			parameters.Add(new UrlParameter("VersionNumber", VersionNumber));
			string url = ConnectionBase.ConstructUrlWithParameters("http://www.kfstorm.com/products/check.php", parameters);
			Now = State.Checking;
			UpdateWork work = new UpdateWork(new ThreadStart(() =>
				{
					string file = null;
					try
					{
						file = new ConnectionBase(true).Get(url);
						if (string.IsNullOrEmpty(file))
							Dispatcher.BeginInvoke(new Action(() => { CheckFailed(new Exception("网络错误")); }));
						else
						{
							using (MemoryStream stream = new MemoryStream())
							using (StreamWriter writer = new StreamWriter(stream))
							{
								writer.Write(file);
								writer.Flush();
								XmlSerializer serializer = new XmlSerializer(typeof(UpdateResult));
								stream.Position = 0;
								UpdateResult result = (UpdateResult)serializer.Deserialize(stream);
								Dispatcher.BeginInvoke(new Action(() =>
									{
										if (!string.IsNullOrEmpty(result.Error)) CheckFailed(new Exception(result.Error));
										else
											if (result.Products.Count > 0) HasNewVersion(result.Products);
											else NoNewVersion();
									}));
							}
						}
					}
					catch (Exception e)
					{
						Dispatcher.BeginInvoke(new Action(() => { CheckFailed(e); }));
					}
				}));
			work.Start();
		}

		/// <summary>
		/// 检查更新失败
		/// </summary>
		/// <param name="error">The error.</param>
		void CheckFailed(Exception error)
		{
			LastError = error;
			Now = State.CheckFailed;
		}

		/// <summary>
		/// 有新版本
		/// </summary>
		/// <param name="products">The products.</param>
		void HasNewVersion(Products products)
		{
			NewerProducts = products;
			NewVersionName = products[0].VersionName;
			NewVersionPublishTime = products[0].PublishTime;
			DownloadLink = products[0].DownloadLink;
			Match mc = Regex.Match(DownloadLink, @".*/(.*)");
			string name = mc.Groups[1].Value;
			DownloadedFilePath = _tempPath + @"\" + name;
			Now = State.HasNewVersion;
		}

		/// <summary>
		/// 没有新版本
		/// </summary>
		void NoNewVersion()
		{
			Now = State.NoNewVersion;
		}

		/// <summary>
		/// 正在下载
		/// </summary>
		void Downloading()
		{
			Now = State.Downloading;
			try
			{
				if (!Directory.Exists(_tempPath))
					Directory.CreateDirectory(_tempPath);
				_client.DownloadFileAsync(new Uri(DownloadLink), DownloadedFilePath);
			}
			catch (Exception e)
			{
				DownloadFailed(e);
			}
		}

		/// <summary>
		/// 下载完成
		/// </summary>
		void DownloadCompleted()
		{
			Now = State.DownloadCompleted;
		}

		/// <summary>
		/// 下载失败
		/// </summary>
		/// <param name="error">The error.</param>
		void DownloadFailed(Exception error)
		{
			LastError = error;
			Now = State.DownloadFailed;
		}

		/// <summary>
		/// 已取消
		/// </summary>
		void Canceled()
		{
			Dispose();
			Now = State.Canneled;
		}

		#endregion

	}
}
