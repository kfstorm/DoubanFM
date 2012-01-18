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

namespace DoubanFM.Core.Json
{
	[DataContract]
	public class LogOnResult
	{
		[DataMember]
		public UserInfo user_info { get; set; }

		[DataMember]
		public bool r { get; set; }

		[DataMember]
		public string err_msg { get; set; }
		
		[DataMember]
		public int err_no { get; set; }

		/// <summary>
		/// 从JSON生成
		/// </summary>
		/// <param name="json">JSON字符串</param>
		/// <returns></returns>
		public static LogOnResult FromJson(string json)
		{
			try
			{
				DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(LogOnResult));
				using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
					return (LogOnResult)ser.ReadObject(stream);
			}
			catch
			{
				return null;
			}
		}
	}
}
