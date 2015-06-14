/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace DoubanFM
{
	/// <summary>
	/// 热键集合
	/// </summary>
	[Serializable]
	public class HotKeys : Dictionary<DoubanFMWindow.Commands, HotKey>
	{
		/// <summary>
		/// 数据保存文件夹
		/// </summary>
		private static string _dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"K.F.Storm\豆瓣电台");
		/// <summary>
		/// 当发生注册错误时发生
		/// </summary>
		public static event EventHandler<RegisterErrorEventArgs> RegisterError;
		void RaiseRegisterErrorEvent(List<Exception> exceptions)
		{
			if (RegisterError != null)
				RegisterError(this, new RegisterErrorEventArgs(exceptions));
		}

		public class RegisterErrorEventArgs : EventArgs
		{
			public List<Exception> Exceptions { get; private set; }
			
			public RegisterErrorEventArgs(List<Exception> exceptions)
				: base()
			{
				Exceptions = exceptions;
			}
		}

		/// <summary>
		/// 注册热键
		/// </summary>
		/// <param name="window">目标窗口</param>
		internal void Register(Window window)
		{
			List<Exception> exceptions = new List<Exception>();
			foreach (var keyValue in this)
			{
				try
				{
					if (!keyValue.Value.IsRegistered)
						keyValue.Value.Register(window);
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}
			if (exceptions.Count > 0)
				RaiseRegisterErrorEvent(exceptions);
		}
		/// <summary>
		/// 注销热键
		/// </summary>
		internal void UnRegister()
		{
			foreach (var keyValue in this)
			{
				if (keyValue.Value.IsRegistered)
					keyValue.Value.UnRegister();
			}
		}
		/// <summary>
		/// 加载热键设置
		/// </summary>
		internal static HotKeys Load()
		{
			HotKeys hotKeys = null;
			try
			{
				using (FileStream stream = File.OpenRead(Path.Combine(_dataFolder, "HotKeys.dat")))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					hotKeys = (HotKeys)formatter.Deserialize(stream);
				}
			}
			catch
			{
				hotKeys = new HotKeys();
			}
			return hotKeys;
		}
		/// <summary>
		/// 保存热键设置
		/// </summary>
		internal void Save()
		{
			try
			{
				if (!Directory.Exists(_dataFolder))
					Directory.CreateDirectory(_dataFolder);
				using (FileStream stream = File.OpenWrite(Path.Combine(_dataFolder, "HotKeys.dat")))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, this);
				}
			}
			catch { }
		}
		/// <summary>
		/// 添加热键，如果有重复的就替换掉
		/// </summary>
		internal void AddHotKey(DoubanFMWindow.Commands command, HotKey hotKey)
		{
			if (this.ContainsKey(command))
				if (this[command].IsRegistered) this[command].UnRegister();
			this[command] = hotKey;
		}
		/// <summary>
		/// 删除热键
		/// </summary>
		internal void Delete(DoubanFMWindow.Commands command)
		{
			if (this.ContainsKey(command))
			{
				if (this[command].IsRegistered)
					this[command].UnRegister();
				this.Remove(command);
			}
		}

		protected HotKeys(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
		public HotKeys()
			: base()
		{ }
	}
}