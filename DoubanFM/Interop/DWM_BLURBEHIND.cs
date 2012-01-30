using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DoubanFM.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct DWM_BLURBEHIND
	{
		internal uint dwFlags;
		[MarshalAs(UnmanagedType.Bool)]
		internal bool fEnable;
		internal IntPtr hRegionBlur;
		[MarshalAs(UnmanagedType.Bool)]
		internal bool fTransitionOnMaximized;

		internal const uint DWM_BB_ENABLE = 0x00000001;
		internal const uint DWM_BB_BLURREGION = 0x00000002;
		internal const uint DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004;
	}
}
