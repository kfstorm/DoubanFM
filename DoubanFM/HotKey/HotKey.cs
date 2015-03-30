/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * Reference : http://www.cnblogs.com/alvinyue/archive/2011/08/03/2126022.html
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Collections;
using System.Runtime.Serialization;
using System.Windows.Input;
using DoubanFM.Interop;

namespace DoubanFM
{
	/// <summary>
	/// 处理热键的类
	/// </summary>
	[Serializable]
	public class HotKey : ISerializable
	{
		#region Member
		/// <summary>
		/// 热键编号
		/// </summary>
		public int KeyId { get; private set; }
		/// <summary>
		/// 窗体句柄
		/// </summary>
		public IntPtr Handle { get; private set; }
		/// <summary>
		/// 热键所在窗体
		/// </summary>
		public Window RegisteredWindiow { get; private set; }
		/// <summary>
		/// 热键控制键
		/// </summary>
		public ControlKeys ControlKey { get; private set; }
		/// <summary>
		/// 热键主键
		/// </summary>
		public Key Key { get; private set; }

		/// <summary>
		/// 热键事件委托
		/// </summary>
		public delegate void OnHotKeyEventHandler(object sender, EventArgs e);
		/// <summary>
		/// 热键事件
		/// </summary>
		public event OnHotKeyEventHandler OnHotKey;
		/// <summary>
		/// 热键哈希表
		/// </summary>
		static Hashtable KeyPair = new Hashtable();
		/// <summary>
		/// 热键消息编号
		/// </summary>
		private const int WM_HOTKEY = 0x0312;
		/// <summary>
		/// 控制键编码
		/// </summary>
		[Flags]
		public enum ControlKeys
		{
			None = 0x0,
			Alt = 0x1,
			Ctrl = 0x2,
			Shift = 0x4,
			Win = 0x8
		}

		/// <summary>
		/// 是否已注册
		/// </summary>
		public bool IsRegistered { get; private set; }
		/// <summary>
		/// 是否已安装钩子
		/// </summary>
		public static bool IsHookAdded { get; private set; }

		#endregion

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="control">控制键</param>
		/// <param name="key">主键</param>
		public HotKey(HotKey.ControlKeys control, Key key)
		{
			ControlKey = control;
			Key = key;
			KeyId = (int)ControlKey + (int)Key * 100;
		}

		//析构函数,解除热键
		~HotKey()
		{
			if (IsRegistered) UnRegister();
		}

		#region core

		/// <summary>
		/// 注册热键
		/// </summary>
		public void Register(Window win)
		{
			if (IsRegistered) return;
			if (ControlKey == ControlKeys.None && Key == Key.None) return;
			Handle = new WindowInteropHelper(win).Handle;
			RegisteredWindiow = win;
			if (HotKey.KeyPair.ContainsKey(KeyId))
			{
				throw new Exception("热键" + this + "已经被注册!");
			}
			//注册热键
			if (false == NativeMethods.RegisterHotKey(Handle, KeyId, (uint)ControlKey, (uint)KeyInterop.VirtualKeyFromKey(Key)))
			{
				throw new Exception("热键" + this + "注册失败!");
			}
			//消息挂钩只能连接一次!!
			if (!IsHookAdded)
			{
				if (false == InstallHotKeyHook(this))
				{

					throw new Exception("消息挂钩连接失败!");
				}
				IsHookAdded = true;
			}
			//添加这个热键索引
			HotKey.KeyPair.Add(KeyId, this);
			IsRegistered = true;
			System.Diagnostics.Debug.WriteLine("热键" + this + "注册成功");
		}
		/// <summary>
		/// 注销热键
		/// </summary>
		public void UnRegister()
		{
			if (!IsRegistered) return;
			if (ControlKey == ControlKeys.None && Key == Key.None) return;
			NativeMethods.UnregisterHotKey(Handle, KeyId);
			HotKey.KeyPair.Remove(KeyId);
			OnHotKey = null;
			IsRegistered = false;
		}

		//安装热键处理挂钩
		static private bool InstallHotKeyHook(HotKey hk)
		{
			if (hk.RegisteredWindiow == null || hk.Handle == IntPtr.Zero)
			{
				return false;
			}
			//获得消息源
			System.Windows.Interop.HwndSource source = System.Windows.Interop.HwndSource.FromHwnd(hk.Handle);
			if (source == null)
			{
				return false;
			}
			//挂接事件

			source.AddHook(HotKey.HotKeyHook);

			return true;
		}
		//热键处理过程
		static private IntPtr HotKeyHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WM_HOTKEY)
			{

				HotKey hk = (HotKey)HotKey.KeyPair[(int)wParam];
				if (hk.OnHotKey != null)
				{

					hk.OnHotKey(hk, EventArgs.Empty);
				}
			}
			return IntPtr.Zero;
		}

		#endregion

		protected HotKey(SerializationInfo info, StreamingContext context)
		{
			ControlKey = (ControlKeys)info.GetValue("ControlKey", typeof(ControlKeys));
			Key = (Key)info.GetValue("Key", typeof(Key));
			KeyId = (int)ControlKey + (int)Key * 100;
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("ControlKey", ControlKey);
			info.AddValue("Key", Key);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (ControlKey != ControlKeys.None) sb.Append(ControlKey);
			if (Key != Key.None)
			{
				if (sb.Length > 0) sb.Append(" + ");
				sb.Append(Key);
			}
			return sb.ToString();
		}
	}
}
