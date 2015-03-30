using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace Hardcodet.Wpf.TaskbarNotification.Interop
{
  /// <summary>
  /// Win API struct providing coordinates for a single point.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct Point
  {
    public int X;
    public int Y;

    public System.Windows.Point ToWpfPoint()
    {
        var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
        var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

        var dpiX = (int)dpiXProperty.GetValue(null, null);
        var dpiY = (int)dpiYProperty.GetValue(null, null);
        return new System.Windows.Point((double)X * 96 / dpiX,
          (double)Y * 96 / dpiY);
      
    }
  }
}