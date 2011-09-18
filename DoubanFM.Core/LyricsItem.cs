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
		[XmlAttribute("id")]
		public int Id { get; set; }
		[XmlAttribute("artist")]
		public string Artist { get; set; }
		[XmlAttribute("title")]
		public string Title { get; set; }
	}
}
