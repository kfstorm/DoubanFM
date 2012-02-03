using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace DoubanFM.Wmi
{
	internal class WmiBase
	{
		internal static TWmiClass[] GetInstances<TWmiClass>(string className) where TWmiClass : WmiBase, new()
		{
			if (className == null)
				throw new ArgumentNullException("className");
			if (className == string.Empty)
				throw new ArgumentException("类名不能为空", "className");

			List<TWmiClass> classes = new List<TWmiClass>();
			using (var foo = new ManagementClass(className))
			{
				ManagementObjectCollection classObjects = foo.GetInstances();
				foreach (var classObject in classObjects)
				{
					TWmiClass aClass = new TWmiClass();
					foreach (var field in typeof(TWmiClass).GetFields())
					{
						object value = classObject[field.Name];
						if (field.FieldType == typeof(DateTime))
						{
							if (value is string)
							{
								DateTime time;
								if (DateTime.TryParse((string)value, out time))
								{
									field.SetValue(aClass, time);
								}
							}
						}
						else
						{
							try
							{
								field.SetValue(aClass, value);
							}
							catch { }
						}
					}
					classes.Add(aClass);
				}
			}

			return classes.ToArray();
		}
	}
}