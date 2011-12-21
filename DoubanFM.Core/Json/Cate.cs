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
    /// JSON格式的门类
    /// </summary>
    [DataContract]
    class Cate
    {
        /// <summary>
        /// 该门类所含频道
        /// </summary>
        [DataMember]
        virtual public Channel[] channels { get; set; }
        /// <summary>
        /// 门类名
        /// </summary>
        [DataMember]
        public string cate { get; set; }
    }
}
