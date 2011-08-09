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
        public IEnumerable<Cate> Personal { get; private set; }
        /// <summary>
        /// 公共频道
        /// </summary>
        public IEnumerable<Cate> Public { get; private set; }
        /// <summary>
        /// DJ频道
        /// </summary>
        public IEnumerable<Cate> Dj { get; private set; }

        internal ChannelInfo(Json.ChannelInfo ci)
        {
            List<Cate> list1 = new List<Cate>();
            foreach (var cate in ci.personal)
                list1.Add(new Cate(cate));
            Personal = list1;
            List<Cate> list2 = new List<Cate>();
            foreach (var cate in ci.pppublic)
                list2.Add(new Cate(cate));
            Public = list2;
            List<Cate> list3 = new List<Cate>();
            foreach (var cate in ci.Dj)
                list3.Add(new Cate(cate));
            Dj = list3;
        }
    }
}
