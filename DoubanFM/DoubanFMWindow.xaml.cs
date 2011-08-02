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
        private Player player;
        /// <summary>
        /// 进度更新计时器
        /// </summary>
        private DispatcherTimer timer;
        /// <summary>
        /// 各种无法在XAML里直接启动的Storyboard
        /// </summary>
        private Storyboard BackgroundColorStoryboard, ShowCover1Storyboard, ShowCover2Storyboard, SlideCoverRightStoryboard, SlideCoverLeftStoryboard, ChangeSongInfoStoryboard, DjChannelClickStoryboard;
        /// <summary>
        /// 滑动封面的计时器
        /// </summary>
        private DispatcherTimer SlideCoverRightTimer, SlideCoverLeftTimer;
        /// <summary>
        /// 当前显示的封面
        /// </summary>
        private Image Cover;
        /// <summary>
        /// 是否已暂停
        /// </summary>
        private bool Paused;
        /// <summary>
        /// 私人与公共频道列表
        /// </summary>
        ObservableCollection<Channel> PersonalChannelsItem, PublicChannelsItem;
        /// <summary>
        /// DJ频道列表
        /// </summary>
        ObservableCollection<Cate> DjCatesItem;
        private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();
        private System.Windows.Forms.ToolStripItem notifyIcon_ShowWindow, notifyIcon_Heart, notifyIcon_Never, notifyIcon_PlayPause, notifyIcon_Next, notifyIcon_Exit;
        #endregion

        #region 初始化
        /// <summary>
        /// 构造函数
        /// </summary>
        public DoubanFMWindow()
        {
            InitializeComponent();

            notifyIcon.Visible = false;
            notifyIcon.Icon = Properties.Resources.NotifyIcon;
            notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler((s, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    this.Visibility = Visibility.Visible;
                    this.Activate();
                }
            });
            System.Windows.Forms.ContextMenuStrip notifyIconMenu = new System.Windows.Forms.ContextMenuStrip();
            notifyIcon.Text = "豆瓣电台";
            notifyIcon.ContextMenuStrip = notifyIconMenu;

            notifyIconMenu.Items.Add("显示窗口");
            notifyIcon_ShowWindow = notifyIconMenu.Items[notifyIconMenu.Items.Count - 1];
            notifyIcon_ShowWindow.Click += new EventHandler((s, e) => { this.Visibility = Visibility.Visible; });
            notifyIconMenu.Items.Add("-");
            notifyIconMenu.Items.Add("加红心");
            notifyIcon_Heart = notifyIconMenu.Items[notifyIconMenu.Items.Count - 1];
            notifyIcon_Heart.Click += new EventHandler((s, e) => { LikeOrUnlike(); });
            notifyIconMenu.Items.Add("不再播放");
            notifyIcon_Never = notifyIconMenu.Items[notifyIconMenu.Items.Count - 1];
            notifyIcon_Never.Click += new EventHandler((s, e) => { Never(); });
            notifyIconMenu.Items.Add("-");
            notifyIconMenu.Items.Add("暂停");
            notifyIcon_PlayPause = notifyIconMenu.Items[notifyIconMenu.Items.Count - 1];
            notifyIcon_PlayPause.Click += new EventHandler((s, e) =>
            {
                System.Windows.Forms.ToolStripItem sender = (System.Windows.Forms.ToolStripItem)s;
                if (sender.Text == "播放")
                    Play();
                else
                    Pause();
            });
            notifyIconMenu.Items.Add("下一首");
            notifyIcon_Next = notifyIconMenu.Items[notifyIconMenu.Items.Count - 1];
            notifyIcon_Next.Click += new EventHandler((s, e) => { Next(); });
            notifyIconMenu.Items.Add("-");
            notifyIconMenu.Items.Add("退出");
            notifyIcon_Exit = notifyIconMenu.Items[notifyIconMenu.Items.Count - 1];
            notifyIcon_Exit.Click += new EventHandler((s, e) => { this.Close(); });

            player = (Player)FindResource("Player");
            player.DjChannelPlayingEnded += new EventHandler(player_DjChannelPlayingEnded);
            PbPassword.Password = player.settings.User.Password;
            if (player.settings.SlideCoverWhenMouseMove == false)
                RadioButtonSlideCoverWhenClick.IsChecked = true;
            //ApplySettings();
            new ColorFunctions();
            BackgroundColorStoryboard = (Storyboard)FindResource("BackgroundColorStoryboard");
            ShowCover1Storyboard = (Storyboard)FindResource("ShowCover1Storyboard");
            ShowCover2Storyboard = (Storyboard)FindResource("ShowCover2Storyboard");
            SlideCoverRightStoryboard = (Storyboard)FindResource("SlideCoverRightStoryboard");
            SlideCoverLeftStoryboard = (Storyboard)FindResource("SlideCoverLeftStoryboard");
            ChangeSongInfoStoryboard = (Storyboard)FindResource("ChangeSongInfoStoryboard");
            DjChannelClickStoryboard = (Storyboard)FindResource("DjChannelClickStoryboard");
            Cover = Cover1;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(1000000);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            SlideCoverRightTimer = new DispatcherTimer();
            SlideCoverRightTimer.Interval = new TimeSpan(5000000);
            SlideCoverRightTimer.Tick += new EventHandler(SlideCoverRightTimer_Tick);
            SlideCoverLeftTimer = new DispatcherTimer();
            SlideCoverLeftTimer.Interval = new TimeSpan(5000000);
            SlideCoverLeftTimer.Tick += new EventHandler(SlideCoverLeftTimer_Tick);

            Thread thread = new Thread(new ThreadStart(new Action(() =>
                {
                    player.Initialize();
                    ShowChannels();
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        bool selected = false;
                        if (player.settings.RememberLastChannel && player.settings.LastChannel != null)
                        {
                            Channel firstChannel = player.settings.LastChannel;
                            if (firstChannel.Id == "0" && player.LoggedOn)
                            {
                                PersonalChannels.SelectedIndex = 0;
                                PersonalClickStoryboard.Begin();
                                selected = true;
                            }
                            else if (firstChannel.Id == "dj")
                            {
                                foreach (Cate djcate in DjCates.Items)
                                {
                                    foreach (Channel channel in djcate.Channels)
                                    {
                                        if (channel.pid == firstChannel.pid)
                                        {
                                            DjCates.SelectedItem = djcate;
                                            DjChannels.SelectedItem = channel;
                                            DjChannelClickStoryboard.Begin();
                                            selected = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (Channel channel in PublicChannels.Items)
                                    if (channel.Id == firstChannel.Id)
                                    {
                                        PublicChannels.SelectedItem = channel;
                                        PublicClickStoryboard.Begin();
                                        selected = true;
                                    }
                            }
                        }
                        if (!selected)
                            if (PublicChannels.Items.Count > 0)
                            {
                                PublicChannels.SelectedIndex = 0;
                                PublicClickStoryboard.Begin();
                            }
                        if (player.LoggedOn)
                        {
                            LogOnPanel.Visibility = Visibility.Hidden;
                            LogOffPanel.Visibility = Visibility.Visible;
                        }
                        Thread thread2 = new Thread(new ThreadStart(() =>
                            {
                                player.Play();
                                this.Dispatcher.BeginInvoke(new Action(() => { Audio.Play(); }));
                            }));
                        thread2.IsBackground = true;
                        thread2.Start();
                    }));
                })));
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 显示频道列表
        /// </summary>
        private void ShowChannels()
        {
            PersonalChannelsItem = new ObservableCollection<Channel>();
            if (player.LoggedOn)
            {
                this.Dispatcher.Invoke(new Action(() => { ButtonPersonal.IsEnabled = true; }));
                foreach (Cate cate in player.channelinfo.Personal)
                {
                    foreach (Channel channel in cate.Channels)
                    {
                        PersonalChannelsItem.Add(channel);
                    }
                }
            }
            else
                this.Dispatcher.Invoke(new Action(() => { ButtonPersonal.IsEnabled = false; }));
            PublicChannelsItem = new ObservableCollection<Channel>();
            foreach (Cate cate in player.channelinfo.Public)
            {
                foreach (Channel channel in cate.Channels)
                {
                    PublicChannelsItem.Add(channel);
                }
            }
            DjCatesItem = new ObservableCollection<Cate>();
            foreach (Cate djcate in player.channelinfo.Dj)
            {
                DjCatesItem.Add(djcate);
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                PersonalChannels.ItemsSource = PersonalChannelsItem;
                PublicChannels.ItemsSource = PublicChannelsItem;
                DjCates.ItemsSource = DjCatesItem;
            }));
        }
        #endregion

        #region 播放器控制
        /// <summary>
        /// 用户点击下一首。异步方法。不保证一定成功。
        /// </summary>
        public void Next()
        {
            if (player.CurrentSong == null) return;
            Audio.Stop();
            Thread thread = new Thread(new ThreadStart(new Action(() =>
                {
                    player.Skip();
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Update();
                    }));
                })));
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 播放结束后下一首。异步方法。不保证一定成功。
        /// </summary>
        public void NaturalNext()
        {
            Thread thread = new Thread(new ThreadStart(new Action(() =>
            {
                player.SongFinished();
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Update();
                }));
            })));
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 暂停。不保证一定成功。
        /// </summary>
        public void Pause()
        {
            if (CheckBoxPause.IsChecked == false)
                CheckBoxPause.IsChecked = true;
            PauseThumb.ImageSource = (ImageSource)FindResource("PlayThumbImage");
            Paused = true;
            Audio.Pause();
            Thread thread = new Thread(new ThreadStart(() =>
            {
                player.Pause();
                this.Dispatcher.BeginInvoke(new Action(() => { notifyIcon_PlayPause.Text = "播放"; }));
            }));
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 播放。不保证一定成功。
        /// </summary>
        public void Play()
        {
            if (CheckBoxPause.IsChecked == true)
                CheckBoxPause.IsChecked = false;
            PauseThumb.ImageSource = (ImageSource)FindResource("PauseThumbImage");
            Paused = false;
            Thread thread = new Thread(new ThreadStart(() =>
                {
                    Song LastSong = player.CurrentSong;
                    player.Play();
                    this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (LastSong != player.CurrentSong)
                                Update();
                            Audio.Play();
                            notifyIcon_PlayPause.Text = "暂停";
                            Debug.WriteLine("Audio.Play()");
                        }));
                }));
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 标记喜欢或不喜欢。
        /// </summary>
        public void LikeOrUnlike()
        {
            if (player.CurrentSong == null)
                return;
            bool CurrentLike = !player.CurrentSong.like;
            if (CurrentLike != CheckBoxLike.IsChecked)
                CheckBoxLike.IsChecked = CurrentLike;
            if (CurrentLike)
            {
                LikeThumb.ImageSource = (ImageSource)FindResource("LikeThumbImage");
                notifyIcon_Heart.Text = "去红心";
            }
            else
            {
                LikeThumb.ImageSource = (ImageSource)FindResource("UnlikeThumbImage");
                notifyIcon_Heart.Text = "加红心";
            }
            Thread thread = new Thread(new ThreadStart(() =>
                {
                    player.LikeOrUnlike();
                }));
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 标记不再播放。
        /// </summary>
        public void Never()
        {
            if (player.CurrentSong == null)
                return;
            Audio.Stop();
            Thread thread = new Thread(new ThreadStart(() =>
                {
                    player.Never();
                    this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Update();
                        }));
                }));
            thread.IsBackground = true;
            thread.Start();
        }
        #endregion

        #region 其他
        /// <summary>
        /// 更新界面内容，主要是音乐信息。换音乐时自动调用。
        /// </summary>
        private void Update()
        {
            if (player.CurrentSong == null)
                return;
            Song song = player.CurrentSong;
            if (song == null) return;
            ChangeCover(song);
            try
            {
                //Audio.Stop();
                Audio.Source = new Uri(song.url);
                if (Paused) Audio.Pause();
                else Audio.Play();
                Audio.IsMuted = !Audio.IsMuted;
                Audio.IsMuted = !Audio.IsMuted;
            }
            catch { }
            ((StringAnimationUsingKeyFrames)ChangeSongInfoStoryboard.Children[1]).KeyFrames[0].Value = song.title;
            ((StringAnimationUsingKeyFrames)ChangeSongInfoStoryboard.Children[2]).KeyFrames[0].Value = song.artist;
            ((StringAnimationUsingKeyFrames)ChangeSongInfoStoryboard.Children[3]).KeyFrames[0].Value = song.albumtitle;
            ChangeSongInfoStoryboard.Begin();
            string stringA = song.title + " - " + song.artist;
            string stringB = "    豆瓣电台 - " + player.Channel.Name;
            this.Title = stringA + stringB;
            if (this.Title.Length <= 63)        //Windows限制托盘图标的提示信息最长为63个字符
                notifyIcon.Text = this.Title;
            else
                notifyIcon.Text = stringA.Substring(0, 63 - stringB.Length) + stringB;
            ChannelTextBlock.Text = player.Channel.Name;
            TotalTime.Content = TimeSpanToStringConverter.QuickConvert(new TimeSpan(0, 0, song.length));
            CurrentTime.Content = TimeSpanToStringConverter.QuickConvert(new TimeSpan(0));
            Slider.Minimum = 0;
            Slider.Maximum = song.length;
            Slider.Value = 0;
            CheckBoxLike.IsChecked = song.like;
            if (player.Channel.Id == "0")
            {
                CheckBoxLike.IsEnabled = true;
                if (song.like)
                {
                    LikeThumb.ImageSource = (ImageSource)FindResource("LikeThumbImage");
                    notifyIcon_Heart.Text = "去红心";
                }
                else
                {
                    LikeThumb.ImageSource = (ImageSource)FindResource("UnlikeThumbImage");
                    notifyIcon_Heart.Text = "加红心";
                }
                ButtonNever.IsEnabled = true;
                NeverThumb.IsEnabled = true;
                NeverThumb.ImageSource = (ImageSource)FindResource("NeverThumbImage");
            }
            else if (player.Channel.Id != "dj")
            {
                CheckBoxLike.IsEnabled = true;
                if (song.like)
                    LikeThumb.ImageSource = (ImageSource)FindResource("LikeThumbImage");
                else
                    LikeThumb.ImageSource = (ImageSource)FindResource("UnlikeThumbImage");
                ButtonNever.IsEnabled = false;
                NeverThumb.IsEnabled = false;
                NeverThumb.ImageSource = (ImageSource)FindResource("NeverThumbImage_Disabled");
            }
            else
            {
                CheckBoxLike.IsEnabled = false;
                LikeThumb.ImageSource = (ImageSource)FindResource("LikeThumbImage_Disabled");
                ButtonNever.IsEnabled = false;
                NeverThumb.IsEnabled = false;
                NeverThumb.ImageSource = (ImageSource)FindResource("NeverThumbImage_Disabled");
            }
        }
        /// <summary>
        /// 更改封面
        /// </summary>
        /// <param name="song">音乐</param>
        void ChangeCover(Song song)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage(new Uri(song.picture));
