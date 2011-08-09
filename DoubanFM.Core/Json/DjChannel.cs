using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DoubanFM.Core.Json
{
    /// <summary>
    /// JSON格式的DJ频道类，仅用于序列化
    /// </summary>
    [DataContract]
    class DjChannel
    {
        /// <summary>
        /// DJ频道的ID，通常为一串6位数字
        /// </summary>
        [DataMember]
        public string channel_id { get; set; }
        /// <summary>
        /// 频道名称
        /// </summary>
        [DataMember]
        public string name { get; set; }
        /// <summary>
        /// 不知道有什么用
        /// </summary>
        [DataMember]
        public string timestamp { get; set; }
        /// <summary>
        /// 更新的节目数量
        /// </summary>
        [DataMember]
        public int update { get; set; }
    }
}
