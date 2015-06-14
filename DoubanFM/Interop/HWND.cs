using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Interop
{
	internal static class HWND
	{
		internal static readonly IntPtr NOTOPMOST = new IntPtr(-2);
		internal static readonly IntPtr TOPMOST = new IntPtr(-1);
		internal static readonly IntPtr TOP = new IntPtr(0);
		internal static readonly IntPtr BOTTOM = new IntPtr(1);
	}
}