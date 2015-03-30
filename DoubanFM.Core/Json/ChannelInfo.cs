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
using System.Text.RegularExpressions;
using System.Xml;

namespace DoubanFM.Core.Json
{
    /// <summary>
    /// JSON格式的频道列表
    /// </summary>
    [DataContract]
    internal class ChannelInfo
    {
        /// <summary>
        /// 私人频道
        /// </summary>
        [DataMember]
        public Channel[] personal { get; set; }

        /// <summary>
        /// 公共频道
        /// </summary>
        [DataMember(Name = "public")]
        public Channel[] Public { get; set; }

        /// <summary>
        /// DJ频道
        /// </summary>
        [DataMember]
        public Channel[] dj { get; set; }

    }
}
