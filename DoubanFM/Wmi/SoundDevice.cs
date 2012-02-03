using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace DoubanFM.Wmi
{
	internal class SoundDevice : WmiBase
	{
		public UInt16 Availability;
		public string Caption;
		public UInt32 ConfigManagerErrorCode;
		public bool ConfigManagerUserConfig;
		public string CreationClassName;
		public string Description;
		public string DeviceID;
		public UInt16 DMABufferSize;
		public bool ErrorCleared;
		public string ErrorDescription;
		public DateTime InstallDate;
		public UInt32 LastErrorCode;
		public string Manufacturer;
		public UInt32 MPU401Address;
		public string Name;
		public string PNPDeviceID;
		public UInt16[] PowerManagementCapabilities;
		public bool PowerManagementSupported;
		public string ProductName;
		public string Status;
		public UInt16 StatusInfo;
		public string SystemCreationClassName;
		public string SystemName;

		internal static SoundDevice[] GetInstances()
		{
			return GetInstances<SoundDevice>("Win32_SoundDevice");
		}
	}
}