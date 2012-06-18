using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Threading;
using System.Runtime.InteropServices;

namespace DoubanFM.Bass
{

	/// <summary>
	/// Bass播放器
	/// </summary>
	public class BassEngine : WPFSoundVisualizationLib.ISpectrumPlayer, INotifyPropertyChanged, IDisposable
	{
		#region Fields
		/// <summary>
		/// BassEngine的唯一实例
		/// </summary>
		private static BassEngine instance;
		/// <summary>
		/// 用于更新播放进度的计时器
		/// </summary>
		private readonly DispatcherTimer positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
		private readonly int maxFFT = (int)(Un4seen.Bass.BASSData.BASS_DATA_AVAILABLE | Un4seen.Bass.BASSData.BASS_DATA_FFT4096);
		/// <summary>
		/// 当播放结束时调用
		/// </summary>
		private readonly Un4seen.Bass.SYNCPROC endTrackSyncProc;
		private int sampleFrequency = 44100;
		/// <summary>
		/// 当前流的句柄
		/// </summary>
		private int activeStreamHandle;
		/// <summary>
		/// 可以使用播放命令
		/// </summary>
		private bool canPlay;
		/// <summary>
		/// 可以使用暂停命令
		/// </summary>
		private bool canPause;
		/// <summary>
		/// 是否正在播放
		/// </summary>
		private bool isPlaying;
		/// <summary>
		/// 可以使用停止命令
		/// </summary>
		private bool canStop;
		/// <summary>
		/// 音频长度
		/// </summary>
		private TimeSpan channelLength = TimeSpan.Zero;
		/// <summary>
		/// 当前播放进度
		/// </summary>
		private TimeSpan currentChannelPosition = TimeSpan.Zero;
		private bool inChannelSet;
		private bool inChannelTimerUpdate;
		/// <summary>
		/// 用于异步打开网络音频文件的线程
		/// </summary>
		private Thread onlineFileWorker;
		/// <summary>
		/// 待执行的命令
		/// </summary>
		enum PendingOperation
		{
			/// <summary>
			/// 无
			/// </summary>
			None = 0,
			/// <summary>
			/// 播放
			/// </summary>
			Play,
			/// <summary>
			/// 暂停
			/// </summary>
			Pause
		};
		/// <summary>
		/// 待执行的命令，当打开网络上的音频时非常有用
		/// </summary>
		private PendingOperation pendingOperation = PendingOperation.None;
		/// <summary>
		/// 音量
		/// </summary>
		private double volume;
		/// <summary>
		/// 是否静音
		/// </summary>
		private bool isMuted;
		/// <summary>
		/// 代理服务器设置的非托管资源句柄
		/// </summary>
		private IntPtr proxyHandle = IntPtr.Zero;
		/// <summary>
		/// 保存正在打开的文件的地址，当短时间内多次打开网络文件时，这个字段保存最后一次打开的文件，可以使其他打开文件的操作失效
		/// </summary>
		private string openningFile = null;
		#endregion

		#region Constructor
		static BassEngine()
		{
			//注册Bass.Net，不注册就会弹出一个启动画面
			Un4seen.Bass.BassNet.Registration("yk000123@sina.com", "2X34201017282922");

			//判断当前系统是32位系统还是64位系统，并加载对应版本的bass.dll
			string targetPath;
			if (Un4seen.Bass.Utils.Is64Bit)
				targetPath = Path.Combine(Path.GetDirectoryName(typeof(BassEngine).Assembly.GetModules()[0].FullyQualifiedName), "x64");
			else
				targetPath = Path.Combine(Path.GetDirectoryName(typeof(BassEngine).Assembly.GetModules()[0].FullyQualifiedName), "x86");

			// now load all libs manually
			Un4seen.Bass.Bass.LoadMe(targetPath);
			//BassMix.LoadMe(targetPath);
			//...
			//loadedPlugIns = Bass.BASS_PluginLoadDirectory(targetPath);
			//...

			Un4seen.Bass.Bass.BASS_SetConfig(Un4seen.Bass.BASSConfig.BASS_CONFIG_DEV_DEFAULT, true);
		}

		private BassEngine(DeviceInfo? deviceInfo = null)
		{
			Initialize(deviceInfo);
			//设置播放结束的回调
			endTrackSyncProc = EndTrack;
		}
		#endregion

