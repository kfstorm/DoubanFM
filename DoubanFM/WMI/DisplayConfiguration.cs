using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace DoubanFM.WMI
{
	public class DisplayConfiguration
	{
		public UInt32 BitsPerPel{get; set;}
		public string Caption{get; set;}
		public string Description{get; set;}
		public string DeviceName{get; set;}
		public UInt32 DisplayFlags{get; set;}
		public UInt32 DisplayFrequency{get; set;}
		public UInt32 DitherType{get; set;}
		public string DriverVersion{get; set;}
		public UInt32 ICMIntent{get; set;}
		public UInt32 ICMMethod{get; set;}
		public UInt32 LogPixels{get; set;}
		public UInt32 PelsHeight{get; set;}
		public UInt32 PelsWidth{get; set;}
		public string SettingID{get; set;}
		public UInt32 SpecificationVersion{get; set;}

		public static DisplayConfiguration[] GetInstances()
		{
			List<DisplayConfiguration> displays = new List<DisplayConfiguration>();
			ManagementObjectCollection displayObjects = new ManagementClass("Win32_DisplayConfiguration").GetInstances();
			foreach (var displayObject in displayObjects)
			{
				DisplayConfiguration display = new DisplayConfiguration();
				foreach (var property in typeof(DisplayConfiguration).GetProperties())
				{
					object value = displayObject[property.Name];
					if (property.PropertyType == typeof(DateTime))
					{
						if (value is string)
						{
							DateTime time;
							if (DateTime.TryParse((string)value, out time))
							{
								property.SetValue(display, time, null);
							}
						}
					}
					else
					{
						property.SetValue(display, value, null);
					}
				}
				displays.Add(display);
			}

			return displays.ToArray();
		}
	}
}