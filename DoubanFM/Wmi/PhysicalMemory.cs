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
	/// 包含物理内存信息的类
	/// </summary>
	internal class PhysicalMemory : WmiBase
	{
		public string BankLabel;
		public UInt64 Capacity;
		public string Caption;
		public string CreationClassName;
		public UInt16 DataWidth;
		public string Description;
		public string DeviceLocator;
		public UInt16 FormFactor;
		public bool HotSwappable;
		public DateTime InstallDate;
		public UInt16 InterleaveDataDepth;
		public UInt32 InterleavePosition;
		public string Manufacturer;
		public UInt16 MemoryType;
		public string Model;
		public string Name;
		public string OtherIdentifyingInfo;
		public string PartNumber;
		public UInt32 PositionInRow;
		public bool PoweredOn;
		public bool Removable;
		public bool Replaceable;
		public string SerialNumber;
		public string SKU;
		public UInt32 Speed;
		public string Status;
		public string Tag;
		public UInt16 TotalWidth;
		public UInt16 TypeDetail;
		public string Version;

		/// <summary>
		/// 获取此类的实例
		/// </summary>
		/// <returns>此类的实例</returns>
		internal static PhysicalMemory[] GetInstances()
		{
			return GetInstances<PhysicalMemory>("Win32_PhysicalMemory");
		}

		/// <summary>
		/// 将字节数格式化为带单位的字符串
		/// </summary>
		/// <param name="bytes">字节数</param>
		/// <returns>带单位的字符串</returns>
		internal static string FormatBytes(ulong bytes)
		{
			const ulong scale = 1024;
			string[] orders = new string[] { "GB", "MB", "KB", "B" };
			ulong max = 1;
			for (int i = 1; i < orders.Length; ++i)
			{
				max *= scale;
			}

			foreach (string order in orders)
			{
				if (bytes >= max)
					return string.Format("{0:##.##}{1}", decimal.Divide(bytes, max), order);

				max /= scale;
			}
			return "0B";
		}
	}
}