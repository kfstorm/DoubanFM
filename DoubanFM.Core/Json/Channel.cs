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
    }
}
