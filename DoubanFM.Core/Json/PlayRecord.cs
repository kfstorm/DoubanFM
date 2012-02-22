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
	/// JSON格式的播放记录
	/// </summary>
	[DataContract]
	public class PlayRecord
	{
		/// <summary>
		/// 喜欢的歌曲数
		/// </summary>
		[DataMember]
		public int liked { get; set; }

		/// <summary>
		/// 不再播放的歌曲数
		/// </summary>
		[DataMember]
		public int banned { get; set; }

		/// <summary>
		/// 播放过的歌曲数
		/// </summary>
		[DataMember]
		public int played { get; set; }
	}
}
