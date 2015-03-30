using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DoubanFM.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct RECT
	{
		internal int left, top, right, bottom;

		internal RECT(int left, int top, int right, int bottom)
		{
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}
	}
}
