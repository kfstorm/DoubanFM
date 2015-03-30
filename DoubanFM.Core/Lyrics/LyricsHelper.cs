/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Diagnostics;

namespace DoubanFM.Core
{
    /// <summary>
    /// 获取歌词
    /// </summary>
    public class LyricsHelper
    {
        /// <summary>
        /// 获取歌词
        /// </summary>
        /// <param name="song">歌曲</param>
        /// <returns>歌词</returns>
        public static Lyrics GetLyrics(Song song)
        {
            //优先获取来自豆瓣的歌词。
            var lyrics = GetDoubanLyrics(song);
            return lyrics ?? TTPlayerLyrics.GetLyrics(song.Artist, song.Title);
        }

        /// <summary>
        /// 从豆瓣获取歌词
        /// </summary>
        /// <param name="song">歌曲</param>
        /// <returns>歌词</returns>
        protected static Lyrics GetDoubanLyrics(Song song)
        {
            Parameters parameters = new Parameters();
            parameters["song_id"] = song.SongId;
            var url = ConnectionBase.ConstructUrlWithParameters("http://music.douban.com/api/song/info", parameters);
            var content = new ConnectionBase().Get(url);
            if (string.IsNullOrEmpty(content)) return null;

            var songInfo = Json.JsonHelper.FromJson<Json.SongInfo>(content);
            if (songInfo == null || string.IsNullOrEmpty(songInfo.Lyric)) return null;
            
            try
            {
                return new Lyrics(songInfo.Lyric);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
