using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Reflection;

namespace DoubanFM.WMI
{
	public class Processor
	{
		public UInt16 AddressWidth { get; set; }
		public UInt16 Architecture { get; set; }
		public UInt16 Availability { get; set; }
		public string Caption { get; set; }
		public UInt32 ConfigManagerErrorCode { get; set; }
		public bool ConfigManagerUserConfig { get; set; }
		public UInt16 CpuStatus { get; set; }
		public string CreationClassName { get; set; }
		public UInt32 CurrentClockSpeed { get; set; }
		public UInt16 CurrentVoltage { get; set; }
		public UInt16 DataWidth { get; set; }
		public string Description { get; set; }
		public string DeviceID { get; set; }
		public bool ErrorCleared { get; set; }
		public string ErrorDescription { get; set; }
		public UInt32 ExtClock { get; set; }
		public UInt16 Family { get; set; }
		public DateTime InstallDate { get; set; }
		public UInt32 L2CacheSize { get; set; }
		public UInt32 L2CacheSpeed { get; set; }
		public UInt32 L3CacheSize { get; set; }
		public UInt32 L3CacheSpeed { get; set; }
		public UInt32 LastErrorCode { get; set; }
		public UInt16 Level { get; set; }
		public UInt16 LoadPercentage { get; set; }
		public string Manufacturer { get; set; }
		public UInt32 MaxClockSpeed { get; set; }
		public string Name { get; set; }
		public UInt32 NumberOfCores { get; set; }
		public UInt32 NumberOfLogicalProcessors { get; set; }
		public string OtherFamilyDescription { get; set; }
		public string PNPDeviceID { get; set; }
		public UInt16[] PowerManagementCapabilities { get; set; }
		public bool PowerManagementSupported { get; set; }
		public string ProcessorId { get; set; }
		public UInt16 ProcessorType { get; set; }
		public UInt16 Revision { get; set; }
		public string Role { get; set; }
		public string SocketDesignation { get; set; }
		public string Status { get; set; }
		public UInt16 StatusInfo { get; set; }
		public string Stepping { get; set; }
		public string SystemCreationClassName { get; set; }
		public string SystemName { get; set; }
		public string UniqueId { get; set; }
		public UInt16 UpgradeMethod { get; set; }
		public string Version { get; set; }
		public UInt32 VoltageCaps { get; set; }

		public static Processor[] GetInstances()
		{
			List<Processor> processors = new List<Processor>();
			ManagementObjectCollection processorObjects = new ManagementClass("Win32_Processor").GetInstances();
			foreach (var processorObject in processorObjects)
			{
				Processor processor = new Processor();
				foreach (var property in typeof(Processor).GetProperties())
				{
					object value = processorObject[property.Name];
					if (property.PropertyType == typeof(DateTime))
					{
						if (value is string)
						{
							DateTime time;
							if (DateTime.TryParse((string)value, out time))
							{
								property.SetValue(processor, time, null);
							}
						}
					}
					else
					{
						property.SetValue(processor, value, null);
					}
				}
				processors.Add(processor);
			}

			return processors.ToArray();
		}
	}
}