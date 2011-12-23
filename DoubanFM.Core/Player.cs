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
		public static readonly DependencyProperty CurrentDjCateProperty = DependencyProperty.Register("CurrentDjCate", typeof(Cate), typeof(Player));
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
				if (!IsInitialized) return;
				if (value == null)
					throw new Exception("频道不能设为空");
				if (value.IsPersonal && !value.IsSpecial && !UserAssistant.IsLoggedOn) return;	//没登录时不能使用私人频道
				if (CurrentChannel != null && CurrentChannel.IsSpecial && !value.IsSpecial)     //由特殊模式转为普通模式
					CurrentSong = null;
				if (CurrentChannel != value)
				{
					if (!value.IsDj) CurrentDjCate = null;
					if ((value.IsDj && (CurrentDjCate == null || !CurrentDjCate.Channels.Contains(value)))
						|| (!value.IsDj && CurrentDjCate != null))
						throw new Exception("在改变CurrentDjCate前改变了CurrentChannel");
					Channel lastChannel = CurrentChannel;
					SetValue(CurrentChannelProperty, value);
					RaiseStopedEvent();
					Settings.LastChannel = CurrentChannel;
					if (CurrentChannel.IsSpecial || CurrentChannel.IsDj || CurrentSong == null || lastChannel.IsDj)
						NewPlayList();
					else Skip();
					RaiseCurrentChannelChangedEvent();
				}
			}
		}
		/// <summary>
		/// 当前DJ门类
		/// </summary>
		public Cate CurrentDjCate
		{
			get { return (Cate)GetValue(CurrentDjCateProperty); }
			set { SetValue(CurrentDjCateProperty, value); }
		}
		private Song _currentSong;
		/// <summary>
		/// 当前歌曲
		/// </summary>
		public Song CurrentSong
		{
			get { return _currentSong; }
			private set
			{
				if (_currentSong != value)
				{
					_currentSong = value;
					if (_currentSong != null)
						RaiseCurrentSongChangedEvent();
				}
			}
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
		/// 搜索
		/// </summary>
		public MusicSearch MusicSearch { get; private set; }

		#endregion

		#region 成员变量

		/// <summary>
		/// 已播放音乐的列表
		/// </summary>
		private Queue<Song> _playedSongs = new Queue<Song>();
		/// <summary>
		/// 待播放音乐的列表
		/// </summary>
		private Queue<Song> _playListSongs = new Queue<Song>();
		/// <summary>
		/// 播放历史的字符串表示
		/// </summary>
		private Queue<string> _playedSongsString = new Queue<string>();
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
		/// 当DJ频道播放完毕时发生。
		/// </summary>
		public event EventHandler DjChannelFinishedPlaying;
		private void RaiseDjChannelFinishedPlayingEvent()
		{
			if (DjChannelFinishedPlaying != null)
				DjChannelFinishedPlaying(this, EventArgs.Empty);
		}
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
			MusicSearch = new MusicSearch(Settings);
			CurrentChannelChanged += new EventHandler((o, e) =>
			{
				IsLikedEnabled = !CurrentChannel.IsDj;
				IsNeverEnabled = CurrentChannel.IsPersonal && UserAssistant.IsLoggedOn;
			});
			CurrentSongChanged += new EventHandler((o, e) =>
			{
				IsLiked = CurrentSong.Like;
			});
			DjChannelFinishedPlaying += new EventHandler((o, e) =>
			{
				foreach (var channel in CurrentDjCate.Channels)
					if (CurrentChannel == channel)
					{
						if (CurrentChannel == CurrentDjCate.Channels.Last())
							CurrentChannel = CurrentDjCate.Channels.First();
						else
							CurrentChannel = CurrentDjCate.Channels.ElementAt(CurrentDjCate.Channels.ToList().IndexOf(CurrentChannel) + 1);
						return;
					}
			});
			UserAssistant.LogOnSucceed += new EventHandler((o, e) =>
			{
				IsNeverEnabled = CurrentChannel.IsPersonal;
			});
			UserAssistant.LogOffSucceed += new EventHandler((o, e) =>
			{
				IsNeverEnabled = false;
				if (CurrentChannel.IsPersonal && !CurrentChannel.IsSpecial)
					CurrentChannel = ChannelInfo.Public.First().Channels.First();
			});
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
			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
				{
					ChannelInfo channelInfo = null;
					string file = null;
					while (true)
					{
						file = new ConnectionBase().Get("http://douban.fm");
						channelInfo = new ChannelInfo(Json.ChannelInfo.FromHtml(file));
						if (!channelInfo.IsEffective) TakeABreak();
						else break;
					}
					UserAssistant.Update(file);
					Dispatcher.Invoke(new Action(() =>
						{
							/**
							当上次退出时是登录状态，但之后在浏览器里注销后，再打开软件会显示未登录，
							但Cookie还在，如果不清除Cookie，第一次登录会失败，清除后第一次登录也能成功
							 * */
							if (UserAssistant.CurrentState != Core.UserAssistant.State.LoggedOn)
								ConnectionBase.cc = ConnectionBase.DefaultCookie;
							ChannelInfo = channelInfo;
							IsInitialized = true;
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
					foreach (Cate djCate in ChannelInfo.Dj)
						foreach (Channel channel in djCate.Channels)
							if (channel == firstChannel)
							{
								CurrentDjCate = djCate;
								selected = true;
							}
				}
				else
					foreach (Cate cate in ChannelInfo.Public)
						foreach (Channel channel in cate.Channels)
							if (channel == firstChannel)
								selected = true;
				if (selected)
					CurrentChannel = firstChannel;
			}
			if (CurrentChannel == null)
				CurrentChannel = ChannelInfo.Public.First().Channels.First();
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
			if (UserAssistant.IsLoggedOn && !CurrentChannel.IsDj)
			{
				++Settings.User.Played;
			}
			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
				{
					lock (workLock)
					{
						PlayerState ps = GetPlayerState();
						AppendPlayedSongs("e");
						ChangeCurrentSong();
						Parameters parameters = new Parameters();
						parameters.Add("sid", ps.CurrentSong.SongId);
						parameters.Add("channel", ps.CurrentChannel.Id);
						parameters.Add("type", "e");
						parameters.Add("pid", ps.CurrentChannel.ProgramId);
						string url = ConnectionBase.ConstructUrlWithParameters("http://douban.fm/j/mine/playlist", parameters);
						while (true)
						{
							string result = new ConnectionBase().Get(url, "*/*", "http://douban.fm");
							if (string.IsNullOrEmpty(result))
							{
								TakeABreak();
								continue;
							}
							if (!ps.CurrentChannel.IsDj && result != "\"ok\"")
								Dispatcher.Invoke(new Action(() =>
									{
										RaiseFinishedPlayingReportFailedEvent(new ErrorEventArgs(new Exception("发送播放完成消息时出错，返回内容：" + result)));
									}));
							else break;
						}
					}
				}));
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
			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
			{
				lock (workLock)
				{
					_skipping = true;
					AppendPlayedSongs("s");
					PlayList pl = null;
					PlayerState ps = GetPlayerState();
					while (true)
					{
						pl = PlayList.GetPlayList(null, ps.CurrentSong.SongId, ps.CurrentChannel, "s", PlayedSongsToString());
						if (!ps.CurrentChannel.IsDj && pl.Count == 0) TakeABreak();
						else break;
					}
					if (!ps.CurrentChannel.IsDj)
						ChangePlayListSongs(pl);
					ChangeCurrentSong();
					_skipping = false;
				}
			}));
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
			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
				{
					lock (workLock)
					{
						_skipping = true;
						AppendPlayedSongs("s");
						PlayList pl = null;
						PlayerState ps = GetPlayerState();
						while (true)
						{
							pl = PlayList.GetPlayList(null, ps.CurrentSong.SongId, ps.CurrentChannel, "s", PlayedSongsToString());
							if (!ps.CurrentChannel.IsDj && pl.Count == 0) TakeABreak();
							else break;
						}
						if (!ps.CurrentChannel.IsDj)
							ChangePlayListSongs(pl);
						ChangeCurrentSong();
						_skipping = false;
					}
				}));
		}
		/// <summary>
		/// 喜欢这首歌
		/// </summary>
		void Like()
		{
			if (CurrentSong == null) return;
			if (CurrentChannel.IsDj) return;
			if (CurrentSong.Like) return;
			CurrentSong.Like = true;
			if (UserAssistant.IsLoggedOn)
			{
				++Settings.User.Liked;
			}
			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
			{
				lock (workLock)
				{
					PlayList pl = null;
					PlayerState ps = GetPlayerState();
					while (true)
					{
						pl = PlayList.GetPlayList(null, ps.CurrentSong.SongId, ps.CurrentChannel, "r", PlayedSongsToString());
						if (pl.Count == 0) TakeABreak();
						else break;
					}
					ChangePlayListSongs(pl);
				}
			}));
		}
		/// <summary>
		/// 不喜欢这首歌
		/// </summary>
		void Unlike()
		{
			if (CurrentSong == null) return;
			if (CurrentChannel.IsDj) return;
			if (!CurrentSong.Like) return;
			CurrentSong.Like = false;
			if (UserAssistant.IsLoggedOn)
			{
				--Settings.User.Liked;
			}
			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
			{
				lock (workLock)
				{
					PlayList pl = null;
					PlayerState ps = GetPlayerState();
					while (true)
					{
						pl = PlayList.GetPlayList(null, ps.CurrentSong.SongId, ps.CurrentChannel, "u", PlayedSongsToString());
						if (pl.Count == 0) TakeABreak();
						else break;
					}
					ChangePlayListSongs(pl);
				}
			}));
		}
		/// <summary>
		/// 对当前音乐标记不再播放
		/// type=b
		/// Ban
		/// </summary>
		public void Never()
		{
			if (CurrentSong == null) return;
			if (!CurrentChannel.IsPersonal) return;
			if (_neverring) return;
			RaiseStopedEvent();
			if (UserAssistant.IsLoggedOn)
			{
				++Settings.User.Banned;
			}
			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
			{
				lock (workLock)
				{
					_neverring = true;
					AppendPlayedSongs("b");
					PlayList pl = null;
					PlayerState ps = GetPlayerState();
					while (true)
					{
						pl = PlayList.GetPlayList(null, ps.CurrentSong.SongId, ps.CurrentChannel, "b", PlayedSongsToString());
						if (pl.Count == 0) TakeABreak();
						else break;
					}
					ChangePlayListSongs(pl);
					ChangeCurrentSong();
					_neverring = false;
				}
			}));
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
			 * 所以这里只是复制了一下NewPlayList()的代码，然后把ChangeCurrentSong();这句话删除掉
			 * */
			if ((DateTime.Now - _pauseTime).TotalMinutes > 30 && !CurrentChannel.IsDj)
			{
				ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
				{
					lock (workLock)
					{
						PlayList pl = null;
						PlayerState ps = GetPlayerState();
						while (true)
						{
							pl = PlayList.GetPlayList(ps.CurrentChannel.Context, null, ps.CurrentChannel, "n", PlayedSongsToString());
							if (pl.Count == 0) TakeABreak();
							else break;
						}
						ChangePlayListSongs(pl);
					}
				}));
			}
		}

		#endregion

		#region 成员方法

		/// <summary>
		/// 当预加载音乐完成后，可调用此方法改变下一首音乐的文件地址
		/// </summary>
		public void ChangeNextSongUrl(string address)
		{
			if (string.IsNullOrEmpty(address)) return;
			lock (_playListSongs)
			{
				if (_playListSongs.Count == 0) return;
				_playListSongs.First().FileUrl = address;
			}
		}
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
		void SaveSettings()
		{
			Settings.Save();
		}
		/// <summary>
		/// 保存Cookies
		/// </summary>
		/// <returns>成功与否</returns>
		static bool SaveCookies()
		{
			return ConnectionBase.SaveCookies();
		}
		/// <summary>
		/// 执行与释放或重置非托管资源相关的应用程序定义的任务。
		/// </summary>
		public void Dispose()
		{
			SaveSettings();
			if (UserAssistant.IsLoggedOn && !Settings.AutoLogOnNextTime)
				ConnectionBase.cc = ConnectionBase.DefaultCookie;
			SaveCookies();
		}
		/// <summary>
		/// 获取全新的播放列表
		/// type=n
		/// New
		/// </summary>
		void NewPlayList()          //根据douban.fm的网络传输观察，只有全新的播放列表才利用context信息，其他播放列表请求都不传输context
		{
			if (!IsInitialized) return;
			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
			{
				PlayList pl = null;
				PlayerState ps = GetPlayerState();
				while(true)
				{
					pl = PlayList.GetPlayList(ps.CurrentChannel.Context, null, ps.CurrentChannel, "n", PlayedSongsToString());
					if (pl.Count == 0) TakeABreak();
					else break;
				}
				ChangePlayListSongs(pl);
				ChangeCurrentSong();
			}));
		}
		/// <summary>
		/// 增加已播放音乐的记录
		/// </summary>
		/// <param name="operationType">操作类型</param>
		void AppendPlayedSongs(string operationType)
		{
			PlayerState ps = GetPlayerState();
			if (ps.CurrentSong == null) return;
			lock (_playedSongs) lock (_playedSongsString)
				{
					_playedSongs.Enqueue(CurrentSong);
					_playedSongsString.Enqueue("|" + CurrentSong.SongId + ":" + operationType);
					while (_playedSongs.Count > 20)
					{
						_playedSongs.Dequeue();
						_playedSongsString.Dequeue();
					}
				}
		}
		/// <summary>
		/// 将已播放音乐的记录转换成字符串
		/// </summary>
		string PlayedSongsToString()
		{
			lock (_playedSongsString)
			{
				StringBuilder sb = new StringBuilder();
				foreach (var s in _playedSongsString)
					sb.Append(s);
				return sb.ToString();
			}
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
			PlayerState ps = GetPlayerState();
			if (_playListSongs.Count == 0 && ps.CurrentChannel.IsDj)
				Dispatcher.Invoke(new Action(() => { RaiseDjChannelFinishedPlayingEvent(); }));
			else
			{
				while (_playListSongs.Count == 0)
				{
					TakeABreak();
				}
				lock (_playListSongs)
					Dispatcher.Invoke(new Action(() => { CurrentSong = _playListSongs.Dequeue(); }));
				CheckPlayListSongsLength();
			}
		}
		/// <summary>
		/// 检查播放列表的长度，若为0，则请求一个新播放列表
		/// type=p
		/// PlayOut
		/// </summary>
		void CheckPlayListSongsLength()
		{
			PlayerState ps = GetPlayerState();
			if (_playListSongs.Count == 0 && !ps.CurrentChannel.IsDj)
				ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
					{
						PlayList pl = null;
						while (true)
						{
							pl = PlayList.GetPlayList(null, ps.CurrentSong.SongId, ps.CurrentChannel, "p", PlayedSongsToString());
							if (pl.Count == 0) TakeABreak();
							else break;
						}
						ChangePlayListSongs(pl);
					}));
		}
		/// <summary>
		/// 网络发送间歇
		/// </summary>
		void TakeABreak()
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

		#endregion

		class PlayerState
		{
			public Channel CurrentChannel { get; set; }
			public Song CurrentSong { get; set; }

			internal PlayerState(Channel currentChannel, Song currentSong)
			{
				CurrentChannel = currentChannel;
				CurrentSong = currentSong;
			}
		}
	}
}
