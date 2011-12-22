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
	public class PlayRecord
	{
		[DataMember]
		public int liked { get; set; }

		[DataMember]
		public int banned { get; set; }

		[DataMember]
		public int played { get; set; }
	}
}
