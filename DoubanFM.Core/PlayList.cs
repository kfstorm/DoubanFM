using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Core
{
    public class PlayList : List<Song>
    {
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
            parameters.Add("context", context);
            parameters.Add("sid", songId);
            parameters.Add("channel", channel.Id);
            if (channel.IsDj)
                parameters.Add("pid", channel.ProgramId);
            parameters.Add("type", operationType);
            parameters.Add("h", history);
            string url = ConnectionBase.ConstructUrlWithParameters("http://douban.fm/j/mine/playlist", parameters.ToArray());
            string json = new ConnectionBase().Get(url, @"application/json, text/javascript, */*; q=0.01", @"http://douban.fm");
            PlayList pl = new PlayList(Json.PlayList.FromJson(json));
            foreach (var song in pl)
            {
                song.Picture = song.Picture.Replace("/mpic/", "/lpic/").Replace("//otho.", "//img3.");
            }
            return pl;
        }
    }
}
