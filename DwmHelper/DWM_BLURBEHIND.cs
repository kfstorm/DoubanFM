using System;
using System.Runtime.InteropServices;

namespace Kfstorm.DwmHelper
{
    /// <summary>
    /// Specifies Desktop Window Manager (DWM) blur-behind properties.
    /// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct DWM_BLURBEHIND
	{
        /// <summary>
        /// A bitwise combination of DWM Blur Behind constant values that indicates which of the members of this structure have been set.
        /// </summary>
		internal uint dwFlags;
        /// <summary>
        /// TRUE to register the window handle to DWM blur behind; FALSE to unregister the window handle from DWM blur behind.
        /// </summary>
		[MarshalAs(UnmanagedType.Bool)]
		internal bool fEnable;
        /// <summary>
        /// The region within the client area where the blur behind will be applied. A NULL value will apply the blur behind the entire client area.
        /// </summary>
		internal IntPtr hRegionBlur;
        /// <summary>
        /// TRUE if the window's colorization should transition to match the maximized windows; otherwise, FALSE.
        /// </summary>
		[MarshalAs(UnmanagedType.Bool)]
		internal bool fTransitionOnMaximized;

        /// <summary>
        /// A value for the fEnable member has been specified.
        /// </summary>
		internal const uint DWM_BB_ENABLE = 0x00000001;
        /// <summary>
        /// A value for the hRgnBlur member has been specified.
        /// </summary>
		internal const uint DWM_BB_BLURREGION = 0x00000002;
        /// <summary>
        /// A value for the fTransitionOnMaximized member has been specified.
        /// </summary>
		internal const uint DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004;
	}
}
