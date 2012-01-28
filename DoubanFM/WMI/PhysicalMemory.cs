using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace DoubanFM.WMI
{
	public class PhysicalMemory
	{
		public string BankLabel { get; set; }
		public UInt64 Capacity { get; set; }
		public string Caption { get; set; }
		public string CreationClassName { get; set; }
		public UInt16 DataWidth { get; set; }
		public string Description { get; set; }
		public string DeviceLocator { get; set; }
		public UInt16 FormFactor { get; set; }
		public bool HotSwappable { get; set; }
		public DateTime InstallDate { get; set; }
		public UInt16 InterleaveDataDepth { get; set; }
		public UInt32 InterleavePosition { get; set; }
		public string Manufacturer { get; set; }
		public UInt16 MemoryType { get; set; }
		public string Model { get; set; }
		public string Name { get; set; }
		public string OtherIdentifyingInfo { get; set; }
		public string PartNumber { get; set; }
		public UInt32 PositionInRow { get; set; }
		public bool PoweredOn { get; set; }
		public bool Removable { get; set; }
		public bool Replaceable { get; set; }
		public string SerialNumber { get; set; }
		public string SKU { get; set; }
		public UInt32 Speed { get; set; }
		public string Status { get; set; }
		public string Tag { get; set; }
		public UInt16 TotalWidth { get; set; }
		public UInt16 TypeDetail { get; set; }
		public string Version { get; set; }

		public static PhysicalMemory[] GetInstances()
		{
			List<PhysicalMemory> memories = new List<PhysicalMemory>();
			ManagementObjectCollection memoryObjects = new ManagementClass("Win32_PhysicalMemory").GetInstances();
			foreach (var memoryObject in memoryObjects)
			{
				PhysicalMemory memory = new PhysicalMemory();
				foreach (var property in typeof(PhysicalMemory).GetProperties())
				{
					object value = memoryObject[property.Name];
					if (property.PropertyType == typeof(DateTime))
					{
						if (value is string)
						{
							DateTime time;
							if (DateTime.TryParse((string)value, out time))
							{
								property.SetValue(memory, time, null);
							}
						}
					}
					else
					{
						property.SetValue(memory, value, null);
					}
				}
				memories.Add(memory);
			}

			return memories.ToArray();
		}

		public static string FormatBytes(ulong bytes)
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