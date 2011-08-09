using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DoubanFM.Core.Json
{
    /// <summary>
    /// JSON格式的频道
    /// </summary>
    [DataContract]
    class Channel
    {
        /// <summary>
        /// 频道ID
        /// </summary>
        [DataMember]
        public string channel_id { get; set; }
        /// <summary>
        /// 频道名称
        /// </summary>
        [DataMember]
        public string name { get; set; }
        /// <summary>
        /// DJ频道特有的属性，节目ID（Program ID?）
        /// </summary>
        [DataMember]
        public string pid { get; set; }
    }
}
