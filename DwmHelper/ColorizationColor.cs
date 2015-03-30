/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System.Windows.Media;

namespace Kfstorm.DwmHelper
{
    /// <summary>
    /// The current color used for Desktop Window Manager (DWM) glass composition.
    /// </summary>
    public class ColorizationColor
    {
        /// <summary>
        /// Convert a 0xAARRGGBB formed color to System.Widnows.Media.Color object.
        /// </summary>
        /// <param name="color">The 0xAARRGGBB formed color.</param>
        /// <returns></returns>
        internal static Color UInt32ToColor(uint color)
        {
            var a = (byte)((color & 0xFF000000u) >> 24);
            var r = (byte)((color & 0x00FF0000u) >> 16);
            var g = (byte)((color & 0x0000FF00u) >> 8);
            var b = (byte)((color & 0x000000FFu));
            return Color.FromArgb(a, r, g, b); 
        }
        /// <summary>
        /// The current color
        /// </summary>
        public Color Color { get; set; }
        /// <summary>
        /// whether glass is opaque.
        /// </summary>
        /// <value>
        ///   <c>true</c> if glass is opaque; otherwise, <c>false</c>.
        /// </value>
        public bool Opaque { get; set; }
    }
}
