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

namespace DoubanFM.Core.Json
{
    /// <summary>
    /// JSON格式的歌曲
    /// </summary>
    [DataContract]
    class Song
    {
        /// <summary>
        /// 专辑的豆瓣资料页面
        /// </summary>
        [DataMember]
        public string album { get; set; }
        /// <summary>
        /// 封面URL
        /// </summary>
        [DataMember]
        public string picture { get; set; }
        [DataMember]
        public string ssid { get; set; }
        /// <summary>
        /// 表演者
        /// </summary>
        [DataMember]
        public string artist { get; set; }
        /// <summary>
        /// 音乐文件URL
        /// </summary>
        [DataMember]
        public string url { get; set; }
        /// <summary>
        /// 唱片公司
        /// </summary>
        [DataMember]
        public string company { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        [DataMember]
        public string title { get; set; }
        /// <summary>
        /// 平均评分
        /// </summary>
        [DataMember]
        public double rating_avg { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        [DataMember]
        public double length { get; set; }
        [DataMember]
        public string source_url { get; set; }
        /// <summary>
        /// 普通音乐应该是""，广告应该是"T"
        /// </summary>
        [DataMember]
        public string subtype { get; set; }
        /// <summary>
        /// 发行年
        /// </summary>
        [DataMember]
        public string public_time { get; set; }
        /// <summary>
        /// 歌曲ID
        /// </summary>
        [DataMember]
        public string sid { get; set; }
        /// <summary>
        /// 专辑ID
        /// </summary>
        [DataMember]
        public string aid { get; set; }
        /// <summary>
        /// 专辑
        /// </summary>
        [DataMember]
        public string albumtitle { get; set; }
        /// <summary>
        /// 当前用户喜欢与否
        /// </summary>
        [DataMember]
        public bool like { get; set; }
    }
}
