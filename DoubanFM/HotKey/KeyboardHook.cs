using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Reflection;

namespace DoubanFM
{
	/// <summary>
	/// 键盘钩子
	/// </summary>
	public class KeyboardHook : IDisposable
	{
		NativeMethods.HookType _hookType = NativeMethods.HookType.WH_KEYBOARD_LL;
		IntPtr _hookHandle = IntPtr.Zero;
		NativeMethods.HookProc _hookFunction = null;


		// events
		public delegate void HookEventHandler(object sender, HookEventArgs e);
		public event HookEventHandler KeyDown;
		public event HookEventHandler KeyUp;

		public KeyboardHook()
		{
			_hookFunction = new NativeMethods.HookProc(HookCallback);
			Install();
		}

		~KeyboardHook()
		{
			Dispose(false);
		}

		// hook function called by system
		private IntPtr HookCallback(int code, IntPtr wParam, ref NativeMethods.KBDLLHOOKSTRUCT lParam)
		{
			if (code < 0)
				return NativeMethods.CallNextHookEx(_hookHandle, code, wParam, ref lParam);

			// KeyUp event
			if ((lParam.flags & 0x80) != 0 && this.KeyUp != null)
				this.KeyUp(this, new HookEventArgs(lParam.vkCode));

			// KeyDown event
			if ((lParam.flags & 0x80) == 0 && this.KeyDown != null)
				this.KeyDown(this, new HookEventArgs(lParam.vkCode));

			return NativeMethods.CallNextHookEx(_hookHandle, code, wParam, ref lParam);
		}

		private void Install()
		{
			// make sure not already installed
			if (_hookHandle != IntPtr.Zero)
				return;

			// need instance handle to module to create a system-wide hook
			Module[] list = System.Reflection.Assembly.GetExecutingAssembly().GetModules();
			System.Diagnostics.Debug.Assert(list != null && list.Length > 0);

			// install system-wide hook
			_hookHandle = NativeMethods.SetWindowsHookEx(_hookType,
				_hookFunction, NativeMethods.GetModuleHandle(list[0].FullyQualifiedName), 0);
		}

		private void Uninstall()
		{
			if (_hookHandle != IntPtr.Zero)
			{
				// uninstall system-wide hook
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