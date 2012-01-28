using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace DoubanFM.WMI
{
	public class SoundDevice
	{
		public UInt16 Availability { get; set; }
		public string Caption { get; set; }
		public UInt32 ConfigManagerErrorCode { get; set; }
		public bool ConfigManagerUserConfig { get; set; }
		public string CreationClassName { get; set; }
		public string Description { get; set; }
		public string DeviceID { get; set; }
		public UInt16 DMABufferSize { get; set; }
		public bool ErrorCleared { get; set; }
		public string ErrorDescription { get; set; }
		public DateTime InstallDate { get; set; }
		public UInt32 LastErrorCode { get; set; }
		public string Manufacturer { get; set; }
		public UInt32 MPU401Address { get; set; }
		public string Name { get; set; }
		public string PNPDeviceID { get; set; }
		public UInt16[] PowerManagementCapabilities { get; set; }
		public bool PowerManagementSupported { get; set; }
		public string ProductName { get; set; }
		public string Status { get; set; }
		public UInt16 StatusInfo { get; set; }
		public string SystemCreationClassName { get; set; }
		public string SystemName { get; set; }

		public static SoundDevice[] GetInstances()
		{
			List<SoundDevice> devices = new List<SoundDevice>();
			ManagementObjectCollection deviceObjects = new ManagementClass("Win32_SoundDevice").GetInstances();
			foreach (var deviceObject in deviceObjects)
			{
				SoundDevice device = new SoundDevice();
				foreach (var property in typeof(SoundDevice).GetProperties())
				{
					object value = deviceObject[property.Name];
					if (property.PropertyType == typeof(DateTime))
					{
						if (value is string)
						{
							DateTime time;
							if (DateTime.TryParse((string)value, out time))
							{
								property.SetValue(device, time, null);
							}
						}
					}
					else
					{
						property.SetValue(device, value, null);
					}
				}
				devices.Add(device);
			}

			return devices.ToArray();
		}
	}
}