		#region Destructor
		~BassEngine()
		{
			Dispose(false);
		}
		#endregion

		#region IDisposable
		bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (onlineFileWorker != null)
					{
						onlineFileWorker.Abort();
						onlineFileWorker = null;
					}
				}
				// at the end of your application call this!
				Un4seen.Bass.Bass.BASS_Free();
				Un4seen.Bass.Bass.FreeMe();
				//BassMix.FreeMe(targetPath);
				//...
				//foreach (int plugin in LoadedBassPlugIns.Keys)
				//    Bass.BASS_PluginFree(plugin);

				if (proxyHandle != IntPtr.Zero)
					Marshal.FreeHGlobal(proxyHandle);
				proxyHandle = IntPtr.Zero;

				_disposed = true;
			}
		}
		#endregion

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		#endregion

		#region Singleton Instance
		/// <summary>
		/// 获取BassEngine的唯一实例
		/// </summary>
		public static BassEngine Instance
		{
			get
			{
				if (instance == null)
					instance = new BassEngine();
				return instance;
			}
		}
		/// <summary>
		/// 显式初始化
		/// </summary>
		public static void ExplicitInitialize(DeviceInfo? deviceInfo = null)
		{
			if (instance == null)
				instance = new BassEngine(deviceInfo);
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// 获取设备列表
		/// </summary>
		/// <returns></returns>
		public static DeviceInfo[] GetDeviceInfos()
		{
			List<DeviceInfo> results = new List<DeviceInfo>();
			var devices = Un4seen.Bass.Bass.BASS_GetDeviceInfos().ToList();
			foreach (var device in devices)
			{
				if (device.IsEnabled && !string.Equals(device.name, "No sound", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(device.name, "Default", StringComparison.CurrentCultureIgnoreCase))
				{
					results.Add(new DeviceInfo { ID = device.id, Name = device.name, Driver = device.driver });
				}
			}
			return results.ToArray();
		}
		/// <summary>
		/// 更换设备
		/// </summary>
		public void ChangeDevice(DeviceInfo? device)
		{
			int deviceNO = FindDevice(device);
			int oldDeviceNO = Un4seen.Bass.Bass.BASS_GetDevice();
			if (oldDeviceNO != deviceNO)
			{
				if (!Un4seen.Bass.Bass.BASS_GetDeviceInfo(deviceNO).IsInitialized)
				{
					IntPtr handle = IntPtr.Zero;
					if (Application.Current.MainWindow != null)
					{
						handle = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
					}
					if (!Un4seen.Bass.Bass.BASS_Init(deviceNO, sampleFrequency, Un4seen.Bass.BASSInit.BASS_DEVICE_DEFAULT, handle))
					{
						throw new Exception(Un4seen.Bass.Bass.BASS_ErrorGetCode().ToString());
					}
				}
				if (activeStreamHandle != 0)
				{
					if (!Un4seen.Bass.Bass.BASS_ChannelSetDevice(activeStreamHandle, deviceNO))
					{
						throw new Exception(Un4seen.Bass.Bass.BASS_ErrorGetCode().ToString());
					}
				}
				if (!Un4seen.Bass.Bass.BASS_SetDevice(oldDeviceNO))
				{
					throw new Exception(Un4seen.Bass.Bass.BASS_ErrorGetCode().ToString());
				}
				if (!Un4seen.Bass.Bass.BASS_Free())
				{
					throw new Exception(Un4seen.Bass.Bass.BASS_ErrorGetCode().ToString());
				}
				if (!Un4seen.Bass.Bass.BASS_SetDevice(deviceNO))
				{
					throw new Exception(Un4seen.Bass.Bass.BASS_ErrorGetCode().ToString());
				}
				
			}
			Device = device;
		}

		/// <summary>
		/// 停止当前音频，并释放资源
		/// </summary>
		public void Stop()
		{
			//Debug.WriteLine("已调用BassEngine.Stop()");

			if (canStop)
			{
				ChannelPosition = TimeSpan.Zero;
				if (ActiveStreamHandle != 0)
				{
					Un4seen.Bass.Bass.BASS_ChannelStop(ActiveStreamHandle);
					Un4seen.Bass.Bass.BASS_ChannelSetPosition(ActiveStreamHandle, ChannelPosition.TotalSeconds);
					//Debug.WriteLine("已调用BASS_ChannelStop()");
				}
				IsPlaying = false;
				CanStop = false;
				CanPlay = false;
				CanPause = false;
			}

			FreeCurrentStream();
			pendingOperation = PendingOperation.None;
		}

		/// <summary>
		/// 暂停当前音频
		/// </summary>
		public void Pause()
		{
			//Debug.WriteLine("已调用BassEngine.Pause()");
			if (IsPlaying && CanPause)
			{
				Un4seen.Bass.Bass.BASS_ChannelPause(ActiveStreamHandle);
				IsPlaying = false;
				CanPlay = true;
				CanPause = false;
				pendingOperation = PendingOperation.None;
			}
			else
			{
				pendingOperation = PendingOperation.Pause;
			}
		}

		/// <summary>
		/// 播放当前音频
		/// </summary>
		public void Play()
		{
			//Debug.WriteLine("已调用BassEngine.Play()");
			if (CanPlay)
			{
				PlayCurrentStream();
				IsPlaying = true;
				CanPause = true;
				CanPlay = false;
				CanStop = true;
				pendingOperation = PendingOperation.None;
			}
			else
			{
				pendingOperation = PendingOperation.Play;
			}
		}

		/// <summary>
		/// 打开文件
		/// </summary>
		/// <param name="filename">文件名</param>
		public void OpenFile(string filename)
		{
			openningFile = filename;
			//Debug.WriteLine("已调用BassEngine.OpenFile()");
			Stop();
			pendingOperation = PendingOperation.None;

            int handle = Un4seen.Bass.Bass.BASS_StreamCreateFile(filename, 0, 0, Un4seen.Bass.BASSFlag.BASS_DEFAULT);

			if (handle != 0)
			{
				ActiveStreamHandle = handle;
				ChannelLength = TimeSpan.FromSeconds(Un4seen.Bass.Bass.BASS_ChannelBytes2Seconds(ActiveStreamHandle, Un4seen.Bass.Bass.BASS_ChannelGetLength(ActiveStreamHandle, 0)));
				Un4seen.Bass.BASS_CHANNELINFO info = new Un4seen.Bass.BASS_CHANNELINFO();
				Un4seen.Bass.Bass.BASS_ChannelGetInfo(ActiveStreamHandle, info);
				sampleFrequency = info.freq;

				int syncHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(ActiveStreamHandle,
					 Un4seen.Bass.BASSSync.BASS_SYNC_END,
					 0,
					 endTrackSyncProc,
					 IntPtr.Zero);

				if (syncHandle == 0)
					throw new ArgumentException("Error establishing End Sync on file stream.", "path");

				CanPlay = true;
				RaiseOpenSucceededEvent();

				switch (pendingOperation)
				{
					case PendingOperation.None:
						break;
					case PendingOperation.Play:
						Play();
						break;
					case PendingOperation.Pause:
						Pause();
						break;
					default:
						break;
				}
			}
			else
			{
				RaiseOpenFailedEvent();
			}
		}

		/// <summary>
		/// 打开网络地址
		/// </summary>
		/// <param name="url">URL地址</param>
		public void OpenUrlAsync(string url)
		{
			openningFile = url;
			//Debug.WriteLine("已调用BassEngine.OpenUrlAsync()");

			Stop();
			pendingOperation = PendingOperation.None;

			onlineFileWorker = new Thread(new ThreadStart(() =>
				{
					int handle = Un4seen.Bass.Bass.BASS_StreamCreateURL(url, 0, Un4seen.Bass.BASSFlag.BASS_DEFAULT, null, IntPtr.Zero);
					
					Application.Current.Dispatcher.BeginInvoke(new Action(() =>
						{
							if (handle != 0)
							{
								if (openningFile == url)		//该文件为正在打开的文件
								{
									ActiveStreamHandle = handle;
									ChannelLength = TimeSpan.FromSeconds(Un4seen.Bass.Bass.BASS_ChannelBytes2Seconds(ActiveStreamHandle, Un4seen.Bass.Bass.BASS_ChannelGetLength(ActiveStreamHandle, 0)));
									Un4seen.Bass.BASS_CHANNELINFO info = new Un4seen.Bass.BASS_CHANNELINFO();
									Un4seen.Bass.Bass.BASS_ChannelGetInfo(ActiveStreamHandle, info);
									sampleFrequency = info.freq;

									int syncHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(ActiveStreamHandle,
										 Un4seen.Bass.BASSSync.BASS_SYNC_END,
										 0,
										 endTrackSyncProc,
										 IntPtr.Zero);

									if (syncHandle == 0)
										throw new ArgumentException("Error establishing End Sync on file stream.", "path");

									CanPlay = true;
									RaiseOpenSucceededEvent();

									switch (pendingOperation)
									{
										case PendingOperation.None:
											break;
										case PendingOperation.Play:
											Play();
											break;
										case PendingOperation.Pause:
											Pause();
											break;
										default:
											break;
									}
								}
								else		//该文件不是正在打开的文件（即文件已过时，可能的原因是UI线程较忙，调用onlineFileWorker.Abort()时BeginInvoke的内容已提交，但还未执行）
								{
									if (!Un4seen.Bass.Bass.BASS_StreamFree(handle))
									{
										Debug.WriteLine("BASS_StreamFree失败：" + Un4seen.Bass.Bass.BASS_ErrorGetCode());
									}
									//Debug.WriteLine("已调用BASS_StreamFree()");
								}
							}
							else
							{
								Debug.WriteLine(Un4seen.Bass.Bass.BASS_ErrorGetCode());
								RaiseOpenFailedEvent();
							}
						}));
					onlineFileWorker = null;
				}));
			onlineFileWorker.IsBackground = true;
			onlineFileWorker.Start();
		}

		/// <summary>
		/// 设置代理服务器
		/// </summary>
		/// <param name="host">主机</param>
		/// <param name="port">端口</param>
		/// <param name="username">用户名</param>
		/// <param name="password">密码</param>
		public void SetProxy(string host, int port, string username = null, string password = null)
		{
			//Debug.WriteLine("已调用BassEngine.SetProxy()");
			if (proxyHandle != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(proxyHandle);
				proxyHandle = IntPtr.Zero;
			}

			//格式：user:pass@server:port
			StringBuilder sb = new StringBuilder();

			//有用户名和密码的情形
			if (!string.IsNullOrEmpty(username))
			{
				if (string.IsNullOrEmpty(password))
					throw new ArgumentException("密码为空", "password");
				sb.Append(username);
				sb.Append(":");
				sb.Append(password);
			}

			if (string.IsNullOrEmpty(host))
				throw new ArgumentException("主机为空", "host");
			sb.Append("@");

			//添加主机和端口号
			if (host.Contains(':'))
				throw new ArgumentException("主机不能包含符号:", "host");
			sb.Append("http://");
			sb.Append(host);
			sb.Append(":");
			sb.Append(port);

			string proxyString = sb.ToString();

			//将String转换为字符数组
			proxyHandle = Marshal.StringToHGlobalAnsi(proxyString);

			//设置代理服务器
			bool result = Un4seen.Bass.Bass.BASS_SetConfigPtr(Un4seen.Bass.BASSConfig.BASS_CONFIG_NET_PROXY, proxyHandle);
			if (!result)
			{
				throw new Exception("设置代理失败：" + Un4seen.Bass.Bass.BASS_ErrorGetCode());
			}
		}

		/// <summary>
		/// 使用默认代理服务器
		/// </summary>
		public void UseDefaultProxy()
		{
			//Debug.WriteLine("已调用BassEngine.UseDefaultProxy()");
			//释放代理服务器设置的非托管资源句柄
			if (proxyHandle != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(proxyHandle);
				proxyHandle = IntPtr.Zero;
			}
			
			//用长度为0的字符串来设置
			proxyHandle = Marshal.StringToHGlobalAnsi(string.Empty);
			bool result = Un4seen.Bass.Bass.BASS_SetConfigPtr(Un4seen.Bass.BASSConfig.BASS_CONFIG_NET_PROXY, proxyHandle);
			if (!result)
			{
				throw new Exception("设置代理失败：" + Un4seen.Bass.Bass.BASS_ErrorGetCode());
			}
		}

		/// <summary>
		/// 不使用任何代理服务器
		/// </summary>
		public void DontUseProxy()
		{
			//Debug.WriteLine("已调用BassEngine.DontUseProxy()");
			//释放代理服务器设置的非托管资源句柄
			if (proxyHandle != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(proxyHandle);
				proxyHandle = IntPtr.Zero;
			}
			//用空指针来设置
			bool result = Un4seen.Bass.Bass.BASS_SetConfigPtr(Un4seen.Bass.BASSConfig.BASS_CONFIG_NET_PROXY, IntPtr.Zero);
			if (!result)
			{
				//bass.dll中BASS_SetConfigPtr函数返回的其实是代理字符串指针，所以设置为NULL时会返回NULL，被Bass.Net封装后就永远返回false。
				//throw new Exception("设置代理失败：" + Un4seen.Bass.Bass.BASS_ErrorGetCode());
			}
		}
		#endregion

		#region Event Handleres
		/// <summary>
		/// 更新播放进度
		/// </summary>
		private void positionTimer_Tick(object sender, EventArgs e)
		{
			if (ActiveStreamHandle == 0)
			{
				ChannelPosition = TimeSpan.Zero;
			}
			else
			{
				inChannelTimerUpdate = true;
				ChannelPosition = TimeSpan.FromSeconds(Un4seen.Bass.Bass.BASS_ChannelBytes2Seconds(ActiveStreamHandle, Un4seen.Bass.Bass.BASS_ChannelGetPosition(ActiveStreamHandle, 0)));
				inChannelTimerUpdate = false;
			}
		}
		#endregion

		#region Private Utility Methods
		/// <summary>
		/// 查找设备的序号
		/// </summary>
		/// <param name="device">要查找的设备</param>
		/// <param name="returnDefault">当找不到设备时，是否返回默认设备的序号</param>
		/// <returns></returns>
		private static int FindDevice(DeviceInfo? device, bool returnDefault = false)
		{


			if (device.HasValue)
			{
				int deviceNO = -1;
				var devices = Un4seen.Bass.Bass.BASS_GetDeviceInfos();
				var filteredDevices = from d in devices where d.id != null && d.id == device.Value.ID select Array.IndexOf(devices, d);
				if (filteredDevices.Count() == 1)
				{
					deviceNO = filteredDevices.First();
				}
				if (deviceNO == -1)
				{
					filteredDevices = from d in devices where d.name == device.Value.Name select Array.IndexOf(devices, d);
					if (filteredDevices.Count() == 1)
					{
						deviceNO = filteredDevices.First();
					}
				}
				if (deviceNO == -1)
				{
					filteredDevices = from d in devices where d.driver == device.Value.Driver select Array.IndexOf(devices, d);
					if (filteredDevices.Count() == 1)
					{
						deviceNO = filteredDevices.First();
					}
				}
				if (deviceNO == -1 && returnDefault)
				{
					return FindDefaultDevice();
				}
				else if (deviceNO != -1)
				{
					return deviceNO;
				}
				else
				{
					throw new Exception("找不到此设备：" + device.Value.Name);
				}
			}
			else
			{
				return FindDefaultDevice();
			}
		}

		/// <summary>
		/// 返回默认设备的序号
		/// </summary>
		/// <returns></returns>
		private static int FindDefaultDevice()
		{
			var devices = Un4seen.Bass.Bass.BASS_GetDeviceInfos();
			for (int i = 0; i < devices.Length; ++i)
			{
				if (devices[i].IsDefault) return i;
			}
			throw new Exception("没有默认设备");
		}

		/// <summary>
		/// 初始化BassEngine
		/// </summary>
		private void Initialize(DeviceInfo? device = null)
		{
			positionTimer.Interval = TimeSpan.FromMilliseconds(50);
			positionTimer.Tick += positionTimer_Tick;

			IsPlaying = false;

			IntPtr handle = IntPtr.Zero;
			if (Application.Current.MainWindow != null)
			{
				handle = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
			}

			int deviceNO = FindDevice(device, true);
			if (!Un4seen.Bass.Bass.BASS_Init(deviceNO, sampleFrequency, Un4seen.Bass.BASSInit.BASS_DEVICE_DEFAULT, handle))
			{
				var error = Un4seen.Bass.Bass.BASS_ErrorGetCode();
				int count = Un4seen.Bass.Bass.BASS_GetDeviceCount();
				for (deviceNO = -1; deviceNO < count; ++deviceNO)
				{
					if (deviceNO != 0 && Un4seen.Bass.Bass.BASS_Init(deviceNO, sampleFrequency, Un4seen.Bass.BASSInit.BASS_DEVICE_DEFAULT, handle))
					{
						break;
					}
				}
				if (deviceNO == count)
				{
					throw new BassInitializationFailureException(error);
				}
			}

			if (device == null && deviceNO == FindDefaultDevice())
			{
				Device = null;
			}
			else
			{
				var info = Un4seen.Bass.Bass.BASS_GetDeviceInfo(Un4seen.Bass.Bass.BASS_GetDevice());
				Device = new DeviceInfo { Driver = info.driver, Name = info.name, ID = info.id };
			}

			Un4seen.Bass.Bass.BASS_SetConfig(Un4seen.Bass.BASSConfig.BASS_CONFIG_NET_TIMEOUT, 15000);
		}

		/// <summary>
		/// 播放当前流
		/// </summary>
		private void PlayCurrentStream()
		{
			// Play Stream
			if (ActiveStreamHandle != 0 && Un4seen.Bass.Bass.BASS_ChannelPlay(ActiveStreamHandle, false))
			{
				Un4seen.Bass.BASS_CHANNELINFO info = new Un4seen.Bass.BASS_CHANNELINFO();
				Un4seen.Bass.Bass.BASS_ChannelGetInfo(ActiveStreamHandle, info);
			}
#if DEBUG
			else
			{

				Debug.WriteLine("Error={0}", Un4seen.Bass.Bass.BASS_ErrorGetCode());

			}
#endif
		}
		/// <summary>
		/// 释放当前流
		/// </summary>
		private void FreeCurrentStream()
		{
			if (onlineFileWorker != null)
			{
				onlineFileWorker.Abort();
				onlineFileWorker = null;
			}

			if (ActiveStreamHandle != 0)
			{
				if (!Un4seen.Bass.Bass.BASS_StreamFree(ActiveStreamHandle))
				{
					Debug.WriteLine("BASS_StreamFree失败：" + Un4seen.Bass.Bass.BASS_ErrorGetCode());
				}
				//Debug.WriteLine("已调用BASS_StreamFree()");
				ActiveStreamHandle = 0;
			}
		}
		/// <summary>
		/// 设置音量
		/// </summary>
		private void SetVolume()
		{
			if (ActiveStreamHandle != 0)
			{
				float realVolume = IsMuted ? 0 : (float)Volume;
				Un4seen.Bass.Bass.BASS_ChannelSetAttribute(ActiveStreamHandle, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_VOL, realVolume);
			}
		}
		#endregion

		#region Callbacks
		/// <summary>
		/// 播放完毕
		/// </summary>
		private void EndTrack(int handle, int channel, int data, IntPtr user)
		{
			Application.Current.Dispatcher.BeginInvoke(new Action(() =>
				{
					Stop();
					RaiseTrackEndedEvent();
				}));
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// 长度
		/// </summary>
		public TimeSpan ChannelLength
		{
			get { return channelLength; }
			protected set
			{
				TimeSpan oldValue = channelLength;
				channelLength = value;
				if (oldValue != channelLength)
					NotifyPropertyChanged("ChannelLength");
			}
		}

		/// <summary>
		/// 位置
		/// </summary>
		public TimeSpan ChannelPosition
		{
			get { return currentChannelPosition; }
			set
			{
				if (!inChannelSet)
				{
					inChannelSet = true; // Avoid recursion
					TimeSpan oldValue = currentChannelPosition;
					TimeSpan position = value;
					if (position > ChannelLength) position = ChannelLength;
					if (position < TimeSpan.Zero) position = TimeSpan.Zero;
					if (!inChannelTimerUpdate)
						Un4seen.Bass.Bass.BASS_ChannelSetPosition(ActiveStreamHandle, Un4seen.Bass.Bass.BASS_ChannelSeconds2Bytes(ActiveStreamHandle, position.TotalSeconds));
					currentChannelPosition = position;
					if (oldValue != currentChannelPosition)
						NotifyPropertyChanged("ChannelPosition");
					inChannelSet = false;
				}
			}
		}

		/// <summary>
		/// 当前流的句柄
		/// </summary>
		public int ActiveStreamHandle
		{
			get { return activeStreamHandle; }
			protected set
			{
				int oldValue = activeStreamHandle;
				activeStreamHandle = value;
				if (oldValue != activeStreamHandle)
				{
					if (activeStreamHandle != 0)
					{
						SetVolume();
					}
					NotifyPropertyChanged("ActiveStreamHandle");
				}
			}
		}

		/// <summary>
		/// 可以使用播放命令
		/// </summary>
		public bool CanPlay
		{
			get { return canPlay; }
			protected set
			{
				bool oldValue = canPlay;
				canPlay = value;
				if (oldValue != canPlay)
					NotifyPropertyChanged("CanPlay");
			}
		}

		/// <summary>
		/// 可以使用暂停命令
		/// </summary>
		public bool CanPause
		{
			get { return canPause; }
			protected set
			{
				bool oldValue = canPause;
				canPause = value;
				if (oldValue != canPause)
					NotifyPropertyChanged("CanPause");
			}
		}

		/// <summary>
		/// 可以使用停止命令
		/// </summary>
		public bool CanStop
		{
			get { return canStop; }
			protected set
			{
				bool oldValue = canStop;
				canStop = value;
				if (oldValue != canStop)
					NotifyPropertyChanged("CanStop");
			}
		}

		/// <summary>
		/// 是否正在播放
		/// </summary>
		public bool IsPlaying
		{
			get { return isPlaying; }
			protected set
			{
				bool oldValue = isPlaying;
				isPlaying = value;
				if (oldValue != isPlaying)
					NotifyPropertyChanged("IsPlaying");
				positionTimer.IsEnabled = value;
			}
		}

		/// <summary>
		/// 音量
		/// </summary>
		public double Volume
		{
			get { return volume; }
			set
			{
				value = Math.Max(0, Math.Min(1, value));
				if (volume != value)
				{
					volume = value;
					SetVolume();
					NotifyPropertyChanged("Volume");
				}
			}
		}

		/// <summary>
		/// 是否静音
		/// </summary>
		public bool IsMuted
		{
			get { return isMuted; }
			set
			{
				if (isMuted != value)
				{
					isMuted = value;
					SetVolume();
					NotifyPropertyChanged("IsMuted");
				}
			}
		}

		/// <summary>
		/// 设备（空代表默认设备）
		/// </summary>
		public DeviceInfo? Device { get; private set; }
		#endregion

		#region Events
		/// <summary>
		/// 当播放完毕时发生。
		/// </summary>
		public event EventHandler TrackEnded;

		/// <summary>
		/// 引发播放完毕事件
		/// </summary>
		void RaiseTrackEndedEvent()
		{
			if (TrackEnded != null)
				TrackEnded(this, EventArgs.Empty);
		}

		/// <summary>
		/// 当打开音频文件失败时发生。
		/// </summary>
		public event EventHandler OpenFailed;

		/// <summary>
		/// 引发打开音频文件失败事件
		/// </summary>
		void RaiseOpenFailedEvent()
		{
			if (OpenFailed != null)
				OpenFailed(this, EventArgs.Empty);
		}

		/// <summary>
		/// 当打开音频文件成功时发生。
		/// </summary>
		public event EventHandler OpenSucceeded;

		/// <summary>
		/// 引发打开音频文件成功事件
		/// </summary>
		void RaiseOpenSucceededEvent()
		{
			if (OpenSucceeded != null)
				OpenSucceeded(this, EventArgs.Empty);
		}
		#endregion

		#region ISpectrumPlayer
		public bool GetFFTData(float[] fftDataBuffer)
		{
			return (Un4seen.Bass.Bass.BASS_ChannelGetData(ActiveStreamHandle, fftDataBuffer, maxFFT)) > 0;
		}

		public int GetFFTFrequencyIndex(int frequency)
		{
			return Un4seen.Bass.Utils.FFTFrequency2Index(frequency, 4096, sampleFrequency);
		}
		#endregion
	}
}