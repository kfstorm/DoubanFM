using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Core
{
	/// <summary>
	/// 搜索结果中的项
	/// </summary>
	public class SearchItem
	{
		/// <summary>
		/// 标题
		/// </summary>
		public string Title { get; private set; }
		/// <summary>
		/// 图片
		/// </summary>
		public string Picture { get; private set; }
		/// <summary>
		/// 链接
		/// </summary>
		public string Link { get; private set; }
		/// <summary>
		/// 其他信息
		/// </summary>
		public string[] Infomations { get; private set; }
		/// <summary>
		/// 是否是艺术家
		/// </summary>
		public bool IsArtist { get; private set; }
		/// <summary>
		/// 上下文
		/// </summary>
		public string Context { get; private set; }
		/// <summary>
		/// 能否收听此项
		/// </summary>
		public bool CanContextPlay
		{
			get { return Context != null && Context.Length > 0; }
		}
		
		internal SearchItem(string title, string picture, string link, string[] infomations, bool isArtist, string context)
		{
			Title = title;
			Picture = picture;
			Link = link;
			Infomations = infomations;
			IsArtist = isArtist;
			Context = context;
		}

		/// <summary>
		/// 返回一个频道
		/// </summary>
		/// <returns></returns>
		public Channel GetChannel()
		{
			if (CanContextPlay) return new Channel(Channel.PersonalId, Title, null, Context);
			else return null;
		}
	}
}
