using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DoubanFM.Core;
using System.Windows.Threading;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Windows.Shell;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DoubanFM
{
	/// <summary>
	/// DoubanFMWindow.xaml 的交互逻辑
	/// </summary>
	public partial class DoubanFMWindow : Window
	{
		#region 成员变量

		/// <summary>
		/// 播放器
		/// </summary>
		private Player _player;
		/// <summary>
		/// 进度更新计时器
		/// </summary>
		private DispatcherTimer _progressRefreshTimer;
		/// <summary>
		/// 防止不执行下一首
		/// </summary>
		private DispatcherTimer _forceNextTimer;
		/// <summary>
		/// 各种无法在XAML里直接启动的Storyboard
		/// </summary>
		private Storyboard BackgroundColorStoryboard, ShowCover1Storyboard, ShowCover2Storyboard, SlideCoverRightStoryboard, SlideCoverLeftStoryboard, ChangeSongInfoStoryboard, DjChannelClickStoryboard;
		/// <summary>
		/// 滑动封面的计时器
		/// </summary>
		private DispatcherTimer _slideCoverRightTimer, _slideCoverLeftTimer;
		/// <summary>
		/// 当前显示的封面
		/// </summary>
		private Image _cover;
		/// <summary>
		/// 托盘图标
		/// </summary>
		private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();
		/// <summary>
		/// 托盘图标右键菜单中的各个菜单项
		/// </summary>
		private System.Windows.Forms.ToolStripMenuItem notifyIcon_ShowWindow, notifyIcon_Heart, notifyIcon_Never, notifyIcon_PlayPause, notifyIcon_Next, notifyIcon_Exit;
		/// <summary>
		/// 隐藏窗口
		/// </summary>
		private InteropWindow _interopWindow;
		/// <summary>
		/// 用于进程间更换频道的临时文件
		/// </summary>
		private string _channelTempFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\K.F.Storm\豆瓣电台\" + "channeltemp.dat";

		#endregion

		#region 构造和初始化

		public DoubanFMWindow()
		{
			Channel channel = Channel.FromCommandLineArgs(System.Environment.GetCommandLineArgs().ToList());
			//只允许运行一个实例
			if (HasAnotherInstance())
			{
				if (channel != null) WriteChannelToTempFile(channel);
				App.Current.Shutdown(0);
				return;
			}
			
			InitializeComponent();
			_player = (Player)FindResource("Player");
			BackgroundColorStoryboard = (Storyboard)FindResource("BackgroundColorStoryboard");
			ShowCover1Storyboard = (Storyboard)FindResource("ShowCover1Storyboard");
			ShowCover2Storyboard = (Storyboard)FindResource("ShowCover2Storyboard");
			SlideCoverRightStoryboard = (Storyboard)FindResource("SlideCoverRightStoryboard");
			SlideCoverLeftStoryboard = (Storyboard)FindResource("SlideCoverLeftStoryboard");
			ChangeSongInfoStoryboard = (Storyboard)FindResource("ChangeSongInfoStoryboard");
			DjChannelClickStoryboard = (Storyboard)FindResource("DjChannelClickStoryboard");

			InitNotifyIcon();

			PbPassword.Password = _player.Settings.User.Password;
			if (!_player.Settings.IsShadowEnabled)
				MainPanel.Margin = new Thickness(1);
			_cover = Cover1;
			_progressRefreshTimer = new DispatcherTimer();
			_progressRefreshTimer.Interval = new TimeSpan(1000000);
			_progressRefreshTimer.Tick += new EventHandler(timer_Tick);
			_progressRefreshTimer.Start();
			_forceNextTimer = new DispatcherTimer();
			_forceNextTimer.Interval = new TimeSpan(600000000);
			_forceNextTimer.Tick += new EventHandler(_forceNextTimer_Tick);
			_forceNextTimer.Start();
			_slideCoverRightTimer = new DispatcherTimer();
			_slideCoverRightTimer.Interval = new TimeSpan(5000000);
			_slideCoverRightTimer.Tick += new EventHandler(SlideCoverRightTimer_Tick);
			_slideCoverLeftTimer = new DispatcherTimer();
			_slideCoverLeftTimer.Interval = new TimeSpan(5000000);
			_slideCoverLeftTimer.Tick += new EventHandler(SlideCoverLeftTimer_Tick);

			_player.Initialized += new EventHandler((o, e) => { ShowChannels(); });
			_player.CurrentChannelChanged += new EventHandler((o, e) =>
				{
					if (!_player.CurrentChannel.IsPersonal || _player.CurrentChannel.IsSpecial)
						PersonalChannels.SelectedItem = null;
					if (!_player.CurrentChannel.IsPublic)
						PublicChannels.SelectedItem = null;
					if (!_player.CurrentChannel.IsDj)
					{
						DjCates.SelectedItem = null;
						DjChannels.SelectedItem = null;
					}
					if (!_player.CurrentChannel.IsSpecial)
						SearchResultList.SelectedItem = null;
					if (_player.CurrentChannel.IsPersonal && !_player.CurrentChannel.IsSpecial && _player.CurrentChannel != (Channel)PersonalChannels.SelectedItem)
					{
						PersonalChannels.SelectedItem = _player.CurrentChannel;
						PersonalClickStoryboard.Begin();
					}
					if (_player.CurrentChannel.IsPublic && _player.CurrentChannel != (Channel)PublicChannels.SelectedItem)
					{
						PublicChannels.SelectedItem = _player.CurrentChannel;
						PublicClickStoryboard.Begin();
					}
					if (_player.CurrentChannel.IsDj && DjCates.Items.Contains(_player.CurrentDjCate))
					{
						DjCates.SelectedItem = _player.CurrentDjCate;
						if (DjChannels.Items.Contains(_player.CurrentChannel) && (Channel)DjChannels.SelectedItem != _player.CurrentChannel)
						{
							DjChannels.SelectedItem = _player.CurrentChannel;
							DjChannelClickStoryboard.Begin();
						}
					}
					if (!_player.CurrentChannel.IsDj)
						AddChannelToJumpList(_player.CurrentChannel);
				});
			_player.CurrentSongChanged += new EventHandler((o, e) =>
			{
				Update();
				if (_player.CurrentSong.IsAd)
					_player.Skip();
				if (_player.IsPlaying) Audio.Play();
				else Audio.Pause();
			});
			_player.Paused += new EventHandler((o, e) =>
			{
				CheckBoxPause.IsChecked = !_player.IsPlaying;
				PauseThumb.ImageSource = (ImageSource)FindResource("PlayThumbImage");
				Audio.Pause();
				notifyIcon_PlayPause.Text = "播放";
			});
			_player.Played += new EventHandler((o, e) =>
			{
				CheckBoxPause.IsChecked = !_player.IsPlaying;
				PauseThumb.ImageSource = (ImageSource)FindResource("PauseThumbImage");
				Audio.Play();
				notifyIcon_PlayPause.Text = "暂停";
			});
			_player.Stoped += new EventHandler((o, e) => { Audio.Stop(); });
			_player.UserAssistant.LogOnFailed += new EventHandler((o, e) =>
			{
				if (_player.UserAssistant.HasCaptcha)
					Captcha.Source = new BitmapImage(new Uri(_player.UserAssistant.CaptchaUrl));
			});
			_player.UserAssistant.LogOnSucceed += new EventHandler((o, e) => { ShowChannels(); });
			_player.UserAssistant.LogOffSucceed += new EventHandler((o, e) =>
			{
				if (_player.UserAssistant.HasCaptcha)
					Captcha.Source = new BitmapImage(new Uri(_player.UserAssistant.CaptchaUrl));
				ShowChannels();
			});

			_player.IsLikedChanged += new EventHandler((o, e) =>
			{
				CheckBoxLike.IsChecked = _player.IsLiked;
				if (_player.IsLikedEnabled)
					if (_player.IsLiked)
						LikeThumb.ImageSource = (ImageSource)FindResource("LikeThumbImage");
					else LikeThumb.ImageSource = (ImageSource)FindResource("UnlikeThumbImage");
				else
					LikeThumb.ImageSource = (ImageSource)FindResource("LikeThumbImage_Disabled");
				notifyIcon_Heart.Checked = _player.IsLiked;
			});
			_player.IsLikedEnabledChanged += new EventHandler((o, e) =>
			{
				if (_player.IsLikedEnabled)
					if (_player.IsLiked)
						LikeThumb.ImageSource = (ImageSource)FindResource("LikeThumbImage");
					else LikeThumb.ImageSource = (ImageSource)FindResource("UnlikeThumbImage");
				else
					LikeThumb.ImageSource = (ImageSource)FindResource("LikeThumbImage_Disabled");
				LikeThumb.IsEnabled = _player.IsLikedEnabled;
				notifyIcon_Heart.Enabled = _player.IsLikedEnabled;
			});
			_player.IsNeverEnabledChanged += new EventHandler((o, e) =>
			{
				if (_player.IsNeverEnabled)
					NeverThumb.ImageSource = (ImageSource)FindResource("NeverThumbImage");
				else
					NeverThumb.ImageSource = (ImageSource)FindResource("NeverThumbImage_Disabled");
				NeverThumb.IsEnabled = _player.IsNeverEnabled;
				notifyIcon_Never.Enabled = _player.IsNeverEnabled;
			});
			_player.GetPlayListFailed += new EventHandler<PlayList.PlayListEventArgs>((o, e) =>
			{
				System.Windows.MessageBox.Show(this, "获取播放列表失败：" + e.Msg, "程序即将关闭", MessageBoxButton.OK, MessageBoxImage.Error);
				this.Close();
			});

			this.Loaded += new RoutedEventHandler((o, e) =>
			{
				_interopWindow = new InteropWindow();
				_interopWindow.Show();
			});

			if (channel != null) _player.Settings.LastChannel = channel;
			//定时检查临时文件，看是否需要更换频道
			ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
			{
				while (true)
				{
					Thread.Sleep(100);
					if (File.Exists(_channelTempFile))
					{
						Channel ch = LoadChannelFromTempFile();
						try
						{
							File.Delete(_channelTempFile);
						}
						catch { }
						if (ch != null)
							Dispatcher.Invoke(new Action(() =>
							{
								if (_player.IsInitialized) _player.CurrentChannel = ch;
								else _player.Settings.LastChannel = ch;
							}));
					}
				}
			}));
			_player.Initialize();
		}

		/// <summary>
		/// 初始化托盘图标
		/// </summary>
		private void InitNotifyIcon()
		{
			notifyIcon.Visible = _player.Settings.AlwaysShowNotifyIcon;
			notifyIcon.Icon = Properties.Resources.NotifyIcon;
			notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler((s, e) =>
			{
				if (e.Button == System.Windows.Forms.MouseButtons.Left)
				{
					if (this.IsVisible == false)
					{
						this.Visibility = Visibility.Visible;
						Dispatcher.BeginInvoke(new Action(() =>
							{
								this.WindowState = WindowState.Normal;
								this.Activate();
							}));
					}
					else this.Visibility = Visibility.Hidden;
				}
			});
			System.Windows.Forms.ContextMenuStrip notifyIconMenu = new System.Windows.Forms.ContextMenuStrip();
			notifyIcon.Text = "豆瓣电台";
			notifyIcon.ContextMenuStrip = notifyIconMenu;

			notifyIconMenu.Items.Add(new ToolStripMenuItem("显示窗口"));
			notifyIcon_ShowWindow = (ToolStripMenuItem)notifyIconMenu.Items[notifyIconMenu.Items.Count - 1];
			notifyIcon_ShowWindow.Click += new EventHandler((s, e) =>
			{
				this.Visibility = Visibility.Visible;
				Dispatcher.BeginInvoke(new Action(() =>
				{
					this.WindowState = WindowState.Normal;
					this.Activate();
				}));
			});
			notifyIconMenu.Items.Add("-");
			notifyIconMenu.Items.Add(new ToolStripMenuItem("喜欢"));
			notifyIcon_Heart = (ToolStripMenuItem)notifyIconMenu.Items[notifyIconMenu.Items.Count - 1];
			notifyIcon_Heart.CheckOnClick = true;
			notifyIcon_Heart.CheckedChanged += new EventHandler((o, e) =>
			{
				_player.IsLiked = notifyIcon_Heart.Checked;
			});
			notifyIconMenu.Items.Add(new ToolStripMenuItem("不再播放"));
			notifyIcon_Never = (ToolStripMenuItem)notifyIconMenu.Items[notifyIconMenu.Items.Count - 1];
			notifyIcon_Never.Enabled = false;
			notifyIcon_Never.Click += new EventHandler((s, e) => { _player.Never(); });
			notifyIconMenu.Items.Add("-");
			notifyIconMenu.Items.Add(new ToolStripMenuItem("暂停"));
			notifyIcon_PlayPause = (ToolStripMenuItem)notifyIconMenu.Items[notifyIconMenu.Items.Count - 1];
			notifyIcon_PlayPause.Click += new EventHandler((s, e) =>
			{
				System.Windows.Forms.ToolStripItem sender = (System.Windows.Forms.ToolStripItem)s;
				if (sender.Text == "播放") _player.IsPlaying = true;
				else _player.IsPlaying = false;
			});
			notifyIconMenu.Items.Add(new ToolStripMenuItem("下一首"));
			notifyIcon_Next = (ToolStripMenuItem)notifyIconMenu.Items[notifyIconMenu.Items.Count - 1];
			notifyIcon_Next.Click += new EventHandler((s, e) => { _player.Skip(); });
			notifyIconMenu.Items.Add("-");
			notifyIconMenu.Items.Add(new ToolStripMenuItem("退出"));
			notifyIcon_Exit = (ToolStripMenuItem)notifyIconMenu.Items[notifyIconMenu.Items.Count - 1];
			notifyIcon_Exit.Click += new EventHandler((s, e) => { this.Close(); });
		}

		#endregion

		#region 其他
		/// <summary>
		/// 检测是否有另一个实例正在运行
		/// </summary>
		bool HasAnotherInstance()
		{
			IntPtr hwnd = InteropWindow.FindWindow(null, InteropWindow.CorrectTitle);
			return hwnd != IntPtr.Zero;
		}
		/// <summary>
		/// 将频道写入临时文件
		/// </summary>
		void WriteChannelToTempFile(Channel channel)
		{
			if (channel != null)
				try
				{
					using (FileStream stream = File.OpenWrite(_channelTempFile))
					{
						BinaryFormatter formatter = new BinaryFormatter();
						formatter.Serialize(stream, channel);
					}
				}
				catch { }
		}
		/// <summary>
		/// 从临时文件加载频道
		/// </summary>
		Channel LoadChannelFromTempFile()
		{
			try
			{
				using (FileStream stream = File.OpenRead(_channelTempFile))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					return (Channel)formatter.Deserialize(stream);
				}
			}
			catch
			{
				return null;
			}
		}
		/// <summary>
		/// 显示频道列表
		/// </summary>
		private void ShowChannels()
		{
			ObservableCollection<Channel> PersonalChannelsItem = new ObservableCollection<Channel>();
			if (_player.UserAssistant.IsLoggedOn)
				foreach (Cate cate in _player.ChannelInfo.Personal)
					foreach (Channel channel in cate.Channels)
						PersonalChannelsItem.Add(channel);
			ObservableCollection<Channel> PublicChannelsItem = new ObservableCollection<Channel>();
			foreach (Cate cate in _player.ChannelInfo.Public)
				foreach (Channel channel in cate.Channels)
					PublicChannelsItem.Add(channel);
			ObservableCollection<Cate> DjCatesItem = new ObservableCollection<Cate>();
			foreach (Cate djcate in _player.ChannelInfo.Dj)
				DjCatesItem.Add(djcate);
			PersonalChannels.ItemsSource = PersonalChannelsItem;
			PublicChannels.ItemsSource = PublicChannelsItem;
			DjCates.ItemsSource = DjCatesItem;
		}
		/// <summary>
		/// 更新界面内容，主要是音乐信息。换音乐时自动调用。
		/// </summary>
		private void Update()
		{
			ChangeCover();
			try
			{
				Audio.Source = new Uri(_player.CurrentSong.FileUrl);

				Audio.IsMuted = !Audio.IsMuted;     //防止无敌静音
				Thread.Sleep(50);
				Audio.IsMuted = !Audio.IsMuted;
				Audio.Volume = _player.Settings.Volume;
			}
			catch { }
			((StringAnimationUsingKeyFrames)ChangeSongInfoStoryboard.Children[1]).KeyFrames[0].Value = _player.CurrentSong.Title;
			((StringAnimationUsingKeyFrames)ChangeSongInfoStoryboard.Children[2]).KeyFrames[0].Value = _player.CurrentSong.Artist;
			((StringAnimationUsingKeyFrames)ChangeSongInfoStoryboard.Children[3]).KeyFrames[0].Value = _player.CurrentSong.Album;
			ChangeSongInfoStoryboard.Begin();
			string stringA = _player.CurrentSong.Title + " - " + _player.CurrentSong.Artist;
			string stringB = "    豆瓣电台 - " + _player.CurrentChannel.Name;
			this.Title = stringA + stringB;
			if (this.Title.Length <= 63)        //Windows限制托盘图标的提示信息最长为63个字符
				notifyIcon.Text = this.Title;
			else
			{
				notifyIcon.Text = stringA.Substring(0, Math.Max(63 - stringB.Length, 0));
				if (notifyIcon.Text.Length + stringB.Length <= 63)
					notifyIcon.Text += stringB.Length;
				else
					notifyIcon.Text = stringA.Substring(0, Math.Min(stringA.Length, 63));
			}
			ChannelTextBlock.Text = _player.CurrentChannel.Name;
			TotalTime.Content = TimeSpanToStringConverter.QuickConvert(_player.CurrentSong.Length);
			CurrentTime.Content = TimeSpanToStringConverter.QuickConvert(new TimeSpan(0));
			Slider.Minimum = 0;
			Slider.Maximum = _player.CurrentSong.Length.TotalSeconds;
			Slider.Value = 0;
		}

		/// <summary>
		/// 更改封面
		/// </summary>
		void ChangeCover()
		{
			try
			{
				BitmapImage bitmap = new BitmapImage(new Uri(_player.CurrentSong.Picture));
				Debug.WriteLine(_player.CurrentSong.Picture);
				if (bitmap.IsDownloading)
				{
					bitmap.DownloadCompleted += new EventHandler((o, e) =>
					{
						Dispatcher.Invoke(new Action(() =>
						{
							try
							{
								if (((BitmapImage)o).UriSource.AbsoluteUri == new Uri(_player.CurrentSong.Picture).AbsoluteUri)
								{
									ChangeBackground((BitmapImage)o);
									SwitchCover((BitmapImage)o);
								}
							}
							catch { }
						}));
					});
					bitmap.DownloadFailed += new EventHandler<ExceptionEventArgs>((o, e) =>
					{
						Dispatcher.Invoke(new Action(() =>
						{
							try
							{
								if (((BitmapImage)o).UriSource.AbsoluteUri.ToString() == new Uri(_player.CurrentSong.Picture).AbsoluteUri.ToString())
								{
									BitmapImage bitmapDefault = new BitmapImage(new Uri("pack://application:,,,/DoubanFM;component/Images/DoubanFM_NoCover.png"));
									ChangeBackground(bitmapDefault);
									SwitchCover(bitmapDefault);
								}
							}
							catch { }
						}));
					});
				}
				else
				{
					ChangeBackground(bitmap);
					SwitchCover(bitmap);
				}
			}
			catch
			{
				BitmapImage bitmap = new BitmapImage(new Uri("pack://application:,,,/DoubanFM;component/Images/DoubanFM_NoCover.png"));
				ChangeBackground(bitmap);
				SwitchCover(bitmap);
			}
		}
		/// <summary>
		/// 根据封面颜色更换背景。封面加载成功时自动调用
		/// </summary>
		/// <param name="NewCover">新封面</param>
		void ChangeBackground(BitmapImage NewCover)
		{
			ColorAnimation animation = (ColorAnimation)BackgroundColorStoryboard.Children[0];
			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
				{
					Color to = ColorFunctions.GetImageColorForBackground(NewCover);
					Dispatcher.Invoke(new Action(() =>
						{
							animation.To = to;
							BackgroundColorStoryboard.Begin();
						}));
				}));
		}
		/// <summary>
		/// 更换封面。封面加载成功时自动调用
		/// </summary>
		/// <param name="NewCover">新封面</param>
		void SwitchCover(BitmapImage NewCover)
		{
			if (_cover == Cover1)
			{
				Cover2.Source = NewCover;
				_cover = Cover2;
				ShowCover2Storyboard.Begin();
			}
			else
			{
				Cover1.Source = NewCover;
				_cover = Cover1;
				ShowCover1Storyboard.Begin();
			}
		}
		/// <summary>
		/// 将频道添加到跳转列表
		/// </summary>
		private void AddChannelToJumpList(Channel channel)
		{
			JumpList jumpList = JumpList.GetJumpList(App.Current);
			if (jumpList == null) jumpList = new JumpList();
			jumpList.ShowRecentCategory = true;
			jumpList.ShowFrequentCategory = true;
			foreach (JumpTask jumpItem in jumpList.JumpItems)
			{
				if (jumpItem.Title == channel.Name) return;
			}
			JumpTask jumpTask = new JumpTask();
			jumpTask.Title = channel.Name;
			jumpTask.Description = jumpTask.Title;
			jumpTask.Arguments = channel.ToCommandLineArgs();
			JumpList.AddToRecentCategory(jumpTask);
			JumpList.SetJumpList(App.Current, jumpList);
		}
		internal void InteropChangeChannel(Channel channel)
		{
			_player.CurrentChannel = channel;
		}
		#endregion

		#region 事件响应
		/// <summary>
		/// 主界面中按下“下一首”
		/// </summary>
		private void ButtonNext_Click(object sender, RoutedEventArgs e)
		{
			_player.Skip();
		}
		/// <summary>
		/// 音乐播放结束
		/// </summary>
		private void Audio_MediaEnded(object sender, RoutedEventArgs e)
		{
			_player.CurrentSongFinishedPlaying();
		}
		/// <summary>
		/// 音乐遇到错误
		/// </summary>
		private void Audio_MediaFailed(object sender, ExceptionRoutedEventArgs e)
		{
			_player.CurrentSongFinishedPlaying();
		}
		/// <summary>
		/// 计时器响应函数，用于更新时间信息
		/// </summary>
		void timer_Tick(object sender, EventArgs e)
		{
			CurrentTime.Content = TimeSpanToStringConverter.QuickConvert(Audio.Position);
			Slider.Value = Audio.Position.TotalSeconds;
		}
		/// <summary>
		/// 当已播放时间超过总时间时，报告音乐已播放完毕。防止网络不好时播放完毕但不换歌
		/// </summary>
		void _forceNextTimer_Tick(object sender, EventArgs e)
		{
			if (Audio.NaturalDuration.HasTimeSpan)
				if ((Audio.Position - Audio.NaturalDuration.TimeSpan).TotalSeconds > 5)
					_player.CurrentSongFinishedPlaying();
		}
		/// <summary>
		/// 修正音乐总时间。音乐加载完成时调用。
		/// </summary>
		void Audio_MediaOpened(object sender, RoutedEventArgs e)
		{
			if (Audio.NaturalDuration.HasTimeSpan)
			{
				if (Math.Abs((TimeSpanToStringConverter.QuickConvertBack((string)TotalTime.Content) - Audio.NaturalDuration.TimeSpan).TotalSeconds) > 2)
					TotalTime.Content = TimeSpanToStringConverter.QuickConvert(Audio.NaturalDuration.TimeSpan);
				Slider.Maximum = Audio.NaturalDuration.TimeSpan.TotalSeconds;
			}
		}
		/// <summary>
		/// 任务栏按下“暂停/播放”按钮
		/// </summary>
		private void PauseThumb_Click(object sender, EventArgs e)
		{
			_player.IsPlaying = !_player.IsPlaying;
		}
		/// <summary>
		/// 任务栏按下“下一首”按钮
		/// </summary>
		private void NextThumb_Click(object sender, EventArgs e)
		{
			_player.Skip();
		}
		/// <summary>
		/// 保存各种信息
		/// </summary>
		private void Window_Closed(object sender, EventArgs e)
		{
			if (_interopWindow != null)
				_interopWindow.Close();
			if (Audio != null)
				Audio.Close();
			if (notifyIcon != null)
				notifyIcon.Dispose();
			if (_player != null)
				_player.Dispose();
		}
		/// <summary>
		/// 更新密码
		/// </summary>
		private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
		{
			_player.Settings.User.Password = PbPassword.Password;
		}
		/// <summary>
		/// 登录
		/// </summary>
		private void ButtonLogOn_Click(object sender, RoutedEventArgs e)
		{
			_player.UserAssistant.LogOn(CaptchaText.Text);
		}
		/// <summary>
		/// 注销
		/// </summary>
		private void ButtonLogOff_Click(object sender, RoutedEventArgs e)
		{
			_player.UserAssistant.LogOff();
		}
		/// <summary>
		/// 验证码被点击
		/// </summary>
		private void ButtonRefreshCaptcha_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			_player.UserAssistant.Refresh();
		}

		/// <summary>
		/// 任务栏点击“喜欢”
		/// </summary>
		private void LikeThumb_Click(object sender, EventArgs e)
		{
			_player.IsLiked = !_player.IsLiked;
		}
		/// <summary>
		/// 主界面点击“不再播放”
		/// </summary>
		private void ButtonNever_Click(object sender, RoutedEventArgs e)
		{
			_player.Never();
		}
		/// <summary>
		/// 任务栏点击“不再播放”
		/// </summary>
		private void NeverThumb_Click(object sender, EventArgs e)
		{
			_player.Never();
		}

		/// <summary>
		/// 更换私人频道
		/// </summary>
		private void PersonalChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (PersonalChannels.SelectedItem != null)
				_player.CurrentChannel = (Channel)PersonalChannels.SelectedItem;
		}
		/// <summary>
		/// 更换公共频道
		/// </summary>
		private void PublicChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (PublicChannels.SelectedItem != null)
				_player.CurrentChannel = (Channel)PublicChannels.SelectedItem;
		}
		/// <summary>
		/// 更换DJ节目
		/// </summary>
		private void DjChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DjChannels.SelectedItem != null)
			{
				_player.CurrentDjCate = (Cate)DjCates.SelectedItem;
				_player.CurrentChannel = (Channel)DjChannels.SelectedItem;
			}
		}
		/// <summary>
		/// 当用户选择某DJ频道时，切换到DJ节目列表
		/// </summary>
		private void DjCates_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DjCates.SelectedItem != null)
			{
				DjChannels.ItemsSource = ((Cate)DjCates.SelectedItem).Channels;
				DjChannelClickStoryboard.Begin();
			}
		}
		/// <summary>
		/// 当用户在按钮“DJ兆赫”上点击时，去除DjCates的选择，以便再次点击“DJ兆赫”时仍然可以选择刚才的项目
		/// </summary>
		private void ButtonDjCates_Click(object sender, RoutedEventArgs e)
		{
			DjCates.SelectedItem = null;
		}
		/// <summary>
		/// 鼠标左键点击封面时滑动封面
		/// </summary>
		private void CoverGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);
			//if (!_player.Settings.SlideCoverWhenMouseMove)                //当设置为“鼠标移动到封面上时滑动”时点击封面仍然应该滑动，所以注释掉
			{
				Point leftLocation = e.GetPosition(LeftPanel);
				Debug.WriteLine("LeftPanel:" + leftLocation);
				HitTestResult leftResult = VisualTreeHelper.HitTest(LeftPanel, leftLocation);
				if (leftResult != null)
				{
					Debug.WriteLine("SlideRight");
					SlideCoverRightStoryboard.Begin();
					return;
				}
				Point rightLocation = e.GetPosition(RightPanel);
				Debug.WriteLine("RightPanel:" + rightLocation);
				HitTestResult rightResult = VisualTreeHelper.HitTest(RightPanel, rightLocation);
				if (rightResult != null)
				{
					Debug.WriteLine("SlideLeft");
					SlideCoverLeftStoryboard.Begin();
				}
			}
		}

		void SlideCoverRightTimer_Tick(object sender, EventArgs e)
		{
			SlideCoverRightStoryboard.Begin();
			_slideCoverRightTimer.Stop();
		}
		void SlideCoverLeftTimer_Tick(object sender, EventArgs e)
		{
			SlideCoverLeftStoryboard.Begin();
			_slideCoverLeftTimer.Stop();
		}

		/// <summary>
		/// 当鼠标移动时滑动封面
		/// </summary>
		private void CoverGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (_player.Settings.SlideCoverWhenMouseMove)
			{
				Point leftLocation = e.GetPosition(LeftPanel);
				Debug.WriteLine("LeftPanel:" + leftLocation);
				HitTestResult leftResult = VisualTreeHelper.HitTest(LeftPanel, leftLocation);
				if (leftResult != null)
				{
					Debug.WriteLine("SlideRight");
					_slideCoverRightTimer.Start();
					_slideCoverLeftTimer.Stop();
					return;
				}
				Point rightLocation = e.GetPosition(RightPanel);
				Debug.WriteLine("RightPanel:" + rightLocation);
				HitTestResult rightResult = VisualTreeHelper.HitTest(RightPanel, rightLocation);
				if (rightResult != null)
				{
					Debug.WriteLine("SlideLeft");
					_slideCoverLeftTimer.Start();
					_slideCoverRightTimer.Stop();
				}
			}
		}

		private void CoverGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			_slideCoverRightTimer.Stop();
			_slideCoverLeftTimer.Stop();
		}

		private void ButtonMinimize_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			this.WindowState = System.Windows.WindowState.Minimized;
		}

		private void ButtonToNotifyIcon_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			this.Visibility = Visibility.Hidden;
		}

		private void ButtonExit_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			this.Close();
		}

		private void Window_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
		{
			if (!_player.Settings.AlwaysShowNotifyIcon)
				notifyIcon.Visible = !this.IsVisible;
		}


		private void CheckBoxAlwaysShowNotifyIcon_IsCheckedChanged(object sender, RoutedEventArgs e)
		{
			if (CheckBoxAlwaysShowNotifyIcon.IsChecked == false)
				notifyIcon.Visible = !this.IsVisible;
			else notifyIcon.Visible = true;
		}

		private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void VisitSoftwareWebsite_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Process.Start("http://www.kfstorm.com/blog/doubanfm/");
		}

		private void VisitOfficialWebsite_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Process.Start("http://douban.fm/");
		}

		private void Search_Click(object sender, RoutedEventArgs e)
		{
			_player.MusicSearch.StartSearch(SearchText.Text);
		}

		private void PreviousPage_Click(object sender, RoutedEventArgs e)
		{
			_player.MusicSearch.PreviousPage();
		}

		private void NextPage_Click(object sender, RoutedEventArgs e)
		{
			_player.MusicSearch.NextPage();
		}

		private void SearchResultList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (SearchResultList.SelectedItem == null) return;
			Channel channel = ((SearchItem)SearchResultList.SelectedItem).GetChannel();
			if (channel != null) _player.CurrentChannel = channel;
		}

		private void Window_Activated(object sender, System.EventArgs e)
		{
			GradientStopCollection active = (GradientStopCollection)FindResource("ActiveShadowGradientStops");
			GradientStopCollection now = (GradientStopCollection)FindResource("ShadowGradientStops");
			now.Clear();
			foreach (GradientStop g in active)
				now.Add(g);
		}

		private void Window_Deactivated(object sender, EventArgs e)
		{
			GradientStopCollection inactive = (GradientStopCollection)FindResource("InactiveShadowGradientStops");
			GradientStopCollection now = (GradientStopCollection)FindResource("ShadowGradientStops");
			now.Clear();
			foreach (GradientStop g in inactive)
				now.Add(g);
		}

		#endregion

	}
}