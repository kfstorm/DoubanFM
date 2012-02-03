using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace DoubanFM.Wmi
{
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

		internal static DisplayConfiguration[] GetInstances()
		{
			return GetInstances<DisplayConfiguration>("Win32_DisplayConfiguration");
		}
	}
}