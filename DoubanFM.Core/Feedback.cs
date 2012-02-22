/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace DoubanFM.Core
{
	/// <summary>
	/// 反馈
	/// </summary>
	public static class Feedback
	{
		/// <summary>
		/// 产品名称
		/// </summary>
		public static readonly string ProductName;
		/// <summary>
		/// 当前版本号
		/// </summary>
		public static readonly string VersionNumber;
		
		static Feedback()
		{
			Assembly assembly = Assembly.GetEntryAssembly();
			ProductName = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute))).Product;
			VersionNumber = assembly.GetName().Version.ToString();
		}

		/// <summary>
		/// 打开反馈的网页
		/// </summary>
		public static void OpenFeedbackPage()
		{
			Parameters parameters = new Parameters();
			parameters["ProductName"] = ProductName;
			parameters["VersionNumber"] = VersionNumber;
			string url = ConnectionBase.ConstructUrlWithParameters("http://www.kfstorm.com/products/feedback.php", parameters);
			UrlHelper.OpenLink(url);
		}
	}
}
