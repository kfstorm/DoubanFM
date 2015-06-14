using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Interop
{
	internal static class WS
	{
		internal const int OVERLAPPED = 0x00000000;
		internal const int POPUP = (int)-0x80000000;
		internal const int CHILD = 0x40000000;
		internal const int MINIMIZE = 0x20000000;
		internal const int VISIBLE = 0x10000000;
		internal const int DISABLED = 0x08000000;
		internal const int CLIPSIBLINGS = 0x04000000;
		internal const int CLIPCHILDREN = 0x02000000;
		internal const int MAXIMIZE = 0x01000000;
		internal const int CAPTION = BORDER | DLGFRAME;
		internal const int BORDER = 0x00800000;
		internal const int DLGFRAME = 0x00400000;
		internal const int VSCROLL = 0x00200000;
		internal const int HSCROLL = 0x00100000;
		internal const int SYSMENU = 0x00080000;
		internal const int THICKFRAME = 0x00040000;
		internal const int GROUP = 0x00020000;
		internal const int TABSTOP = 0x00010000;

		internal const int MINIMIZEBOX = 0x00020000;
		internal const int MAXIMIZEBOX = 0x00010000;

		internal const int TILED = OVERLAPPED;
		internal const int ICONIC = MINIMIZE;
		internal const int SIZEBOX = THICKFRAME;
		internal const int TILEDWINDOW = OVERLAPPEDWINDOW;

		// Common Window Styles
		internal const int OVERLAPPEDWINDOW = OVERLAPPED | CAPTION | SYSMENU | THICKFRAME | MINIMIZEBOX | MAXIMIZEBOX;
		internal const int POPUPWINDOW = POPUP | BORDER | SYSMENU;
		internal const int CHILDWINDOW = CHILD;

		#region WS_EX
		internal static class EX
		{
			internal const int DLGMODALFRAME = 0x00000001;
			internal const int NOPARENTNOTIFY = 0x00000004;
			internal const int TOPMOST = 0x00000008;
			internal const int ACCEPTFILES = 0x00000010;
			internal const int TRANSPARENT = 0x00000020;

			//#if(WINVER >= 0x0400)
			internal const int MDICHILD = 0x00000040;
			internal const int TOOLWINDOW = 0x00000080;
			internal const int WINDOWEDGE = 0x00000100;
			internal const int CLIENTEDGE = 0x00000200;
			internal const int CONTEXTHELP = 0x00000400;

			internal const int RIGHT = 0x00001000;
			internal const int LEFT = 0x00000000;
			internal const int RTLREADING = 0x00002000;
			internal const int LTRREADING = 0x00000000;
			internal const int LEFTSCROLLBAR = 0x00004000;
			internal const int RIGHTSCROLLBAR = 0x00000000;

			internal const int CONTROLPARENT = 0x00010000;
			internal const int STATICEDGE = 0x00020000;
			internal const int APPWINDOW = 0x00040000;

			internal const int OVERLAPPEDWINDOW = (WINDOWEDGE | CLIENTEDGE);
			internal const int PALETTEWINDOW = (WINDOWEDGE | TOOLWINDOW | TOPMOST);
			//#endif /* WINVER >= 0x0400 */

			//#if(_WIN32_WINNT >= 0x0500)
			internal const int LAYERED = 0x00080000;
			//#endif /* _WIN32_WINNT >= 0x0500 */

			//#if(WINVER >= 0x0500)
			internal const int NOINHERITLAYOUT = 0x00100000; // Disable inheritence of mirroring by children
			internal const int LAYOUTRTL = 0x00400000; // Right to left mirroring
			//#endif /* WINVER >= 0x0500 */

			//#if(_WIN32_WINNT >= 0x0500)
			internal const int COMPOSITED = 0x02000000;
			internal const int NOACTIVATE = 0x08000000;
			//#endif /* _WIN32_WINNT >= 0x0500 */
		}
		#endregion
	}
}