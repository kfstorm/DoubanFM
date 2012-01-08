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
    /// 门类
    /// </summary>
    public class Cate
    {
        /// <summary>
        /// 该门类所含频道
        /// </summary>
        public IEnumerable<Channel> Channels { get; private set; }
        /// <summary>
        /// 门类名
        /// </summary>
        public string Name { get; private set; }
        internal Cate(Json.Cate cate)
        {
            Name = cate.cate;
            List<Channel> list = new List<Channel>();
            foreach (var channel in cate.channels)
            {
                list.Add(new Channel(channel));
            }
            Channels = list;
        }
    }
}