#if DEBUG
                if (System.Environment.GetCommandLineArgs().Contains("-LocalMusic"))
                    bitmap_DownloadCompleted(bitmap, null);
                else
#endif
                {
                    bitmap.DownloadCompleted += new EventHandler(bitmap_DownloadCompleted);
                    bitmap.DownloadFailed += new EventHandler<ExceptionEventArgs>(bitmap_DownloadFailed);
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
            Thread thread = new Thread(new ThreadStart(() =>
                {
                    Color to = ColorFunctions.GetImageColorForBackground(NewCover);
                    this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            animation.To = to;
                            BackgroundColorStoryboard.Begin();
                        }));
                }));
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 更换封面。封面加载成功时自动调用
        /// </summary>
        /// <param name="NewCover">新封面</param>
        void SwitchCover(BitmapImage NewCover)
        {
            if (Cover == Cover1)
            {
                Cover2.Source = NewCover;
                Cover = Cover2;
                ShowCover2Storyboard.Begin();
            }
            else
            {
                Cover1.Source = NewCover;
                Cover = Cover1;
                ShowCover1Storyboard.Begin();
            }
        }
        #endregion

        #region 事件响应
        /// <summary>
        /// 封面加载失败时自动调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bitmap_DownloadFailed(object sender, ExceptionEventArgs e)
        {
            if (((BitmapImage)sender).UriSource.AbsoluteUri.ToString() == new Uri(player.CurrentSong.picture).AbsoluteUri.ToString())
            {
                BitmapImage bitmap = new BitmapImage(new Uri("pack://application:,,,/DoubanFM;component/Images/DoubanFM_NoCover.png"));
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        ChangeBackground(bitmap);
                        SwitchCover(bitmap);
                    }
                    catch { }
                }));

            }
        }
        /// <summary>
        /// 封面加载成功时自动调用
        /// </summary>
        /// <param name="sender">封面BitmapImage</param>
        /// <param name="e"></param>
        void bitmap_DownloadCompleted(object sender, EventArgs e)
        {
            if (((BitmapImage)sender).UriSource.AbsoluteUri == new Uri(player.CurrentSong.picture).AbsoluteUri)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        ChangeBackground((BitmapImage)sender);
                        SwitchCover((BitmapImage)sender);
                    }
                    catch { }
                }));
            }
        }
        /// <summary>
        /// 主界面中按下“下一首”按钮时自动调用
        /// </summary>
        /// <param name="sender">按钮</param>
        /// <param name="e"></param>
        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }
        /// <summary>
        /// 音乐播放结束时自动调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Audio_MediaEnded(object sender, RoutedEventArgs e)
        {
            NaturalNext();
        }
        /// <summary>
        /// 音乐遇到错误时自动调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Audio_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            NaturalNext();
        }
        /// <summary>
        /// 计时器响应函数，用于更新时间信息。自动调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Tick(object sender, EventArgs e)
        {
            CurrentTime.Content = TimeSpanToStringConverter.QuickConvert(Audio.Position);
            Slider.Value = Audio.Position.TotalSeconds;
        }
        /// <summary>
        /// 暂停按钮被按下时自动调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxPause_Checked(object sender, RoutedEventArgs e)
        {
            Pause();
        }
        /// <summary>
        /// 播放按钮被按下时自动调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxPause_Unchecked(object sender, RoutedEventArgs e)
        {
            Play();
        }
        /// <summary>
        /// 修正音乐总时间。音乐加载完成时自动调用。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// 任务栏“暂停/播放”按钮按下时自动调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PauseThumb_Click(object sender, EventArgs e)
        {
            if (Paused) Play();
            else Pause();
        }
        /// <summary>
        /// 任务栏“下一首”按钮按下时自动调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextThumb_Click(object sender, EventArgs e)
        {
            Next();
        }
        /// <summary>
        /// 保存各种信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            Audio.Stop();
            notifyIcon.Dispose();
            player.SaveSettings();
            if (!player.settings.AutoLogOnNextTime && player.LoggedOn)
                player.LogOff();
            player.SaveCookies();
        }
        /// <summary>
        /// 更新密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            player.settings.User.Password = PbPassword.Password;
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLogOn_Click(object sender, RoutedEventArgs e)
        {
            //SaveSettings();
            if (player.LoggedOn == false)
            {
                LoggingOnPanel.Visibility = Visibility.Visible;
                LogOnPanel.Visibility = Visibility.Hidden;
                LogOnFailedHint.Visibility = Visibility.Hidden;
                string text = CaptchaText.Text;
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    player.LogOn(text);
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        LoggingOnPanel.Visibility = Visibility.Hidden;
                        if (player.LoggedOn)
                        {
                            LogOnFailedHint.Visibility = Visibility.Hidden;
                            ShowChannels();
                            LogOffPanel.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            LogOnPanel.Visibility = Visibility.Visible;
                            LogOnFailedHint.Visibility = Visibility.Visible;
                            RefreshCaptcha();
                        }
                    }));
                }));
                thread.IsBackground = true;
                thread.Start();
            }
        }
        /// <summary>
        /// 根据情况显示帐号面板的内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlPanel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ControlPanel.SelectedItem == Account && player.LoggedOn == false)
            {
                LogOnPanel.Visibility = Visibility.Visible;
                LogOffPanel.Visibility = Visibility.Hidden;
                RefreshCaptcha();
            }
            else
            {
                LogOnPanel.Visibility = Visibility.Hidden;
                LogOffPanel.Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// 刷新验证码
        /// </summary>
        private void RefreshCaptcha()
        {
            CaptchaGrid.Visibility = Visibility.Collapsed;
            Thread thread = new Thread(new ThreadStart(() =>
            {
                string CaptchaID = player.GetNewCaptchaId();
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    CaptchaText.Text = "";
                    if (CaptchaID != null || CaptchaID != string.Empty)
                    {
                        BitmapImage CaptchaImage = new BitmapImage(new Uri("http://www.douban.com/misc/captcha?id=" + CaptchaID + "&amp;size=s"));
                        CaptchaImage.DownloadCompleted += new EventHandler(CaptchaImage_DownloadCompleted);
                        Captcha.Source = CaptchaImage;
                    }
                }));
            }));
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 验证码图标下载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CaptchaImage_DownloadCompleted(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                CaptchaGrid.Visibility = Visibility.Visible;
            }));
        }
        /// <summary>
        /// 注销
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLogOff_Click(object sender, RoutedEventArgs e)
        {
            if (player.LoggedOn)
            {
                LoggingOffPanel.Visibility = Visibility.Visible;
                LogOffPanel.Visibility = Visibility.Hidden;
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    player.LogOff();
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        LoggingOffPanel.Visibility = Visibility.Hidden;
                        if (!player.LoggedOn)
                        {
                            LogOnPanel.Visibility = Visibility.Visible;
                            ShowChannels();
                            RefreshCaptcha();
                        }
                        else
                            LogOffPanel.Visibility = Visibility.Visible;
                    }));
                }));
                thread.IsBackground = true;
                thread.Start();
            }
        }
        /// <summary>
        /// 验证码被点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonRefreshCaptcha_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RefreshCaptcha();
        }
        /// <summary>
        /// 主界面点击"喜欢"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxLike_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LikeOrUnlike();
        }
        /// <summary>
        /// 任务栏点击“喜欢”
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LikeThumb_Click(object sender, EventArgs e)
        {
            LikeOrUnlike();
        }
        /// <summary>
        /// 主界面点击“不再播放”
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonNever_Click(object sender, RoutedEventArgs e)
        {
            Never();
        }
        /// <summary>
        /// 任务栏点击“不再播放”
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NeverThumb_Click(object sender, EventArgs e)
        {
            Never();
        }

        /// <summary>
        /// 更换私人频道。私人频道选择改变时自动调用。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PersonalChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PersonalChannels.SelectedItem != null)
            {
                Audio.Stop();
                Channel channel = (Channel)PersonalChannels.SelectedItem;
                PublicChannels.SelectedItem = null;
                DjCates.SelectedItem = null;
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    player.Channel = channel;
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        Update();
                    }));
                }));
                thread.IsBackground = true;
                thread.Start();
            }
        }
        /// <summary>
        /// 更换公共频道。公共频道选择改变时自动调用。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PublicChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PublicChannels.SelectedItem != null)
            {
                Audio.Stop();
                Channel channel = (Channel)PublicChannels.SelectedItem;
                PersonalChannels.SelectedItem = null;
                DjCates.SelectedItem = null;
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    player.Channel = channel;
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        Update();
                    }));
                }));
                thread.IsBackground = true;
                thread.Start();
            }
        }
        /// <summary>
        /// 更换DJ节目。DJ节目选择改变时自动调用。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DjChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DjChannels.SelectedItem != null)
            {
                Audio.Stop();
                Channel channel = (Channel)DjChannels.SelectedItem;
                PersonalChannels.SelectedItem = null;
                PublicChannels.SelectedItem = null;
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    player.Channel = channel;
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        Update();
                    }));
                }));
                thread.IsBackground = true;
                thread.Start();
            }
        }

        /// <summary>
        /// 当用户在某DJ频道上点击鼠标左键时，切换到DJ节目列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DjCates_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock)
                DjChannelClickStoryboard.Begin();
        }
        /// <summary>
        /// DJ节目播放完毕时，切换到下一个DJ节目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void player_DjChannelPlayingEnded(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (Cate djcate in DjCates.Items)
                    {
                        int index = djcate.Channels.ToList().IndexOf(player.Channel);
                        if (index != -1)
                        {
                            DjCates.SelectedItem = djcate;
                            if (DjChannels.Items.Count != 0)
                                DjChannels.SelectedIndex = (index + 1) % DjChannels.Items.Count;
                        }
                    }
                }));
        }
        /// <summary>
        /// 鼠标左键点击封面时滑动封面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoverGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            //if (!player.settings.SlideCoverWhenMouseMove)
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

        /// <summary>
        /// Handles the Tick event of the SlideCoverRightTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void SlideCoverRightTimer_Tick(object sender, EventArgs e)
        {
            SlideCoverRightStoryboard.Begin();
            SlideCoverRightTimer.Stop();
        }
        /// <summary>
        /// Handles the Tick event of the SlideCoverLeftTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void SlideCoverLeftTimer_Tick(object sender, EventArgs e)
        {
            SlideCoverLeftStoryboard.Begin();
            SlideCoverLeftTimer.Stop();
        }

        /// <summary>
        /// 当鼠标移动时滑动封面
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void CoverGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (player.settings.SlideCoverWhenMouseMove)
            {
                Point leftLocation = e.GetPosition(LeftPanel);
                Debug.WriteLine("LeftPanel:" + leftLocation);
                HitTestResult leftResult = VisualTreeHelper.HitTest(LeftPanel, leftLocation);
                if (leftResult != null)
                {
                    Debug.WriteLine("SlideRight");
                    SlideCoverRightTimer.Start();
                    SlideCoverLeftTimer.Stop();
                    return;
                }
                Point rightLocation = e.GetPosition(RightPanel);
                Debug.WriteLine("RightPanel:" + rightLocation);
                HitTestResult rightResult = VisualTreeHelper.HitTest(RightPanel, rightLocation);
                if (rightResult != null)
                {
                    Debug.WriteLine("SlideLeft");
                    SlideCoverLeftTimer.Start();
                    SlideCoverRightTimer.Stop();
                }
            }
        }

        /// <summary>
        /// Handles the MouseLeave event of the CoverGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void CoverGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SlideCoverRightTimer.Stop();
            SlideCoverLeftTimer.Stop();
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
            notifyIcon.Visible = !this.IsVisible;
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void VisitSoftwareWebsite_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start("http://kfstorm.wordpress.com/doubanfm/");
        }

        private void VisitOfficialWebsite_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start("http://douban.fm/");
        }
        #endregion

    }

}