using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Interop
{
	internal static class HWND
	{
		internal static readonly IntPtr NoTopMost = new IntPtr(-2);
		internal static readonly IntPtr IntPtrTopMost = new IntPtr(-1);
		internal static readonly IntPtr IntPtrTop = new IntPtr(0);
		internal static readonly IntPtr IntPtrBottom = new IntPtr(1);
	}
}