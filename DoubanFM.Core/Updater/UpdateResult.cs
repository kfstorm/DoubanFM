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
	/// 更新结果
	/// </summary>
	public class UpdateResult
	{
		/// <summary>
		/// 较新的产品列表
		/// </summary>
		public Products Products { get; set; }
		/// <summary>
		/// 错误信息
		/// </summary>
		public string Error { get; set; }
	}
}
