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
	/// JSON格式的用户信息
	/// </summary>
	[DataContract]
	public class UserInfo
	{
		/// <summary>
		/// 用户主页
		/// </summary>
		[DataMember]
		public string url {get; set;}

		/// <summary>
		/// 用于注销的字符串
		/// </summary>
		[DataMember]
		public string ck { get; set;}

		/// <summary>
		/// 播放记录
		/// </summary>
		[DataMember]
		public PlayRecord play_record { get; set; }

		/// <summary>
		/// 用户ID
		/// </summary>
		[DataMember]
		public string id { get; set; }

		/// <summary>
		/// 名号
		/// </summary>
		[DataMember]
		public string name { get; set; }
	}
}
