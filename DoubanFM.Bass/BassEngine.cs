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
		private static BassEngine instance;
		private readonly DispatcherTimer positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
		private readonly int maxFFT = (int)(Un4seen.Bass.BASSData.BASS_DATA_AVAILABLE | Un4seen.Bass.BASSData.BASS_DATA_FFT4096);
		private readonly Un4seen.Bass.SYNCPROC endTrackSyncProc;
		private int sampleFrequency = 44100;
		private int activeStreamHandle;
		private bool canPlay;
		private bool canPause;
		private bool isPlaying;
		private bool canStop;
		private TimeSpan channelLength = TimeSpan.Zero;
		private TimeSpan currentChannelPosition = TimeSpan.Zero;
		private bool inChannelSet;
		private bool inChannelTimerUpdate;
		private Thread onlineFileWorker;
		enum PendingOperation { None = 0, Play, Pause };
		private PendingOperation pendingOperation = PendingOperation.None;
		private double volume;
		private bool isMuted;
		private IntPtr proxyHandle = IntPtr.Zero;
		private string openningFile = null;
		#endregion

		#region Constructor
		static BassEngine()
		{
			Un4seen.Bass.BassNet.Registration("yk000123@sina.com", "2X34201017282922");

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
		}

		private BassEngine()
		{
			Initialize();
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
		#endregion

		#region Public Methods
		/// <summary>
		/// 停止
		/// </summary>
		public void Stop()
		{
			Debug.WriteLine("已调用BassEngine.Stop()");

			if (canStop)
			{
				ChannelPosition = TimeSpan.Zero;
				if (ActiveStreamHandle != 0)
				{
					Un4seen.Bass.Bass.BASS_ChannelStop(ActiveStreamHandle);
					Un4seen.Bass.Bass.BASS_ChannelSetPosition(ActiveStreamHandle, ChannelPosition.TotalSeconds);
					Debug.WriteLine("已调用BASS_ChannelStop()");
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
		/// 暂停
		/// </summary>
		public void Pause()
		{
			Debug.WriteLine("已调用BassEngine.Pause()");
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
		/// 播放
		/// </summary>
		public void Play()
		{
			Debug.WriteLine("已调用BassEngine.Play()");
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
			Debug.WriteLine("已调用BassEngine.OpenFile()");
			Stop();
			pendingOperation = PendingOperation.None;

			int handle = Un4seen.Bass.Bass.BASS_StreamCreateFile(filename, 0, 0, Un4seen.Bass.BASSFlag.BASS_SAMPLE_FLOAT | Un4seen.Bass.BASSFlag.BASS_STREAM_PRESCAN);

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
			Debug.WriteLine("已调用BassEngine.OpenUrlAsync()");

			Stop();
			pendingOperation = PendingOperation.None;

			onlineFileWorker = new Thread(new ThreadStart(() =>
				{
					int handle = Un4seen.Bass.Bass.BASS_StreamCreateURL(url, 0, Un4seen.Bass.BASSFlag.BASS_SAMPLE_FLOAT | Un4seen.Bass.BASSFlag.BASS_STREAM_PRESCAN, null, IntPtr.Zero);

					DateTime start = DateTime.Now;
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
									Debug.WriteLine("已调用BASS_StreamFree()");
								}
							}
							else
							{
								Debug.WriteLine(Un4seen.Bass.Bass.BASS_ErrorGetCode());
								RaiseOpenFailedEvent();
							}
							TimeSpan period = DateTime.Now - start;
							Debug.WriteLine("测时结果：" + period);
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
		public void SetProxy(string host, int? port, string username, string password)
		{
			Debug.WriteLine("已调用BassEngine.SetProxy()");
			if (proxyHandle != IntPtr.Zero)
				Marshal.FreeHGlobal(proxyHandle);
			proxyHandle = IntPtr.Zero;

			//user:pass@server:port
			StringBuilder sb = new StringBuilder();
			if (!string.IsNullOrEmpty(username))
			{
				if (string.IsNullOrEmpty(password))
					throw new ArgumentException("密码为空", "password");
				sb.Append(username);
				sb.Append(":");
				sb.Append(password);
			}
			if (sb.Length == 0)
			{
				if (string.IsNullOrEmpty(host))
					throw new ArgumentException("主机为空", "host");
				if (!port.HasValue)
					throw new ArgumentException("端口号为空", "port");
			}
			sb.Append("@");
			if (!string.IsNullOrEmpty(host) && port.HasValue)
			{
				if (host.Contains(':'))
					throw new ArgumentException("主机不能包含符号:");
				sb.Append("http://");
				sb.Append(host);
				sb.Append(":");
				sb.Append(port);
			}
			string proxyString = sb.ToString();

			// set it
			proxyHandle = Marshal.StringToHGlobalUni(proxyString);
			bool result = Un4seen.Bass.Bass.BASS_SetConfigPtr(Un4seen.Bass.BASSConfig.BASS_CONFIG_NET_PROXY, proxyHandle);

			if (!result)
			{
				throw new Exception("设置BassEngine的代理失败：" + Un4seen.Bass.Bass.BASS_ErrorGetCode());
			}
		}

		/// <summary>
		/// 使用默认代理服务器设置
		/// </summary>
		public void UseDefaultProxy()
		{
			Debug.WriteLine("已调用BassEngine.UseDefaultProxy()");
			if (proxyHandle != IntPtr.Zero)
				Marshal.FreeHGlobal(proxyHandle);
			proxyHandle = IntPtr.Zero;
			
			proxyHandle = Marshal.StringToHGlobalAnsi(string.Empty);
			bool result = Un4seen.Bass.Bass.BASS_SetConfigPtr(Un4seen.Bass.BASSConfig.BASS_CONFIG_NET_PROXY, proxyHandle);

			if (!result)
			{
				throw new Exception("设置BassEngine的代理失败：" + Un4seen.Bass.Bass.BASS_ErrorGetCode());
			}
		}
		#endregion

		#region Event Handleres
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
		/// 初始化
		/// </summary>
		private void Initialize()
		{
			positionTimer.Interval = TimeSpan.FromMilliseconds(50);
			positionTimer.Tick += positionTimer_Tick;

			IsPlaying = false;

			Window mainWindow = Application.Current.MainWindow;
			WindowInteropHelper interopHelper = new WindowInteropHelper(mainWindow);

			if (Un4seen.Bass.Bass.BASS_Init(-1, 44100, Un4seen.Bass.BASSInit.BASS_DEVICE_DEFAULT, interopHelper.Handle))
			{
#if DEBUG
				Un4seen.Bass.BASS_INFO info = new Un4seen.Bass.BASS_INFO();
				Un4seen.Bass.Bass.BASS_GetInfo(info);
				Debug.WriteLine(info.ToString());
#endif
			}
			else
			{
				throw new Exception("Bass initialization error : " + Un4seen.Bass.Bass.BASS_ErrorGetCode());
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
				Debug.WriteLine("已调用BASS_StreamFree()");
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
		#endregion

		#region Events
		public event EventHandler TrackEnded;

		void RaiseTrackEndedEvent()
		{
			if (TrackEnded != null)
				TrackEnded(this, EventArgs.Empty);
		}

		public event EventHandler OpenFailed;

		void RaiseOpenFailedEvent()
		{
			if (OpenFailed != null)
				OpenFailed(this, EventArgs.Empty);
		}

		public event EventHandler OpenSucceeded;

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