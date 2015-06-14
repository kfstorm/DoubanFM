/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;

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

		internal PlayList(Json.PlayList pl)
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
        /// <param name="playerState">播放器状态</param>
        /// <param name="operationType">操作类型</param>
        /// <returns>
        /// 播放列表
        /// </returns>
	    internal static PlayList GetPlayList(Player.PlayerState playerState, string operationType)
		{
			//构造链接
			Parameters parameters = new Parameters();
            parameters["app_name"] = "radio_desktop_win";
            parameters["version"] = "100";
            parameters["user_id"] = playerState.CurrentUser.UserID;
            parameters["token"] = playerState.CurrentUser.Token;
            parameters["expire"] = playerState.CurrentUser.Expire;
            parameters["from"] = "mainsite";
            parameters["context"] = playerState.CurrentChannel.Context;
            parameters["sid"] = playerState.CurrentSong != null ? playerState.CurrentSong.SongId : null;
            parameters["channel"] = playerState.CurrentChannel.Id;
			parameters["type"] = operationType;
			random.NextBytes(bytes);
			parameters["r"] = (BitConverter.ToUInt64(bytes, 0) % 0xFFFFFFFFFF).ToString("x10");

            if (playerState.CurrentUser.IsPro)
            {
                string kbps = null;
                switch (playerState.CurrentUser.ProRate)
                {
                    case ProRate.Kbps64:
                        kbps = "64";
                        break;
                    case ProRate.Kbps128:
                        kbps = "128";
                        break;
                    case ProRate.Kbps192:
                        kbps = "192";
                        break;
                    default:
                        break;
                }
                parameters["kbps"] = kbps;
            }

            string url = ConnectionBase.ConstructUrlWithParameters("http://www.douban.com/j/app/radio/people", parameters);

			//获取列表
			string json = new ConnectionBase().Get(url, @"application/json, text/javascript, */*; q=0.01", @"http://douban.fm");
			var jsonPlayList = Json.JsonHelper.FromJson<Json.PlayList>(json);
			if (jsonPlayList != null && jsonPlayList.r)
				RaiseGetPlayListFailedEvent(json);
			PlayList pl = new PlayList(jsonPlayList);

			//将小图更换为大图
			foreach (var s in pl)
			{
				s.Picture = s.Picture.Replace("/mpic/", "/lpic/").Replace("//otho.", "//img3.");
			}

			//去广告
			pl.RemoveAll(s => s.IsAd);

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
