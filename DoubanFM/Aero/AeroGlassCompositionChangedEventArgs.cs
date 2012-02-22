using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Aero
{
	/// <summary>
	/// GlassAvailabilityChanged事件的参数
	/// </summary>
	public class AeroGlassCompositionChangedEventArgs : EventArgs
	{
		/// <summary>
		/// 生成 <see cref="AeroGlassCompositionChangedEventArgs"/> class 的新实例。
		/// </summary>
		/// <param name="avialbility">玻璃效果是否可用</param>
		internal AeroGlassCompositionChangedEventArgs(bool avialbility)
		{
			GlassAvailable = avialbility;
		}

		/// <summary>
		/// 玻璃效果是否可用
		/// </summary>
		public bool GlassAvailable { get; private set; }

	}
}
