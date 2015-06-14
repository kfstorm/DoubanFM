/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System.Reflection;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DoubanFM
{
    /// <summary>
    /// Represents a display device.
    /// </summary>
    public class Screen
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Screen" /> class.
        /// </summary>
        /// <param name="bitsPerPixel">The number of bits of memory, associated with one pixel of data.</param>
        /// <param name="bounds">The bounds of the display.</param>
        /// <param name="deviceName">The device name associated with a display.</param>
        /// <param name="primary">if set to <c>true</c> this display is primary.</param>
        /// <param name="workingArea">The working area of the display.</param>
        protected Screen(int bitsPerPixel, Rect bounds, string deviceName, bool primary, Rect workingArea)
        {
            WorkingArea = workingArea;
            Primary = primary;
            DeviceName = deviceName;
            Bounds = bounds;
            BitsPerPixel = bitsPerPixel;
        }

        static Screen()
        {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        #endregion Constructors

        #region Events

        private static void SystemEvents_UserPreferenceChanged(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            if (e.Category != UserPreferenceCategory.Desktop) return;
            if (DisplayChanged != null)
            {
                DisplayChanged(null, EventArgs.Empty);
            }
        }

        private static void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (DisplayChanged != null)
            {
                DisplayChanged(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Occurs when display changed.
        /// </summary>
        public static event EventHandler DisplayChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets an array of all displays on the system.
        /// </summary>
        /// <value>
        /// An array of type Screen, containing all displays on the system.
        /// </value>
        public static Screen[] AllScreens
        {
            get
            {
                return (from screen in System.Windows.Forms.Screen.AllScreens select ConvertToScreen(screen)).ToArray();
            }
        }

        /// <summary>
        /// Gets the number of bits of memory, associated with one pixel of data.
        /// </summary>
        /// <value>
        /// The number of bits of memory, associated with one pixel of data.
        /// </value>
        public int BitsPerPixel { get; private set; }

        /// <summary>
        /// Gets the bounds of the display.
        /// </summary>
        /// <value>
        /// A Rect, representing the bounds of the display.
        /// </value>
        public Rect Bounds { get; private set; }

        /// <summary>
        /// Gets the device name associated with a display.
        /// </summary>
        /// <value>
        /// The device name associated with a display.
        /// </value>
        public string DeviceName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a particular display is the primary device.
        /// </summary>
        /// <value>
        ///   <c>true</c> if  this display is primary; otherwise, <c>false</c>.
        /// </value>
        public bool Primary { get; private set; }

        /// <summary>
        /// Gets the primary display.
        /// </summary>
        /// <value>
        /// The primary display.
        /// </value>
        public static Screen PrimaryScreen
        {
            get
            {
                return ConvertToScreen(System.Windows.Forms.Screen.PrimaryScreen);
            }
        }

        /// <summary>
        /// Gets the working area of the display. The working area is the desktop area of the display, excluding taskbars, docked windows, and docked tool bars.
        /// </summary>
        /// <value>
        /// A Rect, representing the working area of the display.
        /// </value>
        public Rect WorkingArea { get; private set; }

        #endregion Properties

        #region Public Methods

        #region FromHandle

        /// <summary>
        /// Retrieves a Screen for the display that contains the largest portion of the object referred to by the specified handle.
        /// </summary>
        /// <param name="hwnd">The window handle for which to retrieve the Screen.</param>
        /// <returns>A Screen for the display that contains the largest region of the object. In multiple display environments where no display contains any portion of the specified window, the display closest to the object is returned.</returns>
        public static Screen FromHandle(IntPtr hwnd)
        {
            return ConvertToScreen(System.Windows.Forms.Screen.FromHandle(hwnd));
        }

        #endregion FromHandle

        #region FromPoint

        /// <summary>
        /// Retrieves a Screen for the display that contains the specified point.
        /// </summary>
        /// <param name="point">A Point that specifies the location for which to retrieve a Screen.</param>
        /// <returns>A Screen for the display that contains the point. In multiple display environments where no display contains the point, the display closest to the specified point is returned.</returns>
        public static Screen FromPoint(Point point)
        {
            return ConvertToScreen(System.Windows.Forms.Screen.FromPoint(ConvertFromPoint(point)));
        }

        #endregion FromPoint

        #region FromRect

        /// <summary>
        /// Retrieves a Screen for the display that contains the largest portion of the rectangle.
        /// </summary>
        /// <param name="rect">A rectangle that specifies the area for which to retrieve the display.</param>
        /// <returns>A Screen for the display that contains the largest region of the specified rectangle. In multiple display environments where no display contains the rectangle, the display closest to the rectangle is returned.</returns>
        public static Screen FromRect(Rect rect)
        {
            return ConvertToScreen(System.Windows.Forms.Screen.FromRectangle(ConvertFromRect(rect)));
        }

        #endregion FromRect

        #region GetBounds

        /// <summary>
        /// Retrieves the bounds of the display that contains the specified point.
        /// </summary>
        /// <param name="point">A Point that specifies the coordinates for which to retrieve the display bounds.</param>
        /// <returns>A Rect that specifies the bounds of the display that contains the specified point. In multiple display environments where no display contains the specified point, the display closest to the point is returned.</returns>
        public static Rect GetBounds(Point point)
        {
            return ConvertToRect(System.Windows.Forms.Screen.GetBounds(ConvertFromPoint(point)));
        }

        /// <summary>
        /// Retrieves the bounds of the display that contains the largest portion of the specified rectangle.
        /// </summary>
        /// <param name="rectangle">A Rect that specifies the area for which to retrieve the display bounds.</param>
        /// <returns>A Rect that specifies the bounds of the display that contains the specified rectangle. In multiple display environments where no monitor contains the specified rectangle, the monitor closest to the rectangle is returned.</returns>
        public static Rect GetBounds(Rect rectangle)
        {
            return ConvertToRect(System.Windows.Forms.Screen.GetBounds(ConvertFromRect(rectangle)));
        }

        #endregion GetBounds

        #region GetWorkingArea

        /// <summary>
        /// Retrieves the working area closest to the specified point. The working area is the desktop area of the display, excluding taskbars, docked windows, and docked tool bars.
        /// </summary>
        /// <param name="pt">A Point that specifies the coordinates for which to retrieve the working area.</param>
        /// <returns>A Rect that specifies the working area. In multiple display environments where no display contains the specified point, the display closest to the point is returned.</returns>
        public static Rect GetWorkingArea(Point pt)
        {
            return ConvertToRect(System.Windows.Forms.Screen.GetWorkingArea(ConvertFromPoint(pt)));
        }

        /// <summary>
        /// Retrieves the working area for the display that contains the largest portion of the specified rectangle. The working area is the desktop area of the display, excluding taskbars, docked windows, and docked tool bars.
        /// </summary>
        /// <param name="rect">The Rect that specifies the area for which to retrieve the working area.</param>
        /// <returns>A Rect that specifies the working area. In multiple display environments where no display contains the specified rectangle, the display closest to the rectangle is returned.</returns>
        public static Rect GetWorkingArea(Rect rect)
        {
            return ConvertToRect(System.Windows.Forms.Screen.GetWorkingArea(ConvertFromRect(rect)));
        }

        #endregion GetWorkingArea

        #endregion Public Methods

        #region Overrides

        public override int GetHashCode()
        {
            return DeviceName.GetHashCode();
        }

        public override string ToString()
        {
            return DeviceName;
        }

        #endregion Overrides

        #region Convert Between WPF and WinForm

        protected static Point ConvertToPoint(System.Drawing.Point point)
        {
            return new Point(ConvertToXCoordinate(point.X), ConvertToYCoordinate(point.Y));
        }

        protected static System.Drawing.Point ConvertFromPoint(Point point)
        {
            return new System.Drawing.Point(ConvertFromXCoordinate(point.X), ConvertFromYCoordinate(point.Y));
        }

        protected static Rect ConvertToRect(System.Drawing.Rectangle rectangle)
        {
            return new Rect(ConvertToXCoordinate(rectangle.Left), ConvertToYCoordinate(rectangle.Top),
                ConvertToXCoordinate(rectangle.Width), ConvertToYCoordinate(rectangle.Height));
        }

        protected static System.Drawing.Rectangle ConvertFromRect(Rect rectangle)
        {
            return new System.Drawing.Rectangle(ConvertFromXCoordinate(rectangle.Left), ConvertFromYCoordinate(rectangle.Top),
                ConvertFromXCoordinate(rectangle.Width), ConvertFromYCoordinate(rectangle.Height));
        }

        protected static Screen ConvertToScreen(System.Windows.Forms.Screen screen)
        {
            return new Screen(screen.BitsPerPixel, ConvertToRect(screen.Bounds), screen.DeviceName, screen.Primary, ConvertToRect(screen.WorkingArea));
        }

        /// <summary>
        /// Convert X coordinate from physical pixel to device-independent pixel.
        /// </summary>
        /// <param name="coordinate">Physical pixel</param>
        /// <returns>Device-independent pixel</returns>
        protected static double ConvertToXCoordinate(int coordinate)
        {
            return (double)coordinate * 96 / GetDpiX();
        }

        /// <summary>
        /// Convert Y coordinate from physical pixel to device-independent pixel.
        /// </summary>
        /// <param name="coordinate">Physical pixel</param>
        /// <returns>Device-independent pixel</returns>
        protected static double ConvertToYCoordinate(int coordinate)
        {
            return (double)coordinate * 96 / GetDpiY();
        }

        /// <summary>
        /// Convert X coordinate from device-independent pixel to physical pixel.
        /// </summary>
        /// <param name="coordinate">Physical pixel</param>
        /// <returns>Device-independent pixel</returns>
        protected static int ConvertFromXCoordinate(double coordinate)
        {
            return (int)(coordinate / 96 * GetDpiX());
        }

        /// <summary>
        /// Convert Y coordinate from device-independent pixel to physical pixel.
        /// </summary>
        /// <param name="coordinate">Physical pixel</param>
        /// <returns>Device-independent pixel</returns>
        protected static int ConvertFromYCoordinate(double coordinate)
        {
            return (int)(coordinate / 96 * GetDpiY());
        }

        #endregion Convert Between WPF and WinForm

        #region DPI Methods

        protected static readonly PropertyInfo dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
        protected static readonly PropertyInfo dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Get the DPI setting of X-axis.
        /// </summary>
        /// <returns>The DPI setting of X-axis.</returns>
        protected static int GetDpiX()
        {
            return (int)dpiXProperty.GetValue(null, null);
        }

        /// <summary>
        /// Get the DPI setting of Y-axis.
        /// </summary>
        /// <returns>The DPI setting of Y-axis.</returns>
        protected static int GetDpiY()
        {
            return (int)dpiYProperty.GetValue(null, null);
        }

        #endregion
    }
}