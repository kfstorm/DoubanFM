/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows;
using System.Diagnostics;

namespace DoubanFM.Core
{
	/// <summary>
	/// 播放器核心
	/// </summary>
	public class Player : DependencyObject, IDisposable
	{
		#region 依赖项属性

		public static readonly DependencyProperty IsInitializedProperty = DependencyProperty.Register("IsInitialized", typeof(bool), typeof(Player));
		public static readonly DependencyProperty CurrentChannelProperty = DependencyProperty.Register("CurrentChannel", typeof(Channel), typeof(Player));
		public static readonly DependencyProperty CurrentSongProperty = DependencyProperty.Register("CurrentSong", typeof(Song), typeof(Player), new PropertyMetadata(new PropertyChangedCallback((d, e) =>
	{
		(d as Player).RaiseCurrentSongChangedEvent();
	})));
		public static readonly DependencyProperty IsLikedProperty = DependencyProperty.Register("IsLiked", typeof(bool), typeof(Player),
			new PropertyMetadata(new PropertyChangedCallback((o, e) =>
			{
				Player player = (Player)o;
				if (player.CurrentSong == null) return;
				player.RaiseIsLikedChangedEvent();
				if ((bool)e.NewValue == player.CurrentSong.Like) return;
				if (player.CurrentSong.Like == false) player.Like();
				else player.Unlike();
			})));
		public static readonly DependencyProperty IsLikedEnabledProperty = DependencyProperty.Register("IsLikedEnabled", typeof(bool), typeof(Player), new PropertyMetadata(true));
		public static readonly DependencyProperty IsNeverEnabledProperty = DependencyProperty.Register("IsNeverEnabled", typeof(bool), typeof(Player));
		public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register("IsPlaying", typeof(bool), typeof(Player),
			new PropertyMetadata(true, new PropertyChangedCallback((o, e) =>
			{
				Player player = (Player)o;
				if ((bool)e.NewValue == true) player.Play();
				else player.Pause();
				player.RaiseIsPlayingChangedEvent();
			})));
		public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register("Settings", typeof(Settings), typeof(Player));

		#endregion

		#region 属性

		/// <summary>
		/// 频道列表
		/// </summary>
		public ChannelInfo ChannelInfo { get; private set; }
		/// <summary>
		/// 当前频道
		/// </summary>
		public Channel CurrentChannel
		{
			get { return (Channel)GetValue(CurrentChannelProperty); }
			set
			{
				if (CurrentChannel != value)
				{
					if (!IsInitialized) return;
					if (value == null)
						throw new Exception("频道不能设为空");
					if (value.IsPersonal && !value.IsSpecial && !UserAssistant.IsLoggedOn) return;	//没登录时不能使用私人频道
					if (!value.IsSpecial && !ChannelInfo.Personal.Contains(value) && !ChannelInfo.Public.Contains(value) && !ChannelInfo.Dj.Contains(value)) return;		//除特殊频道外，无法播放频道列表中不存在的频道
					RaiseStopedEvent();
					Channel lastChannel = CurrentChannel;
					CurrentSong = null;
					SetValue(CurrentChannelProperty, value);
					Settings.LastChannel = CurrentChannel;
					NewPlayList();
					RaiseCurrentChannelChangedEvent();
				}
			}
		}
		/// <summary>
		/// 当前歌曲
		/// </summary>
		public Song CurrentSong
		{
			get { return (Song)GetValue(CurrentSongProperty); }
			protected set { SetValue(CurrentSongProperty, value); }
		}
		/// <summary>
		/// 设置
		/// </summary>
		public Settings Settings
		{
			get { return (Settings)GetValue(SettingsProperty); }
			set { SetValue(SettingsProperty, value); }
		}
		/// <summary>
		/// 是否已初始化
		/// </summary>
		public bool IsInitialized
		{
			get { return (bool)GetValue(IsInitializedProperty); }
			private set
			{
				if (value == false) return;
				if (IsInitialized == false)
				{
					SetValue(IsInitializedProperty, true);
					RaiseInitializedEvent();
				}
			}
		}
		/// <summary>
		/// 是否正在播放
		/// </summary>
		public bool IsPlaying
		{
			get { return (bool)GetValue(IsPlayingProperty); }
			set { SetValue(IsPlayingProperty, value); }
		}
		/// <summary>
		/// 是否喜欢这首歌
		/// </summary>
		public bool IsLiked
		{
			get { return (bool)GetValue(IsLikedProperty); }
			set
			{
				if (!IsLikedEnabled) return;
				SetValue(IsLikedProperty, value);
			}
		}
		/// <summary>
		/// 获取一个值，该值指示红心是否启用
		/// </summary>
		public bool IsLikedEnabled
		{
			get { return (bool)GetValue(IsLikedEnabledProperty); }
			private set
			{
				if (IsLikedEnabled != value)
				{
					SetValue(IsLikedEnabledProperty, value);
					RaiseIsLikedEnabledChangedEvent();
				}
			}
		}
		/// <summary>
		/// 获取一个值，该值指示垃圾桶是否启用
		/// </summary>
		public bool IsNeverEnabled
		{
			get { return (bool)GetValue(IsNeverEnabledProperty); }
			private set
			{
				if (IsNeverEnabled != value)
				{
					SetValue(IsNeverEnabledProperty, value);
					RaiseIsNeverEnabledChangedEvent();
				}
			}
		}
		/// <summary>
		/// 处理登录、注销等
		/// </summary>
		public UserAssistant UserAssistant { get; private set; }
		/// <summary>
		/// 频道搜索
		/// </summary>
		public ChannelSearch ChannelSearch { get; private set; }
		
