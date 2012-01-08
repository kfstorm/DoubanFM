using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace DoubanFM
{
	public class HookEventArgs : EventArgs
	{
		public Key Key;
		public bool Alt;
		public bool Control;
		public bool Shift;
		public bool Windows;

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
