using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DoubanFM.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct DWM_THUMBNAIL_PROPERTIES
	{
		internal uint dwFlags;
		internal RECT rcDestination;
		internal RECT rcSource;
		internal byte opacity;
		[MarshalAs(UnmanagedType.Bool)]
		internal bool fVisible;
		[MarshalAs(UnmanagedType.Bool)]
		internal bool fSourceClientAreaOnly;

		internal const uint DWM_TNP_RECTDESTINATION = 0x00000001;
		internal const uint DWM_TNP_RECTSOURCE = 0x00000002;
		internal const uint DWM_TNP_OPACITY = 0x00000004;
		internal const uint DWM_TNP_VISIBLE = 0x00000008;
		internal const uint DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010;
	}
}