		#endregion

		#region 成员变量

		/// <summary>
		/// 待播放音乐的列表
		/// </summary>
		private Queue<Song> _playListSongs = new Queue<Song>();
		/// <summary>
		/// 上次暂停的时间
		/// </summary>
		private DateTime _pauseTime = DateTime.Now;
		/// <summary>
		/// 防止同时进行多个任务的锁
		/// </summary>
		private object workLock = new object();
		/// <summary>
		/// 是否正有一个“下一首”任务正在执行
		/// </summary>
		private bool _skipping;
		/// <summary>
		/// 是否正有一个“不再播放”任务正在执行
		/// </summary>
		private bool _neverring;

		#endregion

		#region 事件

		/// <summary>
		/// 当当前频道改变时发生。
		/// </summary>
		public event EventHandler CurrentChannelChanged;
		private void RaiseCurrentChannelChangedEvent()
		{
			if (CurrentChannelChanged != null)
				CurrentChannelChanged(this, EventArgs.Empty);
		}
		/// <summary>
		/// 当当前歌曲改变时发生。
		/// </summary>
		public event EventHandler CurrentSongChanged;
		private void RaiseCurrentSongChangedEvent()
		{
			if (CurrentSongChanged != null)
				CurrentSongChanged(this, EventArgs.Empty);
		}
		/// <summary>
		/// 当初始化完成时发生。
		/// </summary>
		public event EventHandler Initialized;
		private void RaiseInitializedEvent()
		{
			if (Initialized != null)
				Initialized(this, EventArgs.Empty);
		}
		/// <summary>
		/// 当音乐继续时发生。
		/// </summary>
		public event EventHandler Played;
		private void RaisePlayedEvent()
		{
			if (Played != null)
				Played(this, EventArgs.Empty);
		}
		/// <summary>
		/// 当音乐暂停时发生。
		/// </summary>
		public event EventHandler Paused;
		private void RaisePausedEvent()
		{
			if (Paused != null)
				Paused(this, EventArgs.Empty);
		}
		/// <summary>
		/// 当音乐停止时发生。
		/// </summary>
		public event EventHandler Stoped;
		private void RaiseStopedEvent()
		{
			if (Stoped != null)
				Stoped(this, EventArgs.Empty);
		}
		/// <summary>
		/// 当IsLiked改变时发生。
		/// </summary>
		public event EventHandler IsLikedChanged;
		private void RaiseIsLikedChangedEvent()
		{
			if (IsLikedChanged != null)
				IsLikedChanged(this, EventArgs.Empty);
		}
		/// <summary>
		/// 当IsLikedEnabled改变时发生。
		/// </summary>
		public event EventHandler IsLikedEnabledChanged;
		private void RaiseIsLikedEnabledChangedEvent()
		{
			if (IsLikedEnabledChanged != null)
				IsLikedEnabledChanged(this, EventArgs.Empty);
		}
		/// <summary>
		/// 当IsNeverEnabled改变时发生。
		/// </summary>
		public event EventHandler IsNeverEnabledChanged;
		private void RaiseIsNeverEnabledChangedEvent()
		{
			if (IsNeverEnabledChanged != null)
				IsNeverEnabledChanged(this, EventArgs.Empty);
		}
		/// <summary>
		/// 当IsPlaying改变时发生。
		/// </summary>
		public event EventHandler IsPlayingChanged;
		private void RaiseIsPlayingChangedEvent()
		{
			if (IsPlayingChanged != null)
				IsPlayingChanged(this, EventArgs.Empty);
		}
		/// <summary>
		/// 当获取播放列表失败时发生。
		/// </summary>
		public event EventHandler<PlayList.PlayListEventArgs> GetPlayListFailed;
		private void RaiseGetPlayListFailedEvent(PlayList.PlayListEventArgs e)
		{
			if (GetPlayListFailed != null)
				GetPlayListFailed(this, e);
		}
		/// <summary>
		/// 当一首歌自然播放完成时向服务器发送添加播放记录的消息失败时发生。
		/// </summary>
		public event EventHandler<ErrorEventArgs> FinishedPlayingReportFailed;
		private void RaiseFinishedPlayingReportFailedEvent(ErrorEventArgs e)
		{
			if (FinishedPlayingReportFailed != null)
			{
				FinishedPlayingReportFailed(this, e);
			}
		}

