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
    internal class PlayList
    {
        /// <summary>
        /// 是否发生错误
        /// </summary>
        [DataMember]
        public bool r { get; set; }

        /// <summary>
        /// 播放列表里的歌曲
        /// </summary>
        [DataMember]
        public List<Song> song { get; set; }

        public PlayList()
        {
            song = new List<Song>();
        }
    }
}
