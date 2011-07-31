using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Diagnostics;

namespace DoubanFM.Core
{
    /// <summary>
    /// 播放器
    /// </summary>
    public class Player
    {
        private Channel channel { get; set; }
        /// <summary>
        /// 获取频道列表
        /// </summary>
        public ChannelInfo channelinfo { get; private set; }
        /// <summary>
        /// 播放器设置
        /// </summary>
        public Settings settings { get; set; }
        /// <summary>
        /// 获取验证码ID
        /// </summary>
        public string CaptchaId { get; private set; }
        /// <summary>
        /// Gets a value indicating whether [logged on].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [logged on]; otherwise, <c>false</c>.
        /// </value>
        public bool LoggedOn { get; private set; }
        /// <summary>
        /// 获取注销的链接
        /// </summary>
        public string LogOffLink { get; private set; }
        /// <summary>
        /// 已播放音乐的列表
        /// </summary>
        private Queue<Song> PlayedSongs = new Queue<Song>();
        /// <summary>
        /// 待播放音乐的列表
        /// </summary>
        private Queue<Song> PlayListSongs = new Queue<Song>();
        /// <summary>
        /// 已播放音乐的字符串表示
        /// </summary>
        private Queue<string> PlayedSongsString = new Queue<string>();
        /// <summary>
        /// 获取当前播放的音乐
        /// </summary>
        public Song CurrentSong { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="Player"/> is initialized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if initialized; otherwise, <c>false</c>.
        /// </value>
        public bool Initialized { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is playing.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is playing; otherwise, <c>false</c>.
        /// </value>
        public bool IsPlaying { get; private set; }
        /// <summary>
        /// Gets the pause time.
        /// </summary>
        private DateTime PauseTime { get; set; }
        /// <summary>
        /// 获取或设置当前频道
        /// </summary>
        public Channel Channel
        {
            get
            {
                return channel;
            }
            set
            {
                lock (this)
                {
                    channel = value;
                    settings.LastChannel = channel;
                    if (channel.Id != "dj")
                    {
                        if (CurrentSong != null)
                            Skip();
                        else
                            NewPlayList();
                    }
                    else
                    {
                        NewPlayList();
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when [dj channel playing ended].
        /// </summary>
        public event EventHandler DjChannelPlayingEnded;
        /// <summary>
        /// Raises the DjChannelPlayingEnded event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void RaiseDjChannelPlayingEnded(EventArgs e)
        {
            if (DjChannelPlayingEnded != null)
                DjChannelPlayingEnded(this, e);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// </summary>
        public Player()
        {
            LoadSettings();
            LoadCookies();
        }
        /// <summary>
        /// 读取偏好设置
        /// </summary>
        /// <returns>读取成功与否</returns>
        public bool LoadSettings()
        {
            try
            {
                using (FileStream stream = File.OpenRead(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\K.F.Storm\豆瓣电台\settings.dat"))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    settings = (Settings)formatter.Deserialize(stream);
                }
                settings.User.Password = Encryption.Decrypt(settings.User.Password);
            }
            catch
            {
                settings = new Settings();
                return false;
            }
            if (settings.User == null)
                settings.User = new UserInfo("", "");
            return true;
        }
        /// <summary>
        /// 保存偏好设置
        /// </summary>
        /// <returns>保存成功与否</returns>
        public bool SaveSettings()
        {
            string tempPassword = settings.User.Password;
            if (!settings.RememberPassword)
                settings.User.Password = "";
            try
            {
                settings.User.Password = Encryption.Encrypt(settings.User.Password);
                string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\K.F.Storm\豆瓣电台";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using (FileStream stream = File.OpenWrite(dir + @"\settings.dat"))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, settings);
                }
            }

            catch
            {
                settings.User.Password = tempPassword;
                return false;
            }
            settings.User.Password = tempPassword;
            return true;
        }
        /// <summary>
        /// 加载Cookies
        /// </summary>
        /// <returns>成功与否</returns>
        public static bool LoadCookies()
        {
            return ConnectionBase.LoadCookies();
        }
        /// <summary>
        /// 保存Cookies
        /// </summary>
        /// <returns>成功与否</returns>
        public bool SaveCookies()
        {
            return ConnectionBase.SaveCookies();
        }
        /// <summary>
        /// 播放器初始化
        /// </summary>
        public void Initialize()
        {
            if (Initialized) return;
#if DEBUG
            if (System.Environment.GetCommandLineArgs().Contains("-LocalMusic"))
                channelinfo = ChannelInfo.GetChannelInfo();
            else
#endif
            lock (this)
            {
                while (channelinfo == null)
                {
                    string file = new ConnectionBase().Get("http://douban.fm");
                    //string file = new StreamReader(File.OpenRead(@"F:\K.F.Storm\Documents\Visual Studio 2010\Projects\DoubanFM\Research\douban.fm(Loggedout).html"), Encoding.Default).ReadToEnd();
                    //channelinfo = ChannelInfo.GetChannelInfo(file);
                    //System.Runtime.Serialization.Json.DataContractJsonSerializer ser = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(ChannelInfo));
                    //using (FileStream fs = File.OpenWrite("LocalMusic_ChannelInfo.dat"))
                    //    ser.WriteObject(fs, channelinfo);
                    HtmlAnalysis ha = new HtmlAnalysis(file);
                    channelinfo = ha.GetChannelInfo();
                    LoggedOn = ha.IsLoggedOn();
                    LogOffLink = ha.GetLogOffLink();
                }
            }
            Initialized = true;
            PauseTime = DateTime.Now;
            return;
        }
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <returns>登录操作成功与否</returns>
        public bool LogOn(string captchaText)
        {
            lock (this)
            {
                if (LoggedOn) return true;
                string PostData = "source=radio&form_email=" + settings.User.Username + "&form_password=" + settings.User.Password;
                if (CaptchaId != null)
                    PostData += "&captcha-solution=" + captchaText + "&captcha-id=" + CaptchaId;
                if (settings.AutoLogOnNextTime)
                    PostData += "&remember=on";
                else
                    PostData += "&remember=off";
                //PostData += "&user_login=%E7%99%BB%E5%BD%95";
                string file = new ConnectionBase().Post("https://www.douban.com/accounts/login", "http://www.douban.com/accounts/login?source=radio", Encoding.Default.GetBytes(PostData));
                HtmlAnalysis ha = new HtmlAnalysis(file);
                LoggedOn = ha.IsLoggedOn();
                LogOffLink = ha.GetLogOffLink();
                return LoggedOn;
            }
        }
        /// <summary>
        /// 获取验证码ID
        /// </summary>
        /// <returns>ID，若没有，则返回0</returns>
        public string GetNewCaptchaId()
        {
            lock (this)
            {
                string file = new ConnectionBase().Get("http://www.douban.com/accounts/login");
                HtmlAnalysis ha = new HtmlAnalysis(file);
                CaptchaId = ha.GetCaptchaID();
                return CaptchaId;
            }
        }
        /// <summary>
        /// 注销
        /// </summary>
        /// <returns>成功与否</returns>
        public bool LogOff()
        {
            lock (this)
            {
                if (!LoggedOn) return true;
                new ConnectionBase().Get(LogOffLink);
                string file = new ConnectionBase().Get("http://douban.fm");
                LoggedOn = new HtmlAnalysis(file).IsLoggedOn();
                return !LoggedOn;
            }
        }
        /// <summary>
        /// 增加已播放音乐的记录
        /// </summary>
        /// <param name="type">操作类型</param>
        private void AppendPlayedSongs(string type)
        {
            if (CurrentSong == null) return;
            PlayedSongs.Enqueue(CurrentSong);
            PlayedSongsString.Enqueue("|" + CurrentSong.sid + ":" + type);
            while (PlayedSongs.Count > 20)
            {
                PlayedSongs.Dequeue();
                PlayedSongsString.Dequeue();
            }
        }
        /// <summary>
        /// 将已播放音乐的记录转换成字符串，用于h参数
        /// </summary>
        /// <returns>字符串</returns>
        private string PlayedSongsToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in PlayedSongsString)
                sb.Append(s);
            return sb.ToString();
        }
        /// <summary>
        /// 更换播放列表队列
        /// </summary>
        /// <param name="pl">新播放列表</param>
        private void ChangePlayListSongs(PlayList pl)
        {
            var songs = pl.Songs;
            PlayListSongs.Clear();
            foreach (Song song in songs)
                PlayListSongs.Enqueue(song);
        }
        /// <summary>
        /// 更换当前播放的音乐
        /// </summary>
        private void ChangeCurrentSong()
        {
            if (PlayListSongs.Count == 0 && Channel.Id == "dj")
                RaiseDjChannelPlayingEnded(EventArgs.Empty);
            else
            {
                CheckPlayListSongsLength();
                CurrentSong = PlayListSongs.Dequeue();
                CheckPlayListSongsLength();
                if (CurrentSong.subtype == "T")
                    SongFinished();
            }
        }
        /// <summary>
        /// 检查播放列表的长度，若为0，则请求一个新播放列表
        /// type=p
        /// PlayOut
        /// </summary>
        private void CheckPlayListSongsLength()
        {
            if (!Initialized) return;
            if (PlayListSongs.Count == 0 && Channel.Id != "dj")
            {
                PlayList pl = null;
                do
                {
                    pl = PlayList.GetNewPlayList(CurrentSong.sid, Channel, "p", PlayedSongsToString());
                } while (pl.Songs.Count == 0);
                ChangePlayListSongs(pl);
            }
        }
        /// <summary>
        /// 歌曲自然播放完毕，添加播放记录或请求新播放列表
        /// type=e
        /// Played
        /// </summary>
        public void SongFinished()
        {
            lock (this)
            {
                if (!Initialized) return;
#if DEBUG
                if (System.Environment.GetCommandLineArgs().Contains("-LocalMusic"))
                {
                    CurrentSong = PlayListSongs.ElementAt(new Random().Next(PlayListSongs.Count));
                    return;
                }
#endif
                AppendPlayedSongs("e");
                string url = "http://douban.fm/j/mine/playlist?sid=" + CurrentSong.sid + "&channel=" + Channel.Id + "&type=" + "e";
                string Result = new ConnectionBase().Get(url, "*/*", "http://douban.fm");
                ChangeCurrentSong();
            }
        }
        /// <summary>
        /// 跳过当前歌曲
        /// type=s
        /// Skip
        /// </summary>
        public void Skip()
        {
            if (!Initialized) return;
#if DEBUG
            if (System.Environment.GetCommandLineArgs().Contains("-LocalMusic"))
            {
                CurrentSong = PlayListSongs.ElementAt(new Random().Next(PlayListSongs.Count));
                return;
            }
#endif
            lock (this)
            {
                AppendPlayedSongs("s");
                PlayList pl = null;
                do
                {
                    pl = PlayList.GetNewPlayList(CurrentSong.sid, Channel, "s", PlayedSongsToString());
                } while (Channel.Id != "dj" && pl.Songs.Count == 0);
                if (Channel.Id != "dj")
                    ChangePlayListSongs(pl);
                ChangeCurrentSong();
            }
        }
        /// <summary>
        /// 获取全新的播放列表
        /// type=n
        /// New
        /// </summary>
        public void NewPlayList()
        {
            lock (this)
            {
                if (!Initialized) return;
                PlayList pl = null;
                do
                {
                    pl = PlayList.GetNewPlayList(null, Channel, "n", PlayedSongsToString());
                } while (pl.Songs.Count == 0);
                ChangePlayListSongs(pl);
                ChangeCurrentSong();
            }
        }
        /// <summary>
        /// 对当前音乐标记喜欢或不喜欢
        /// type=r or type=u
        /// Like or Unlike
        /// </summary>
        public void LikeOrUnlike()
        {
            lock (this)
            {
                if (!Initialized) return;
                if (CurrentSong == null) return;
                if (Channel.Id == "dj") return;
                CurrentSong.like = !CurrentSong.like;
                PlayList pl = null;
                do
                {
                    if (CurrentSong.like)
                    {
                        AppendPlayedSongs("r");
                        pl = PlayList.GetNewPlayList(CurrentSong.sid, Channel, "r", PlayedSongsToString());
                    }
                    else
                    {
                        AppendPlayedSongs("u");
                        pl = PlayList.GetNewPlayList(CurrentSong.sid, Channel, "u", PlayedSongsToString());
                    }
                } while (pl.Songs.Count == 0);
                ChangePlayListSongs(pl);
            }
        }
        /// <summary>
        /// 对当前音乐标记不再播放
        /// type=b
        /// Ban
        /// </summary>
        public void Never()
        {
            lock (this)
            {
                if (!Initialized) return;
                if (CurrentSong == null) return;
                if (Channel.Id != "0") return;
                AppendPlayedSongs("b");
                PlayList pl = null;
                do
                {
                    pl = PlayList.GetNewPlayList(CurrentSong.sid, Channel, "b", PlayedSongsToString());
                } while (pl.Songs.Count == 0);
                ChangePlayListSongs(pl);
                ChangeCurrentSong();
            }
        }
        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            lock (this)
            {
                if (!Initialized) return;
                if (IsPlaying == false) return;
                IsPlaying = false;
                PauseTime = DateTime.Now;
            }
        }
        /// <summary>
        /// 播放。若暂停时长超过半个小时，则播放一个新的播放列表
        /// </summary>
        public void Play()
        {
            Debug.WriteLine("In Player.Play()");
            lock (this)
            {
                Debug.WriteLine("In lock block in Player.Play()");
                if (!Initialized) return;
                Debug.WriteLine("Initialized");
                if (IsPlaying) return;
                Debug.WriteLine("IsPlaying=false");
                if ((DateTime.Now - PauseTime).TotalMinutes > 30 && Channel.Id != "dj")
                    NewPlayList();
                IsPlaying = true;
                Debug.WriteLine("IsPlaying=true");
            }
        }
    }
}