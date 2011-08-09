using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DoubanFM.Core.Json
{
    /// <summary>
    /// JSON格式序列化用
    /// </summary>
    [DataContract]
    class SubDjChannel
    {
        /// <summary>
        /// 通常为5位数字，实为某DJ频道的某节目ID
        /// </summary>
        [DataMember(Name = "channel_id")]
        public string pid { get; set; }
        /// <summary>
        /// 节目名称
        /// </summary>
        [DataMember]
        public string name { get; set; }
    }
}
