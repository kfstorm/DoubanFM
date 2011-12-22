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
	[DataContract]
	public class UserInfo
	{
		[DataMember]
		public string url {get; set;}

		[DataMember]
		public string ck { get; set;}

		[DataMember]
		public PlayRecord play_record { get; set; }

		[DataMember]
		public string id { get; set; }

		[DataMember]
		public string name { get; set; }
	}
}
