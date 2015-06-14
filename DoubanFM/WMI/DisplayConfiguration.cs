/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace DoubanFM.Wmi
{
	/// <summary>
	/// 包含显卡配置信息的类
	/// </summary>
	internal class DisplayConfiguration : WmiBase
	{
		public UInt32 BitsPerPel;
		public string Caption;
		public string Description;
		public string DeviceName;
		public UInt32 DisplayFlags;
		public UInt32 DisplayFrequency;
		public UInt32 DitherType;
		public string DriverVersion;
		public UInt32 ICMIntent;
		public UInt32 ICMMethod;
		public UInt32 LogPixels;
		public UInt32 PelsHeight;
		public UInt32 PelsWidth;
		public string SettingID;
		public UInt32 SpecificationVersion;

		/// <summary>
		/// 获取此类的实例
		/// </summary>
		/// <returns>此类的实例</returns>
		internal static DisplayConfiguration[] GetInstances()
		{
			return GetInstances<DisplayConfiguration>("Win32_DisplayConfiguration");
		}
	}
}