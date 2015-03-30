/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DoubanFM.Core
{
	/// <summary>
	/// 表示歌词搜索结果中的一个歌词文件
	/// </summary>
	[XmlTypeAttribute("lrc")]
	public class LyricsItem
	{
		/// <summary>
		/// ID
		/// </summary>
		[XmlAttribute("id")]
		public int Id { get; set; }
		/// <summary>
		/// 艺术家
		/// </summary>
		[XmlAttribute("artist")]
		public string Artist { get; set; }
		/// <summary>
		/// 曲目名称
		/// </summary>
		[XmlAttribute("title")]
		public string Title { get; set; }
	}
}
