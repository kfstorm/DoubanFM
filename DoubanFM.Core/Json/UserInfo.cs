using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DoubanFM.Core.Json
{
    /// <summary>
    /// JSON格式的用户信息
    /// </summary>
    [DataContract]
    internal class UserInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [DataMember]
        public string user_id { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        [DataMember]
        public string err { get; set; }

        /// <summary>
        /// 是否发生错误
        /// </summary>
        [DataMember]
        public bool r { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [DataMember]
        public string name { get; set; }

        /// <summary>
        /// 累积收听歌曲数量
        /// </summary>
        [DataMember]
        public int played_num { get; set; }

        /// <summary>
        /// 加红心歌曲数量
        /// </summary>
        [DataMember]
        public int liked_num { get; set; }

        /// <summary>
        /// 不再播放歌曲数量
        /// </summary>
        [DataMember]
        public int banned_num { get; set; }

        /// <summary>
        /// Pro服务到期时间
        /// </summary>
        [DataMember]
        public string pro_expire_date { get; set; }

        /// <summary>
        /// Pro服务状态
        /// </summary>
        [DataMember]
        public string pro_status { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        [DataMember]
        public string icon { get; set; }
    }
}
