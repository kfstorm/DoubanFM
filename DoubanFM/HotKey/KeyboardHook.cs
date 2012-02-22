using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Reflection;
using DoubanFM.Interop;

namespace DoubanFM
{
	/// <summary>
	/// 键盘钩子
	/// </summary>
	public class KeyboardHook : IDisposable
	{
		WH _hookType = WH.KEYBOARD_LL;
		IntPtr _hookHandle = IntPtr.Zero;
		HOOKPROC _hookFunction = null;


		//事件
		public delegate void HookEventHandler(object sender, HookEventArgs e);
		public event HookEventHandler KeyDown;
		public event HookEventHandler KeyUp;

		public KeyboardHook()
		{
			_hookFunction = new HOOKPROC(HookCallback);
			Install();
		}

		~KeyboardHook()
		{
			Dispose(false);
		}

		//由系统调用的钩子函数
		private IntPtr HookCallback(int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
		{
			if (code < 0)
				return NativeMethods.CallNextHookEx(_hookHandle, code, wParam, ref lParam);

			// KeyUp事件
			if ((lParam.flags & 0x80) != 0 && this.KeyUp != null)
				this.KeyUp(this, new HookEventArgs(lParam.vkCode));

			// KeyDown事件
			if ((lParam.flags & 0x80) == 0 && this.KeyDown != null)
				this.KeyDown(this, new HookEventArgs(lParam.vkCode));

			return NativeMethods.CallNextHookEx(_hookHandle, code, wParam, ref lParam);
		}

		/// <summary>
		/// 安装键盘钩子
		/// </summary>
		private void Install()
		{
			//确保当前未安装钩子
			if (_hookHandle != IntPtr.Zero)
				return;

			//安装全局钩子需要模块句柄
			Module[] list = System.Reflection.Assembly.GetExecutingAssembly().GetModules();
			System.Diagnostics.Debug.Assert(list != null && list.Length > 0);

			//安装全局钩子
			_hookHandle = NativeMethods.SetWindowsHookEx(_hookType,
				_hookFunction, NativeMethods.GetModuleHandle(list[0].FullyQualifiedName), 0);
		}

		/// <summary>
		/// 卸载键盘钩子
		/// </summary>
		private void Uninstall()
		{
			if (_hookHandle != IntPtr.Zero)
			{
				// 卸载全局钩子
				NativeMethods.UnhookWindowsHookEx(_hookHandle);
				_hookHandle = IntPtr.Zero;
			}
		}

		#region IDisposable

		bool _disposed;
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
				}
				Uninstall();
				_disposed = true;
			}
		}
		#endregion
	}
}