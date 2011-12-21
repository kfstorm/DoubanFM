/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace DoubanFM.Core.Json
{
    /// <summary>
    /// JSON格式的播放列表
    /// </summary>
    [DataContract]
    class PlayList
    {
        [DataMember]
        public int r { get; set; }
        /// <summary>
        /// 播放列表里的歌曲
        /// </summary>
        [DataMember]
        public List<Song> song { get; set; }

        public PlayList()
        {
            song = new List<Song>();
        }
        /// <summary>
        /// 从JSON生成
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns></returns>
        public static PlayList FromJson(string json)
        {
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(PlayList));
                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                    return (PlayList)ser.ReadObject(stream);
            }
            catch
            {
                return null;
            }
        }
    }
}
