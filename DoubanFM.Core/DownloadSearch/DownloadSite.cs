/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Core
{
	/// <summary>
	/// 提供音乐下载的网站
	/// </summary>
	[Flags]
	public enum DownloadSite
	{
		None = 0x0,
		/// <summary>
		/// 谷歌音乐(www.google.cn/music)
		/// </summary>
		GoogleMusic = 0x1,
		/// <summary>
		/// 百度听(ting.baidu.com)
		/// </summary>
		BaiduTing = 0x2
	}
}
