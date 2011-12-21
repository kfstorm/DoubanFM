/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Core.Json
{
    /// <summary>
    /// 序列化用
    /// </summary>
    class SubDjCate
    {
        /// <summary>
        /// 频道编号
        /// </summary>
        public string Cate { get; set; }
        /// <summary>
        /// 节目们
        /// </summary>
        public SubDjChannel[] DjChannels { get; set; }
    }
}
