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
	/// 播放列表
	/// </summary>
	public class PlayList : List<Song>
	{
		/// <summary>
		/// 当获取播放列表失败时发生。
		/// </summary>
		internal static event EventHandler<PlayListEventArgs> GetPlayListFailed;

		static Random random = new Random();
		static byte[] bytes = new byte[8];

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
		/// <returns>播放列表</returns>
		internal static PlayList GetPlayList(string context, string songId, Channel channel, string operationType)
		{
			//构造链接
			Parameters parameters = new Parameters();
			parameters["from"] = "mainsite";
			parameters["context"] = context;
			parameters["sid"] = songId;
			parameters["channel"] = channel.Id;
			parameters["type"] = operationType;
			random.NextBytes(bytes);
			parameters["r"] = (BitConverter.ToUInt64(bytes, 0) % 0xFFFFFFFFFF).ToString("x10");

			string url = ConnectionBase.ConstructUrlWithParameters("http://douban.fm/j/mine/playlist", parameters);

			//获取列表
			string json = new ConnectionBase().Get(url, @"application/json, text/javascript, */*; q=0.01", @"http://douban.fm");
			var jsonPlayList = Json.PlayList.FromJson(json);
			if (jsonPlayList != null && jsonPlayList.r)
				RaiseGetPlayListFailedEvent(json);
			PlayList pl = new PlayList(jsonPlayList);

			//将小图更换为大图
			foreach (var song in pl)
			{
				song.Picture = song.Picture.Replace("/mpic/", "/lpic/").Replace("//otho.", "//img3.");
			}

			//去广告
			pl.RemoveAll(new Predicate<Song>(song => { return song.IsAd; }));

			return pl;
		}

		/// <summary>
		/// 播放列表的事件参数
		/// </summary>
		public class PlayListEventArgs : EventArgs
		{
			/// <summary>
			/// 消息
			/// </summary>
			public string Message { get; private set; }
			internal PlayListEventArgs(string message)
			{
				Message = message;
			}
		}
	}
}
