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
	/// 分享音乐信息
	/// </summary>
	class ShareSongInfo
	{
		/// <summary>
		/// 歌曲标题（或者DJ节目名称）
		/// </summary>
		public string SongName { get; set; }
		/// <summary>
		/// 表演者
		/// </summary>
		public string ArtistName { get; set; }
		/// <summary>
		/// 频道名称（或者DJ频道名称）
		/// </summary>
		public string ChannelName { get; set; }
		/// <summary>
		/// 分享的电台链接
		/// </summary>
		public string Url { get; set; }
		/// <summary>
		/// 封面地址
		/// </summary>
		public string CoverUrl { get; set; }

		/// <summary>
		/// 生成 <see cref="ShareSongInfo"/> class 的新实例。
		/// </summary>
		/// <param name="songName">歌曲标题（或者DJ节目名称）</param>
		/// <param name="artistName">表演者</param>
		/// <param name="channelName">频道名称（或者DJ频道名称）</param>
		/// <param name="url">分享的电台链接</param>
		/// <param name="coverUrl">封面地址</param>
		internal ShareSongInfo(string songName, string artistName, string channelName, string url, string coverUrl)
		{
			SongName = songName;
			ArtistName = artistName;
			ChannelName = channelName;
			Url = string.IsNullOrEmpty(url) ? "http://douban.fm" : url;
			CoverUrl = coverUrl;
		}

		/// <summary>
		/// 获取分享音乐信息
		/// </summary>
		/// <param name="song">歌曲</param>
		/// <param name="channel">频道（或者DJ节目）</param>
		/// <returns></returns>
		internal static ShareSongInfo GetInfo(Song song, Channel channel)
		{
			string songName = song.Title;
			string channelName = channel.Name; ;
			string url = null;
			Parameters parameters = new Parameters();
			parameters["cid"] = channel.IsRedHeart ? Channel.PersonalId : channel.Id;
			if (!song.IsAd)
			{
				parameters["start"] = song.SongId + "g" + song.SSId + "g" + channel.Id;
				//url = "http://douban.fm/?start=" + song.SongId + "g" + song.SSId + "g" + channel.Id + "&cid=" + channel.Id;
			}
			else
			{
				parameters["daid"] = song.SongId;
				//url = "http://douban.fm/?daid=" + song.SongId + "&cid=" + channel.Id;
			}
			if (channel.IsSpecial)
			{
				parameters["context"] = channel.Context;
				//url += "&context=" + channel.Context;
			}
			url = ConnectionBase.ConstructUrlWithParameters("http://douban.fm/", parameters);

			return new ShareSongInfo(songName, song.Artist, channelName, url, song.Picture);
		}
	}
}
