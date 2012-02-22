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
	public class PlayList : List<Song>
	{
		/// <summary>
		/// 当获取播放列表失败时发生。
		/// </summary>
		internal static event EventHandler<PlayListEventArgs> GetPlayListFailed;

		private static void RaiseGetPlayListFailedEvent(string json)
		{
			if (GetPlayListFailed != null)
				GetPlayListFailed(null, new PlayListEventArgs(json));
		}

		internal PlayList(DoubanFM.Core.Json.PlayList pl)
			:base()
		{
			if (pl != null &&pl.song != null)
				foreach (var song in pl.song)
				{
					this.Add(new Song(song));
				}
		}
		/// <summary>
		/// 获取播放列表
		/// </summary>
		/// <param name="context">上下文</param>
		/// <param name="songId">当前歌曲ID</param>
		/// <param name="channel">频道</param>
		/// <param name="operationType">操作类型</param>
		/// <param name="history">播放历史</param>
		/// <returns></returns>
		internal static PlayList GetPlayList(string context, string songId, Channel channel, string operationType, string history)
		{
			Parameters parameters = new Parameters();
			parameters["from"] = "ie9";
			parameters["context"] = context;
			parameters["sid"] = songId;
			parameters["channel"] = channel.Id;
			if (channel.IsDj)
				parameters["pid"] = channel.ProgramId;
			parameters["type"] = operationType;
			parameters["h"] = history;
			string url = ConnectionBase.ConstructUrlWithParameters("http://douban.fm/j/mine/playlist", parameters);
			string json = new ConnectionBase().Get(url, @"application/json, text/javascript, */*; q=0.01", @"http://douban.fm");
			var jsonPlayList = Json.PlayList.FromJson(json);
			if (jsonPlayList != null && jsonPlayList.r != 0)
				RaiseGetPlayListFailedEvent(json);
			PlayList pl = new PlayList(jsonPlayList);
			foreach (var song in pl)
			{
				song.Picture = song.Picture.Replace("/mpic/", "/lpic/").Replace("//otho.", "//img3.");
			}
			pl.RemoveAll(new Predicate<Song>(song => { return song.IsAd; }));		//去广告
			return pl;
		}

		public class PlayListEventArgs : EventArgs
		{
			public string Msg { get; private set; }
			internal PlayListEventArgs(string msg)
			{
				Msg = msg;
			}
		}
	}
}
