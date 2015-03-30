/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using System.Configuration;

namespace DoubanFM.Core
{
	/// <summary>
	/// 播放器核心
	/// </summary>
	public class Player : DependencyObject
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

        /// <summary>
        /// 当用户的播放记录发生改变时发生。
        /// </summary>
        public event EventHandler PlayRecordChanged;
        private void RaisePlayRecordChangedEvent()
        {
            if (PlayRecordChanged != null)
            {
                PlayRecordChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 当用户的登录状态过期时发生。
        /// </summary>
        public event EventHandler UserExpired;
        private void RaiseUserExpiredEvent()
        {
            if (UserExpired != null)
            {
                UserExpired(this, EventArgs.Empty);
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

            //加红心功能始终启用
		    IsLikedEnabled = true;

			//歌曲改变时更新红心状态
			CurrentSongChanged += new EventHandler((o, e) =>
			{
				if (CurrentSong != null)
				{
					IsLiked = CurrentSong.Like;
				}
			});

            UserAssistant.CurrentStateChanged += (o, e) =>
                {
                    //更新IsNeverEnabled
                    IsNeverEnabled = UserAssistant.CurrentState == UserAssistant.State.LoggedOn || UserAssistant.CurrentState == UserAssistant.State.LoggingOff;
                    switch ((UserAssistant.State) e.NewValue)
                    {
                        case UserAssistant.State.LoggedOn:
                            RaisePlayRecordChangedEvent();
                            break;
                        case UserAssistant.State.LoggedOff:
                            if (CurrentChannel != null && CurrentChannel.IsPersonal && !CurrentChannel.IsSpecial)
                                CurrentChannel = ChannelInfo.Public.First();
                            break;
                    }
                };

			//获取播放列表失败后引发事件
			PlayList.GetPlayListFailed += (o, e) => Dispatcher.Invoke(new Action(() => RaiseGetPlayListFailedEvent(e)));
		}
		/// <summary>
		/// 播放器初始化
		/// </summary>
		public void Initialize()
		{
			if (IsInitialized) return;
			Debug.WriteLine(DateTime.Now + " 播放器核心初始化中");
			//bool lastTimeLoggedOn = Settings.LastTimeLoggedOn;
			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
				{
                    ////如果用户上次退出软件时处于未登录状态，则启动时不更新登录状态
                    //if (lastTimeLoggedOn)
                    //{
                    //    string file;
                    //    while (true)
                    //    {
                    //        Debug.WriteLine(DateTime.Now + " 刷新豆瓣FM主页……");
                    //        file = new ConnectionBase().Get("http://douban.fm/");
                    //        Debug.WriteLine(DateTime.Now + " 刷新完成");
                    //        if (!string.IsNullOrEmpty(file))
                    //        {
                    //            break;
                    //        }
                    //        TakeABreak();
                    //    }

                    //    //更新用户的登录状态
                    //    UserAssistant.Update(file);
                    //}

                    //检查用户登录状态是否已过期。
                    bool expired = !string.IsNullOrEmpty(UserAssistant.Settings.User.Token);
                    UserAssistant.Initialize();
                    expired = expired && string.IsNullOrEmpty(UserAssistant.Settings.User.Token);

				    var channelInfo = GetChannelInfo();
					
					Dispatcher.Invoke(new Action(() =>
						{
							/**
							当上次退出时是登录状态，但之后在浏览器里注销后，再打开软件会显示未登录，
							但Cookie还在，如果不清除Cookie，第一次登录会失败，清除后第一次登录也能成功
							 * */
                            //if (UserAssistant.CurrentState != Core.UserAssistant.State.LoggedOn)
                            //    UserAssistant.LogOff();
							ChannelInfo = channelInfo;
                            RaisePlayRecordChangedEvent();
							IsInitialized = true;
							Debug.WriteLine(DateTime.Now + " 播放器核心初始化完成");
                           
                            if (expired)
                            {
                                RaiseUserExpiredEvent();
                            }

							//选择一个频道
							ChooseChannelAtStartup();
						}));
				}));
		}

        /// <summary>
        /// 根据情况从本地或服务器获取频道列表。
        /// </summary>
        /// <returns>频道列表</returns>
        private static ChannelInfo GetChannelInfo()
        {
            var localPath = Path.Combine(ConnectionBase.DataFolder, "channelinfo");
            ChannelInfo localChannelInfo = null;

            //尝试获取本地频道列表
            try
            {
                if (File.Exists(localPath))
                {
                    var localChannelInfoTime = File.GetLastWriteTime(localPath);
                    var content = File.ReadAllText(localPath);
                    var channelInfo = new ChannelInfo(Json.JsonHelper.FromJson<Json.ChannelInfo>(content));
                    if (channelInfo.IsEffective)
                    {
                        localChannelInfo = channelInfo;
                    }

                    //满足条件时采用本地频道列表
                    var distance = TimeSpan.FromSeconds(int.Parse(ConfigurationManager.AppSettings["Player.ChannelInfoExpireSeconds"]));
                    if (localChannelInfo != null && DateTime.Now - localChannelInfoTime < distance)
                    {
                        return localChannelInfo;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(DateTime.Now + " 获取本地频道列表失败：" + ex.Message);
            }

            //尝试获取服务器频道列表
            int tryCount = 0;
            int tryCountMax = int.Parse(ConfigurationManager.AppSettings["Player.ChannelInfoRetryCount"]);
            while (true)
            {
                //获取服务器频道列表多次失败后采用本地频道列表
                if (tryCount == tryCountMax && localChannelInfo != null) return localChannelInfo;

                ++tryCount;

                Debug.WriteLine(DateTime.Now + " 获取频道列表……");
                var file = new ConnectionBase {UseGzip = true}.Get(
                    string.Format(ConfigurationManager.AppSettings["Player.ChannelInfoUrlFormat"],
                        typeof (Player).Assembly.GetName().Version));
                Debug.WriteLine(DateTime.Now + " 获取频道列表完成");
                if (string.IsNullOrEmpty(file))
                {
                    TakeABreak();
                    continue;
                }
                var channelInfo = new ChannelInfo(Json.JsonHelper.FromJson<Json.ChannelInfo>(file));
                if (!channelInfo.IsEffective)
                {
                    Debug.WriteLine(DateTime.Now + " 获取频道列表失败");
                    TakeABreak();
                }
                else
                {
                    Debug.WriteLine(DateTime.Now + " 获取频道列表成功");
                    try
                    {
                        File.WriteAllText(localPath, file);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(DateTime.Now + " 写入本地频道列表失败：" + ex.Message);
                    }
                    return channelInfo;
                }
            }
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
				//else if (firstChannel.IsDj)
				{
					foreach (Channel channel in ChannelInfo.Dj)
						if (channel == firstChannel)
						{
							selected = true;
						}
				}
				//else
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
        /// 码率设置已改变
        /// </summary>
        public void ProRateChanged()
        {
            Report("n", false);
        }

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
                RaisePlayRecordChangedEvent();
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
                RaisePlayRecordChangedEvent();
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
                RaisePlayRecordChangedEvent();
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
                RaisePlayRecordChangedEvent();
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
		    Settings.Current = Settings;
		}
		/// <summary>
		/// 保存偏好设置
		/// </summary>
		public void SaveSettings()
		{
			Settings.Save();
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
						pl = PlayList.GetPlayList(ps, type);
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
				pl = PlayList.GetPlayList(ps, "p");
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
	    private PlayerState GetPlayerState()
	    {
	        PlayerState ps = null;
	        if (CheckAccess())
	            ps = new PlayerState(Settings.User == null ? null : (User) Settings.User.Clone(),
	                                 CurrentChannel == null ? null : (Channel) CurrentChannel.Clone(),
	                                 CurrentSong == null ? null : (Song) CurrentSong.Clone());
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
		internal class PlayerState
		{
            public User CurrentUser { get; set; }
			/// <summary>
			/// 当前频道
			/// </summary>
			public Channel CurrentChannel { get; set; }
			/// <summary>
			/// 当前歌曲
			/// </summary>
			public Song CurrentSong { get; set; }

			internal PlayerState(User currentUser, Channel currentChannel, Song currentSong)
			{
			    CurrentUser = currentUser;
				CurrentChannel = currentChannel;
				CurrentSong = currentSong;
			}
		}
	}
}
