using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DoubanFM.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct MARGINS
	{
		internal int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight;

		internal MARGINS(int left, int top, int right, int bottom)
		{
			cxLeftWidth = left;
			cyTopHeight = top;
			cxRightWidth = right;
			cyBottomHeight = bottom;
		}
	}
}
