using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Interop
{
	// hook method called by system
	internal delegate IntPtr HOOKPROC(int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);
}
