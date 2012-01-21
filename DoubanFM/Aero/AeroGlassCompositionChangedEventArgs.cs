using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Aero
{
	/// <summary>
	/// Event argument for The GlassAvailabilityChanged event
	/// </summary>
	public class AeroGlassCompositionChangedEventArgs : EventArgs
	{
		internal AeroGlassCompositionChangedEventArgs(bool avialbility)
		{
			GlassAvailable = avialbility;
		}

		/// <summary>
		/// The new GlassAvailable state
		/// </summary>
		public bool GlassAvailable { get; private set; }

	}
}
