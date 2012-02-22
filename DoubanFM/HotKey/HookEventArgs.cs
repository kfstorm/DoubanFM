using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace DoubanFM
{
	/// <summary>
	/// 键盘钩子的事件参数
	/// </summary>
	public class HookEventArgs : EventArgs
	{
		/// <summary>
		/// 按下的所有键
		/// </summary>
		public Key Key;
		/// <summary>
		/// 是否按下了Alt键
		/// </summary>
		public bool Alt;
		/// <summary>
		/// 是否按下了Control键
		/// </summary>
		public bool Control;
		/// <summary>
		/// 是否按下了Shift键
		/// </summary>
		public bool Shift;
		/// <summary>
		/// 是否按下了Windows徽标键
		/// </summary>
		public bool Windows;

		/// <summary>
		/// 生成 <see cref="HookEventArgs"/> class 的新实例。
		/// </summary>
		/// <param name="keyCode">Win32下的键码</param>
		public HookEventArgs(UInt32 keyCode)
		{
			this.Key = KeyInterop.KeyFromVirtualKey((int)keyCode);
			ModifierKeys modifier = Keyboard.Modifiers;
			this.Alt = modifier.HasFlag(ModifierKeys.Alt);
			this.Control = modifier.HasFlag(ModifierKeys.Control);
			this.Shift = modifier.HasFlag(ModifierKeys.Shift);
			this.Windows = modifier.HasFlag(ModifierKeys.Windows);
		}
	}
}
