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
			Renren
		}

		/// <summary>
		/// 获取分享链接
		/// </summary>
		/// <param name="song">歌曲</param>
		/// <param name="channel">频道（或者DJ节目）</param>
		/// <param name="cate">DJ频道</param>
		/// <param name="site">要分享的网站</param>
		public static string GetShareLink(Song song, Channel channel, Cate cate, Sites site)
		{
			ShareSongInfo songInfo = ShareSongInfo.GetInfo(song, channel, cate);
			Parameters parameters = new Parameters(true);
			Parameters tempParas = new Parameters(true);
			string url = null;
			string tempurl = null;
			string text = GetShareText(songInfo.SongName, songInfo.ArtistName, songInfo.ChannelName, songInfo.Type);

			switch (site)
			{
				case Sites.Douban:
					parameters.Add("name", songInfo.SongName);
					parameters.Add("href", songInfo.Url);
					parameters.Add("image", songInfo.CoverUrl);
					parameters.Add("text", "");
					parameters.Add("desc", "(来自豆瓣电台客户端 - " + songInfo.ChannelName + ")");
					parameters.Add("apikey", "079055d6a0d5ddf816b10183e28199e8");
					url = ConnectionBase.ConstructUrlWithParameters("http://shuo.douban.com/!service/share", parameters);
					break;
				case Sites.Weibo:
					tempParas.Add("appkey", "3163308509");
					tempParas.Add("url", songInfo.Url);
					tempParas.Add("title", text);
					tempParas.Add("source", "");
					tempParas.Add("sourceurl", "");
					tempParas.Add("content", "utf-8");
					tempParas.Add("pic", songInfo.CoverUrl);
					tempurl = ConnectionBase.ConstructUrlWithParameters("http://v.t.sina.com.cn/share/share.php", tempParas);
					parameters.Add("type", "redir");
					parameters.Add("vendor", "bshare_sina");
					parameters.Add("url", tempurl);
					url = ConnectionBase.ConstructUrlWithParameters("http://www.douban.com/link2", parameters);
					break;
				case Sites.Msn:
					tempParas.Add("url", songInfo.Url);
					tempParas.Add("title", text);
					tempParas.Add("screenshot", songInfo.CoverUrl);
					tempurl = ConnectionBase.ConstructUrlWithParameters("http://profile.live.com/badge", tempParas);
					parameters.Add("type", "redir");
					parameters.Add("vendor", "bshare_msn");
					parameters.Add("url", tempurl);
					url = ConnectionBase.ConstructUrlWithParameters("http://www.douban.com/link2", parameters);
					break;
				case Sites.Kaixin:
					tempParas.Add("rurl", songInfo.Url);
					tempParas.Add("rcontent", "");
					tempParas.Add("rtitle", text);
					tempurl = ConnectionBase.ConstructUrlWithParameters("http://www.kaixin001.com/repaste/bshare.php", tempParas);
					parameters.Add("type", "redir");
					parameters.Add("vendor", "bshare_kx");
					parameters.Add("url", tempurl);
					url = ConnectionBase.ConstructUrlWithParameters("http://www.douban.com/link2", parameters);
					break;
				case Sites.Renren:
					tempParas.Add("url", songInfo.Url);
					tempParas.Add("title", text);
					tempurl = ConnectionBase.ConstructUrlWithParameters("http://www.connect.renren.com/share/sharer", tempParas);
					parameters.Add("type", "redir");
					parameters.Add("vendor", "bshare_renren");
					parameters.Add("url", tempurl);
					url = ConnectionBase.ConstructUrlWithParameters("http://www.douban.com/link2", parameters);
					break;
				default:
					break;
			}
			return url;
		}

		/// <summary>
		/// 获取分享文字
		/// </summary>
		static string GetShareText(string songName, string artistName, string channelName, string type)
		{
			switch (type)
			{
				case "movie-ost":
					return "我正在收听《" + artistName + "》的电影原声 “" + songName + "” （来自豆瓣电台客户端 - " + channelName + "）";
				case "easy":
					return "我正在收听 " + artistName + " 的乐曲《" + songName + "》（来自豆瓣电台客户端 - " + channelName + "）";
				case "dj":
					return "我正在收听节目 《" + songName + "》- " + channelName + "（来自豆瓣电台客户端 - DJ兆赫 ）";
				default:
					return "我正在收听 " + artistName + " 的单曲《" + songName + "》（来自豆瓣电台客户端 - " + channelName + "）";
			}
		}
	}
}
