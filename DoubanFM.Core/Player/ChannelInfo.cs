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
    /// 频道列表
    /// </summary>
    public class ChannelInfo
    {
        /// <summary>
        /// 私人频道
        /// </summary>
        public IEnumerable<Channel> Personal { get; private set; }
        /// <summary>
        /// 公共频道
        /// </summary>
		public IEnumerable<Channel> Public { get; private set; }
        /// <summary>
        /// DJ频道
        /// </summary>
		public IEnumerable<Channel> Dj { get; private set; }

        /// <summary>
        /// 是否是有效的频道列表
        /// </summary>
        public bool IsEffective { get { return Personal != null && Public != null && Public.Count() > 0 && Dj != null; } }

        internal ChannelInfo(Json.ChannelInfo ci)
        {
            if (ci == null) return;
			List<Channel> list1 = new List<Channel>();
			if (ci.personal != null)
			{
				foreach (var channel in ci.personal)
				{
					Channel ch = new Channel(channel);
					if (ch.IsEffective)
					{
						list1.Add(ch);
					}
				}
			}
            Personal = list1;
			List<Channel> list2 = new List<Channel>();
			if (ci.Public != null)
			{
				foreach (var channel in ci.Public)
				{
					Channel ch = new Channel(channel);
					if (ch.IsEffective)
					{
						list2.Add(ch);
					}
				}
			}
            Public = list2;
			List<Channel> list3 = new List<Channel>();
			if (ci.dj != null)
			{
				foreach (var channel in ci.dj)
				{
					Channel ch = new Channel(channel);
					if (ch.IsEffective)
					{
						list3.Add(ch);
					}
				}
			}
            Dj = list3;
        }
    }
}