		#endregion

		#region 构造及初始化

		public Player()
			: base()
		{
			LoadSettings();
			UserAssistant = new UserAssistant();
			UserAssistant.Settings = Settings;
			ChannelSearch = new ChannelSearch(Settings);
			DownloadSearch.Settings = Settings;

			//频道改变时更新红心和垃圾桶的启用状态
			CurrentChannelChanged += new EventHandler((o, e) =>
			{
				IsLikedEnabled = true;
				IsNeverEnabled = UserAssistant.IsLoggedOn;
			});

			//歌曲改变时更新红心状态
			CurrentSongChanged += new EventHandler((o, e) =>
			{
				if (CurrentSong != null)
				{
					IsLiked = CurrentSong.Like;
				}
			});

			//登录成功后更新垃圾桶启用状态
			UserAssistant.LogOnSucceed += new EventHandler((o, e) =>
			{
				IsNeverEnabled = CurrentChannel.IsPersonal;
			});

			//注销成功后更新垃圾桶启用状态；如果正在播放私人兆赫，则更换为公共兆赫
			UserAssistant.LogOffSucceed += new EventHandler((o, e) =>
			{
				IsNeverEnabled = false;
				if (CurrentChannel.IsPersonal && !CurrentChannel.IsSpecial)
					CurrentChannel = ChannelInfo.Public.First();
			});

			//获取播放列表失败后引发事件
			PlayList.GetPlayListFailed += new EventHandler<PlayList.PlayListEventArgs>((o, e) =>
			{
				Dispatcher.Invoke(new Action(() => { RaiseGetPlayListFailedEvent(e); }));
			});
		}
		/// <summary>
		/// 播放器初始化
		/// </summary>
		public void Initialize()
		{
			if (IsInitialized) return;
			Debug.WriteLine(DateTime.Now + " 播放器核心初始化中");
			bool lastTimeLoggedOn = Settings.LastTimeLoggedOn;
			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
				{
					ChannelInfo channelInfo = null;
					string file = null;
					//如果用户上次退出软件时处于未登录状态，则启动时不更新登录状态
					if (lastTimeLoggedOn)
					{
						while (true)
						{
							Debug.WriteLine(DateTime.Now + " 刷新豆瓣FM主页……");
							file = new ConnectionBase().Get("http://douban.fm/settings/profile");
							Debug.WriteLine(DateTime.Now + " 刷新完成");
							if (!string.IsNullOrEmpty(file))
							{
								break;
							}
							TakeABreak();
						}

						//更新用户的登录状态
						UserAssistant.Update(file);
					}

					while (true)
					{
						Debug.WriteLine(DateTime.Now + " 获取频道列表……");
						file = new ConnectionBase().Get("http://doubanfmcloud-channelinfo.stor.sinaapp.com/channelinfo");
						Debug.WriteLine(DateTime.Now + " 获取频道列表完成");
						if (string.IsNullOrEmpty(file))
						{
							TakeABreak();
							continue;
						}
						channelInfo = new ChannelInfo(Json.ChannelInfo.FromJson(file));
						if (channelInfo == null || !channelInfo.IsEffective)
						{
							Debug.WriteLine(DateTime.Now + " 获取频道列表失败");
							TakeABreak();
						}
						else
						{
							Debug.WriteLine(DateTime.Now + " 获取频道列表成功");
							break;
						}
					}
					
					Dispatcher.Invoke(new Action(() =>
						{
							/**
							当上次退出时是登录状态，但之后在浏览器里注销后，再打开软件会显示未登录，
							但Cookie还在，如果不清除Cookie，第一次登录会失败，清除后第一次登录也能成功
							 * */
							if (UserAssistant.CurrentState != Core.UserAssistant.State.LoggedOn)
								UserAssistant.LogOff();
							ChannelInfo = channelInfo;
							IsInitialized = true;
							Debug.WriteLine(DateTime.Now + " 播放器核心初始化完成");
							//选择一个频道
							ChooseChannelAtStartup();
						}));
				}));
		}

		/// <summary>
		/// 刚启动时选择一个频道
		/// </summary>
		private void ChooseChannelAtStartup()
		{
			if (Settings.LastChannel != null)
			{
				bool selected = false;
				Channel firstChannel = Settings.LastChannel;
				if (firstChannel.IsPersonal && UserAssistant.IsLoggedOn ||firstChannel.IsSpecial)
					selected = true;
				else if (firstChannel.IsDj)
				{
					foreach (Channel channel in ChannelInfo.Dj)
						if (channel == firstChannel)
						{
							selected = true;
						}
				}
				else
				{
					foreach (Channel channel in ChannelInfo.Public)
						if (channel == firstChannel)
						{
							selected = true;
						}
				}
				if (selected)
					CurrentChannel = firstChannel;
			}
			if (CurrentChannel == null)
				CurrentChannel = ChannelInfo.Public.First();
		}

		#endregion

		#region 播放器控制

		/// <summary>
		/// 歌曲自然播放完毕，添加播放记录或请求新播放列表。
		/// type=e
		/// Played
		/// </summary>
		public void CurrentSongFinishedPlaying()
		{
			RaiseStopedEvent();
			if (UserAssistant.IsLoggedOn)
			{
				++Settings.User.Played;
			}
			ChangeToNextSong("e");
		}
		/// <summary>
		/// 媒体加载失败时请调用此方法
		/// 此方法与Skip()方法唯一的不同是不会触发Stoped事件
		/// 如果媒体加载失败时调用Skip()，会触发Stoped事件，然后会执行MediaElement的Stop()方法，然后再次失败，于是Skip方法会循环调用
		/// </summary>
		public void MediaFailed()
		{
			if (CurrentSong == null) return;
			if (_skipping) return;
			ChangeToNextSong("s");
		}
		/// <summary>
		/// 跳过当前歌曲
		/// type=s
		/// Skip
		/// </summary>
		public void Skip()
		{
			if (CurrentSong == null) return;
			if (_skipping) return;
			RaiseStopedEvent();
			ChangeToNextSong("s");
		}
		/// <summary>
		/// 喜欢这首歌
		/// </summary>
		void Like()
		{
			if (CurrentSong == null) return;
			if (CurrentSong.Like) return;
			CurrentSong.Like = true;
			if (UserAssistant.IsLoggedOn)
			{
				++Settings.User.Liked;
			}
			Report("r", false);
		}
		/// <summary>
		/// 不喜欢这首歌
		/// </summary>
		void Unlike()
		{
			if (CurrentSong == null) return;
			if (!CurrentSong.Like) return;
			CurrentSong.Like = false;
			if (UserAssistant.IsLoggedOn)
			{
				--Settings.User.Liked;
			}
			Report("u", false);
		}
		/// <summary>
		/// 对当前音乐标记不再播放
		/// type=b
		/// Ban
		/// </summary>
		public void Never()
		{
			if (CurrentSong == null) return;
			if (_neverring) return;
			RaiseStopedEvent();
			if (UserAssistant.IsLoggedOn)
			{
				++Settings.User.Banned;
			}

			ChangeToNextSong("b");
		}
		/// <summary>
		/// 暂停
		/// </summary>
		void Pause()
		{
			if (CurrentSong == null) return;
			IsPlaying = false;
			_pauseTime = DateTime.Now;
			RaisePausedEvent();
		}
		/// <summary>
		/// 播放。若暂停时长超过半个小时，则播放一个新的播放列表
		/// </summary>
		void Play()
		{
			if (CurrentSong == null) return;
			IsPlaying = true;
			RaisePlayedEvent();
			/* 由于豆瓣电台的音乐地址都是临时的，所以超过一定时间后按道理应该立即重新获取一个全新的播放列表，
			 * 但考虑到播放体验的流畅性，这里仍然播放当前的音乐，但是_playListSongs要更换，
			 * */
			if ((DateTime.Now - _pauseTime).TotalMinutes > 30)
			{
				Report("n", false);
			}
		}

		#endregion

		#region 成员方法

		/// <summary>
		/// 获取下一首音乐的URL
		/// </summary>
		/// <returns></returns>
		public string GetNextSongUrl()
		{
			lock (_playListSongs)
			{
				if (_playListSongs.Count > 0)
					return _playListSongs.First().FileUrl;
				else return null;
			}
		}
		/// <summary>
		/// 读取偏好设置
		/// </summary>
		void LoadSettings()
		{
			Settings = Core.Settings.Load();
		}
		/// <summary>
		/// 保存偏好设置
		/// </summary>
		/// <param name="includeCookie">是否包括Cookie</param>
		public void SaveSettings(bool includeCookie = false)
		{
			Settings.Save();
			if (includeCookie)
				SaveCookies();
		}
		/// <summary>
		/// 保存Cookies
		/// </summary>
		/// <returns>成功与否</returns>
		static bool SaveCookies()
		{
			return ConnectionBase.SaveCookies();
		}

		private bool disposed;
		/// <summary>
		/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
		/// </summary>
		/// <param name="saveSettings">是否保存设置</param>
		public void Dispose(bool saveSettings)
		{
			if (disposed) return;
			if (saveSettings)
			{
				SaveSettings();
				if (UserAssistant.IsLoggedOn && !Settings.AutoLogOnNextTime)
					UserAssistant.LogOff();
				SaveCookies();
			}
			disposed = true;
		}
		public void Dispose()
		{
			Dispose(true);
		}
		/// <summary>
		/// 获取全新的播放列表
		/// type=n
		/// New
		/// </summary>
		void NewPlayList()          //根据douban.fm的网络传输观察，只有全新的播放列表才利用context信息，其他播放列表请求都不传输context
		{
			if (!IsInitialized) return;
			ChangeToNextSong("n");
		}

		void Report(string type, bool changeCurrentSong = true)
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
			{
				lock (workLock)
				{
					if (type == "s")
					{
						_skipping = true;
					}
					if (type == "b")
					{
						_neverring = true;
					}
					PlayList pl = null;
					PlayerState ps = GetPlayerState();
					while (true)
					{
						pl = PlayList.GetPlayList(ps.CurrentSong, ps.CurrentChannel, type);
						if ((type == "p" || type == "n") && pl.Count == 0) TakeABreak();
						else if (type == "e") break;
						else if (ps.CurrentChannel.IsDj) break;
						else if (pl.Count == 0) TakeABreak();
						else break;
					}
					PlayerState ps2 = GetPlayerState();
					if (ps.CurrentChannel == ps2.CurrentChannel)
					{
						if (pl.Count > 0)
						{
							ChangePlayListSongs(pl);
						}
						if (changeCurrentSong)
						{
							ChangeCurrentSong();
						}
					}
					if (type == "s")
					{
						_skipping = false;
					}
					if (type == "b")
					{
						_neverring = false;
					}
				}
			}));
		}

		void ChangeToNextSong(string type)
		{
			Report(type);
		}

		/// <summary>
		/// 更换播放列表队列
		/// </summary>
		/// <param name="pl">新播放列表</param>
		void ChangePlayListSongs(PlayList pl)
		{
			lock (_playListSongs)
			{
				_playListSongs.Clear();
				foreach (var song in pl)
					_playListSongs.Enqueue(song);
			}
		}
		/// <summary>
		/// 更换当前播放的音乐
		/// </summary>
		void ChangeCurrentSong()
		{
			PlayList pl = null;
			PlayerState ps = GetPlayerState();
			while (_playListSongs.Count == 0)
			{
				pl = PlayList.GetPlayList(ps.CurrentSong, ps.CurrentChannel, "p");
				if (pl.Count == 0)
					TakeABreak();
				else
				{
					ChangePlayListSongs(pl);
					break;
				}
			}
			lock (_playListSongs)
				Dispatcher.Invoke(new Action(() => { CurrentSong = _playListSongs.Dequeue(); }));
		}
		/// <summary>
		/// 网络发送间歇
		/// </summary>
		static void TakeABreak()
		{
			Thread.Sleep(5000);
		}
		/// <summary>
		/// 获取当前状态，用于向线程池加入任务时传递参数
		/// </summary>
		/// <returns></returns>
		PlayerState GetPlayerState()
		{
			PlayerState ps = null;
			if (CheckAccess())
				ps = new PlayerState(CurrentChannel == null ? null : (Channel)CurrentChannel.Clone(), CurrentSong == null ? null : (Song)CurrentSong.Clone());
			else
				Dispatcher.Invoke(new Action(() => { ps = GetPlayerState(); }));
			return ps;
		}
		/// <summary>
		/// 是否可以加入收藏
		/// </summary>
		/// <param name="channel">频道</param>
		public bool CanAddToFavorites(Channel channel)
		{
			if (!IsInitialized || channel == null) return false;
			if (channel.IsPersonal && !channel.IsSpecial)
				return false;
			return !Settings.FavoriteChannels.Contains(channel);
		}
		/// <summary>
		/// 是否可以取消收藏
		/// </summary>
		/// <param name="channel">频道</param>
		public bool CanRemoveFromFavorites(Channel channel)
		{
			if (!IsInitialized || channel == null) return false;
			if (channel.IsPersonal && !channel.IsSpecial)
				return false;
			return Settings.FavoriteChannels.Contains(channel);
		}
		/// <summary>
		/// 将频道加入收藏
		/// </summary>
		/// <param name="channel">频道</param>
		/// <returns>是否成功</returns>
		public bool AddToFavorites(Channel channel)
		{
			if (!CanAddToFavorites(channel))
			{
				return false;
			}
			else
			{
				Settings.FavoriteChannels.Add(channel);
				return true;
			}
		}
		/// <summary>
		/// 将频道从收藏中删除
		/// </summary>
		/// <param name="channel">频道</param>
		/// <returns>是否成功</returns>
		public bool RemoveFromFavorites(Channel channel)
		{
			if (!CanRemoveFromFavorites(channel))
			{
				return false;
			}
			else
			{
				Settings.FavoriteChannels.Remove(channel);
				return true;
			}
		}

		#endregion

		/// <summary>
		/// 记录播放器的状态
		/// </summary>
		class PlayerState
		{
			/// <summary>
			/// 当前频道
			/// </summary>
			public Channel CurrentChannel { get; set; }
			/// <summary>
			/// 当前歌曲
			/// </summary>
			public Song CurrentSong { get; set; }

			internal PlayerState(Channel currentChannel, Song currentSong)
			{
				CurrentChannel = currentChannel;
				CurrentSong = currentSong;
			}
		}
	}
}
