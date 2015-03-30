using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Interop
{
	internal struct KBDLLHOOKSTRUCT
	{
		internal UInt32 vkCode;
		internal UInt32 scanCode;
		internal UInt32 flags;
		internal UInt32 time;
		internal IntPtr extraInfo;
	}
}
