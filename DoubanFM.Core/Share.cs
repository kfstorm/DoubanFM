/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Core
{
    /// <summary>
    /// 分享
    /// </summary>
    public class Share
    {
        /// <summary>
        /// 分享网站
        /// </summary>
        public enum Sites
        {
            /// <summary>
            /// 无分享网站，仅复制网址
            /// </summary>
            None,
            /// <summary>
            /// 豆瓣
            /// </summary>
            Douban,
            /// <summary>
            /// 新浪微博
            /// </summary>
            Weibo,
            /// <summary>
            /// MSN
            /// </summary>
            Msn,
            /// <summary>
            /// 开心网
            /// </summary>
            Kaixin,
            /// <summary>
            /// 人人网
            /// </summary>
            Renren,
            /// <summary>
            /// 腾讯微博
            /// </summary>
            TencentWeibo,
            /// <summary>
            /// 饭否
            /// </summary>
            Fanfou,
            /// <summary>
            /// Facebook
            /// </summary>
            Facebook,
            /// <summary>
            /// Twitter
            /// </summary>
            Twitter,
            /// <summary>
            /// QQ空间
            /// </summary>
            Qzone
        }

        /// <summary>
        /// 分享的歌曲
        /// </summary>
        public Song Song { get; private set; }

        /// <summary>
        /// 歌曲所在的频道
        /// </summary>
        public Channel Channel { get; private set; }

        /// <summary>
        /// 分享的网站
        /// </summary>
        public Sites? Site { get; set; }

        /// <summary>
        /// 分享的内容
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 分享的内容（不包含软件的下载地址）
        /// </summary>
        public string TextWithoutSource { get; set; }

        /// <summary>
        /// 分享的链接
        /// </summary>
        public string Url
        {
            get
            {
                return _songInfo.Url;
            }
        }

        /// <summary>
        /// 包含了分享需要的歌曲信息
        /// </summary>
        private ShareSongInfo _songInfo;

        /// <summary>
        /// 生成 <see cref="Share"/> class 的新实例。
        /// </summary>
        /// <param name="song">歌曲</param>
        /// <param name="channel">频道</param>
        /// <param name="site">网站</param>
        public Share(Song song, Channel channel, Sites site)
        {
            if (song == null)
                throw new ArgumentNullException("song");
            if (channel == null)
                throw new ArgumentNullException("channel");
            Song = song;
            Channel = channel;
            Site = site;

            _songInfo = ShareSongInfo.GetInfo(song, channel);
            Text = GetShareText(_songInfo.SongName, _songInfo.ArtistName, _songInfo.ChannelName);
            TextWithoutSource = GetShareText(_songInfo.SongName, _songInfo.ArtistName, _songInfo.ChannelName, false);
        }

        /// <summary>
        /// 生成 <see cref="Share"/> class 的新实例。
        /// </summary>
        /// <param name="player">播放器</param>
        /// <param name="site">网站</param>
        public Share(Player player, Sites site)
            : this(player.CurrentSong, player.CurrentChannel, site)
        { }

        /// <summary>
        /// 生成 <see cref="Share"/> class 的新实例。
        /// </summary>
        /// <param name="song">歌曲</param>
        /// <param name="channel">频道</param>
        public Share(Song song, Channel channel)
            : this(song, channel, Sites.None)
        { }

        /// <summary>
        /// 生成 <see cref="Share"/> class 的新实例。
        /// </summary>
        /// <param name="player">播放器</param>
        public Share(Player player)
            : this(player, Sites.None)
        { }

        /// <summary>
        /// 获取用于显示的网站排序
        /// </summary>
        /// <returns></returns>
        public static Sites[] GetSortedSites()
        {
            return new Sites[] { Sites.None, Sites.Douban, Sites.Weibo, Sites.Msn, Sites.Kaixin, Sites.Renren, Sites.Qzone, Sites.TencentWeibo, Sites.Fanfou, Sites.Facebook, Sites.Twitter };
        }

        /// <summary>
        /// 获取分享链接
        /// </summary>
        public string GetShareLink()
        {
            if (Site == null)
                throw new Exception("没有设定分享网站。");
            Parameters parameters = new Parameters(true);
            string url = null;

            switch (Site)
            {
                case Sites.None:
                    throw new InvalidOperationException("复制网址模式不能获取分享链接");
                //break;
                case Sites.Douban:
                    parameters["name"] = _songInfo.SongName;
                    parameters["href"] = _songInfo.Url;
                    parameters["image"] = _songInfo.CoverUrl;
                    parameters["text"] = "";
                    parameters["desc"] = GetShareTextPartTwo(_songInfo.ChannelName);
                    parameters["apikey"] = "0c2e1df44f97c4eb248a59dceec74ec1";
                    url = ConnectionBase.ConstructUrlWithParameters("http://shuo.douban.com/!service/share", parameters);
                    break;
                case Sites.Weibo:
                    parameters["appkey"] = "1075899032";
                    parameters["url"] = _songInfo.Url;
                    parameters["title"] = TextWithoutSource;
                    parameters["content"] = "utf-8";
                    parameters["pic"] = _songInfo.CoverUrl;
                    url = ConnectionBase.ConstructUrlWithParameters("http://service.t.sina.com.cn/share/share.php", parameters);
                    break;
                case Sites.Msn:
                    parameters["url"] = _songInfo.Url;
                    parameters["title"] = Text;
                    parameters["screenshot"] = _songInfo.CoverUrl;
                    url = ConnectionBase.ConstructUrlWithParameters("http://profile.live.com/badge", parameters);
                    break;
                case Sites.Kaixin:
                    parameters["rurl"] = _songInfo.Url;
                    parameters["rcontent"] = "";
                    parameters["rtitle"] = Text;
                    url = ConnectionBase.ConstructUrlWithParameters("http://www.kaixin001.com/repaste/bshare.php", parameters);
                    break;
                case Sites.Renren:
                    parameters["resourceUrl"] = _songInfo.Url;
                    parameters["title"] = GetShareTextPartOne(_songInfo.SongName, _songInfo.ArtistName); ;
                    parameters["pic"] = _songInfo.CoverUrl;
                    parameters["description"] = GetShareTextPartTwo(_songInfo.ChannelName);
                    parameters["charset"] = "utf-8";
                    url = ConnectionBase.ConstructUrlWithParameters("http://widget.renren.com/dialog/share", parameters);
                    break;
                case Sites.TencentWeibo:
                    parameters["url"] = _songInfo.Url;
                    parameters["title"] = Text;
                    parameters["site"] = "http://www.kfstorm.com/doubanfm";
                    parameters["pic"] = _songInfo.CoverUrl;
                    parameters["appkey"] = "801098586";
                    url = ConnectionBase.ConstructUrlWithParameters("http://v.t.qq.com/share/share.php", parameters);
                    break;
                case Sites.Fanfou:
                    parameters["u"] = _songInfo.Url;
                    parameters["t"] = Text;
                    parameters["s"] = "bm";
                    url = ConnectionBase.ConstructUrlWithParameters("http://fanfou.com/sharer", parameters);
                    break;
                case Sites.Facebook:
                    parameters["u"] = _songInfo.Url;
                    parameters["t"] = Text;
                    url = ConnectionBase.ConstructUrlWithParameters("http://www.facebook.com/sharer.php", parameters);
                    break;
                case Sites.Twitter:
                    parameters["status"] = Text + " " + _songInfo.Url;
                    url = ConnectionBase.ConstructUrlWithParameters("http://twitter.com/home", parameters);
                    break;
                case Sites.Qzone:
                    parameters["url"] = _songInfo.Url;
                    parameters["title"] = Text;
                    url = ConnectionBase.ConstructUrlWithParameters("http://sns.qzone.qq.com/cgi-bin/qzshare/cgi_qzshare_onekey", parameters);
                    break;
                default:
                    break;
            }
            return url;
        }

        /// <summary>
        /// 打开分享链接或复制网址
        /// </summary>
        public void Go()
        {
            if (Site == Sites.None)
            {
                try
                {
                    System.Windows.Clipboard.Clear();
                    System.Windows.Clipboard.SetText(Url);
                }
                catch { }
            }
            else
            {
                UrlHelper.OpenLink(GetShareLink());
            }
        }

        /// <summary>
        /// 获取分享文字
        /// </summary>
        static string GetShareText(string songName, string artistName, string channelName, bool withSource = true)
        {

            return GetShareTextPartOne(songName, artistName) + " " + GetShareTextPartTwo(channelName, withSource);
        }

        static string GetShareTextPartOne(string songName, string artistName)
        {
            return string.Format(Resources.Resources.ShareTextPartOne, songName, artistName);
        }

        static string GetShareTextPartTwo(string channelName, bool withSource = true)
        {
            return string.Format(Resources.Resources.ShareTextPartTwo, withSource ? "(http://kfstorm.com/doubanfm)" : string.Empty, channelName);
        }
    }
}