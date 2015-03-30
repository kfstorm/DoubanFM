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
	/// 所有WMI类的基类，用于获取系统中的各种信息
	/// </summary>
	internal class WmiBase
	{
		/// <summary>
		/// 获取某个WMI类的实例
		/// </summary>
		/// <typeparam name="TWmiClass">具体的WMI类</typeparam>
		/// <param name="className">在Windows中该WMI类的类名</param>
		/// <returns>一个实例数组</returns>
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