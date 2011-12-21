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
		/// 音乐类型
		/// </summary>
		public string Type { get; set; }
		/// <summary>
		/// 封面地址
		/// </summary>
		public string CoverUrl { get; set; }

		internal ShareSongInfo(string songName, string artistName, string channelName, string url, string type, string coverUrl)
		{
			SongName = songName;
			ArtistName = artistName;
			ChannelName = channelName;
			Url = string.IsNullOrEmpty(url) ? "http://douban.fm" : url;
			Type = type;
			CoverUrl = coverUrl;
		}

		/// <summary>
		/// 获取分享音乐信息
		/// </summary>
		/// <param name="song">歌曲</param>
		/// <param name="channel">频道（或者DJ节目）</param>
		/// <param name="cate">DJ频道</param>
		internal static ShareSongInfo GetInfo(Song song, Channel channel, Cate cate)
		{
			string songName = null;
			string channelName = null;
			string type = null;
			string url = null;
			if (channel.IsDj)
			{
				songName = channel.Name;
				channelName = cate.Name;
				Parameters parameters = new Parameters();
				parameters.Add("cid", channel.Id);
				parameters.Add("pid", channel.ProgramId);
				url = ConnectionBase.ConstructUrlWithParameters("http://douban.fm/", parameters);
			}
			else
			{
				songName = song.Title;
				channelName = channel.Name;
				if (!song.IsAd)
					url = "http://douban.fm/?start=" + song.SongId + "g" + song.SSId + "g" + channel.Id + "&cid=" + channel.Id;
				else url = "http://douban.fm/?daid=" + song.SongId + "&cid=" + channel.Id;
				if (channel.IsSpecial)
					url += "&context=" + channel.Context;
			}

			switch (channelName)
			{
				case "电影原声":
					type = "movie-ost";
					break;
				case "轻音乐":
					type = "easy";
					break;
				default:
					type = "single";
					break;
			}
			if (channel.IsDj) type = "dj";

			return new ShareSongInfo(songName, song.Artist, channelName, url, type, song.Picture);
		}
	}
}
