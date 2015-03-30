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
	/// 提供下载搜索的功能
	/// </summary>
	public static class DownloadSearch
	{
		/// <summary>
		/// 设置
		/// </summary>
		public static Settings Settings { get; internal set; }
		/// <summary>
		/// 搜索
		/// </summary>
		/// <param name="title">标题</param>
		/// <param name="artist">艺术家</param>
		/// <param name="album">专辑</param>
		public static void Search(string title, string artist, string album)
		{
			if (title == null) title = string.Empty;
			if (artist == null) artist = string.Empty;
			if (album == null) album = string.Empty;
			string keyword = GetKeyword(title, artist, album);
			if (Settings.DownloadSite.HasFlag(DownloadSite.BaiduMusic))
			{
				BaiduMusicSearch(keyword);
			}
            if (Settings.DownloadSite.HasFlag(DownloadSite.QQMusic))
            {
                QQMusicSearch(keyword);
            }
		}
		
        /// <summary>
		/// 搜索百度音乐
		/// </summary>
		/// <param name="keyword">关键词</param>
		private static void BaiduMusicSearch(string keyword)
		{
			Parameters parameters = new Parameters();
			parameters["key"] = keyword;
			string url = ConnectionBase.ConstructUrlWithParameters("http://music.baidu.com/search", parameters);
			UrlHelper.OpenLink(url);
		}

        /// <summary>
        /// 搜索QQ音乐
        /// </summary>
        /// <param name="keyword">关键词</param>
        private static void QQMusicSearch(string keyword)
        {
            string url = string.Format("http://y.qq.com/#type=soso&p={0}",
                                       Uri.EscapeDataString(
                                           string.Format("?p=1&catZhida=1&lossless=0&t=100&utf8=1&w={0}",
                                                         Uri.EscapeDataString(keyword))));
            UrlHelper.OpenLink(url);
        }

		/// <summary>
		/// 获取用于搜索的关键词
		/// </summary>
		/// <param name="title">标题</param>
		/// <param name="artist">艺术家</param>
		/// <param name="album">专辑</param>
		/// <returns>用于搜索的关键词</returns>
		private static string GetKeyword(string title, string artist, string album)
		{
			if (album.EndsWith("..."))
			{
				album = album.Substring(0, album.Length - 3);
			}
			if (Settings.TrimBrackets)
			{
				title = TrimBrackets(title);
				artist = TrimBrackets(artist);
				album = TrimBrackets(album);
			}
			if (Settings.SearchAlbum)
			{
				return string.Format("{0} {1} {2}", title, artist, album);
			}
			else
			{
				return string.Format("{0} {1}", title, artist);
			}
		}

		/// <summary>
		/// 各种括号
		/// </summary>
		private static readonly List<char> brackets = new List<char> { '(', '（', '[', '【'};
			
		/// <summary>
		/// 剔除括号内的内容
		/// </summary>
		/// <param name="someString">任意字符串</param>
		/// <returns>剔除括号后的内容</returns>
		private static string TrimBrackets(string someString)
		{
			int index;

			foreach (var bracket in brackets)
			{
				index = someString.IndexOf(bracket);
				if (index != -1)
				{
					someString = someString.Substring(0, index);
				}
			}

			someString = someString.Trim();
			
			return someString;
		}
	}
}
