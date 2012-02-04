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

		public static readonly Dictionary<Sites, string> SiteName;

		public Song Song { get; private set; }

		public Channel Channel { get; private set; }

		public Cate Cate { get; private set; }

		public Sites? Site { get; set; }

		public string Text { get; set; }

		public string Url
		{
			get
			{
				return _songInfo.Url;
			}
		}

		private ShareSongInfo _songInfo;

		static Share()
		{
			SiteName = new Dictionary<Sites, string>();
			SiteName.Add(Sites.Douban, "豆瓣");
			SiteName.Add(Sites.Facebook, "Facebook");
			SiteName.Add(Sites.Fanfou, "饭否");
			SiteName.Add(Sites.Kaixin, "开心网");
			SiteName.Add(Sites.Msn, "MSN");
			SiteName.Add(Sites.Renren, "人人网");
			SiteName.Add(Sites.TencentWeibo, "腾讯微博");
			SiteName.Add(Sites.Twitter, "Twitter");
			SiteName.Add(Sites.Weibo, "新浪微博");
			SiteName.Add(Sites.Qzone, "QQ空间");
			SiteName.Add(Sites.None, "*复制网址到剪贴板*");
		}

		public Share(Song song, Channel channel, Cate cate, Sites site)
		{
			if (song == null)
				throw new ArgumentNullException("song");
			if (channel == null)
				throw new ArgumentNullException("channel");
			Song = song;
			Channel = channel;
			Cate = cate;
			Site = site;

			_songInfo = ShareSongInfo.GetInfo(song, channel, cate);
			Text = GetShareText(_songInfo.SongName, _songInfo.ArtistName, _songInfo.ChannelName, _songInfo.Type);
		}

		public Share(Player player, Sites site)
			: this(player.CurrentSong, player.CurrentChannel, player.CurrentDjCate, site)
		{ }

		public Share(Song song, Channel channel, Cate cate)
			: this(song, channel, cate, Sites.None)
		{ }

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
			Parameters tempParas = new Parameters(true);
			string url = null;
			string tempurl = null;
			
			switch (Site)
			{
				case Sites.None:
					throw new InvalidOperationException("复制网址模式不能获取分享链接");
					//break;
				case Sites.Douban:
					parameters.Add("name", _songInfo.SongName);
					parameters.Add("href", _songInfo.Url);
					parameters.Add("image", _songInfo.CoverUrl);
					parameters.Add("text", "");
					parameters.Add("desc", "（来自豆瓣电台客户端【http://kfstorm.com/doubanfm】 - " + _songInfo.ChannelName + "）");
					parameters.Add("apikey", "079055d6a0d5ddf816b10183e28199e8");
					url = ConnectionBase.ConstructUrlWithParameters("http://shuo.douban.com/!service/share", parameters);
					break;
				case Sites.Weibo:
					tempParas.Add("appkey", "3163308509");
					tempParas.Add("url", _songInfo.Url);
					tempParas.Add("title", Text);
					tempParas.Add("source", "");
					tempParas.Add("sourceurl", "");
					tempParas.Add("content", "utf-8");
					tempParas.Add("pic", _songInfo.CoverUrl);
					tempurl = ConnectionBase.ConstructUrlWithParameters("http://v.t.sina.com.cn/share/share.php", tempParas);
					parameters.Add("type", "redir");
					parameters.Add("vendor", "bshare_sina");
					parameters.Add("url", tempurl);
					url = ConnectionBase.ConstructUrlWithParameters("http://www.douban.com/link2", parameters);
					break;
				case Sites.Msn:
					tempParas.Add("url", _songInfo.Url);
					tempParas.Add("title", Text);
					tempParas.Add("screenshot", _songInfo.CoverUrl);
					tempurl = ConnectionBase.ConstructUrlWithParameters("http://profile.live.com/badge", tempParas);
					parameters.Add("type", "redir");
					parameters.Add("vendor", "bshare_msn");
					parameters.Add("url", tempurl);
					url = ConnectionBase.ConstructUrlWithParameters("http://www.douban.com/link2", parameters);
					break;
				case Sites.Kaixin:
					tempParas.Add("rurl", _songInfo.Url);
					tempParas.Add("rcontent", "");
					tempParas.Add("rtitle", Text);
					tempurl = ConnectionBase.ConstructUrlWithParameters("http://www.kaixin001.com/repaste/bshare.php", tempParas);
					parameters.Add("type", "redir");
					parameters.Add("vendor", "bshare_kx");
					parameters.Add("url", tempurl);
					url = ConnectionBase.ConstructUrlWithParameters("http://www.douban.com/link2", parameters);
					break;
				case Sites.Renren:
					tempParas.Add("url", _songInfo.Url);
					tempParas.Add("title", Text);
					tempurl = ConnectionBase.ConstructUrlWithParameters("http://www.connect.renren.com/share/sharer", tempParas);
					parameters.Add("type", "redir");
					parameters.Add("vendor", "bshare_renren");
					parameters.Add("url", tempurl);
					url = ConnectionBase.ConstructUrlWithParameters("http://www.douban.com/link2", parameters);
					break;
				case Sites.TencentWeibo:
					parameters.Add("url", _songInfo.Url);
					parameters.Add("title", Text);
					parameters.Add("site", "http://www.kfstorm.com/doubanfm");
					parameters.Add("pic", _songInfo.CoverUrl);
					url = ConnectionBase.ConstructUrlWithParameters("http://v.t.qq.com/share/share.php", parameters);
					break;
				case Sites.Fanfou:
					string fanfouParameters = "?u=" + Uri.EscapeDataString(_songInfo.Url)
						+ "?t=" + Uri.EscapeDataString(Text)
						+ "?d=" + Uri.EscapeDataString("")
						+ "?s=bm";
					url = "http://fanfou.com/share" + "r" + fanfouParameters;
					break;
				case Sites.Facebook:
					parameters.Add("u", _songInfo.Url);
					parameters.Add("t", Text);
					url = ConnectionBase.ConstructUrlWithParameters("http://www.facebook.com/sharer.php", parameters);
					break;
				case Sites.Twitter:
					parameters.Add("status", Text + " " + _songInfo.Url);
					url = ConnectionBase.ConstructUrlWithParameters("http://twitter.com/home", parameters);
					break;
				case Sites.Qzone:
					parameters.Add("url", _songInfo.Url);
					parameters.Add("title", Text);
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
		static string GetShareText(string songName, string artistName, string channelName, string type)
		{
			switch (type)
			{
				case "movie-ost":
					return "我正在收听《" + artistName + "》的电影原声 “" + songName + "” （来自豆瓣电台客户端【http://kfstorm.com/doubanfm】 - " + channelName + "）";
				case "easy":
					return "我正在收听 " + artistName + " 的乐曲《" + songName + "》（来自豆瓣电台客户端【http://kfstorm.com/doubanfm】 - " + channelName + "）";
				case "dj":
					return "我正在收听节目 《" + songName + "》- " + channelName + "（来自豆瓣电台客户端【http://kfstorm.com/doubanfm】 - DJ兆赫 ）";
				default:
					return "我正在收听 " + artistName + " 的单曲《" + songName + "》（来自豆瓣电台客户端【http://kfstorm.com/doubanfm】 - " + channelName + "）";
			}
		}
	}
}
