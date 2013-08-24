/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace Kfstorm.DwmHelper
{
    /// <summary>
    /// Encapsulated some DWM functions.
    /// </summary>
    public class DwmHelper
    {
        /// <summary>
        /// Gets a value indicating whether DWM is supported.
        /// </summary>
        /// <value>
        ///   <c>true</c> if DWM is supported; otherwise, <c>false</c>.
        /// </value>
        public static bool IsDwmSupported
        {
            get
            {
                return Environment.OSVersion.Version.Major >= 6;
            }
        }

        /// <summary>
        /// Gets a value indicating whether composition is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if composition is enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsCompositionEnabled
        {
            get
            {
                ValidateDwmIsSupported();
                bool enabled;
                NativeMethods.DwmIsCompositionEnabled(out enabled);
                return enabled;
            }
        }

        /// <summary>
        /// Gets a value indicating whether Aero glass effect is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if Aero glass effect is enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsAeroGlassEffectEnabled
        {
            get
            {
                ValidateDwmIsSupported();
                return IsCompositionEnabled && ColorizationColor.Opaque == false;
            }
        }

        /// <summary>
        /// Gets the colorization color.
        /// </summary>
        /// <value>
        /// The colorization color.
        /// </value>
        public static ColorizationColor ColorizationColor
        {
            get
            {
                ValidateDwmIsSupported();
                uint color;
                bool opaque;
                NativeMethods.DwmGetColorizationColor(out color, out opaque);
                return new ColorizationColor {Color = ColorizationColor.UInt32ToColor(color), Opaque = opaque};
            }
        }

        private readonly Window _window;

        /// <summary>
        /// Gets the the window asso associated with this DwmHelper instance.
        /// </summary>
        /// <value>
        /// The window asso associated with this DwmHelper instance。
        /// </value>
        public Window Window
        {
            get
            {
                return _window;
            }
        }

        /// <summary>
        /// Validates the DWM is supported.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">DWM is not supported by this operating system.</exception>
        protected static void ValidateDwmIsSupported()
        {
            if (!IsDwmSupported)
            {
                throw new InvalidOperationException("DWM is not supported by this operating system.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DwmHelper"/> class based on a window.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <exception cref="System.ArgumentNullException">window</exception>
        public DwmHelper(Window window)
        {
            ValidateDwmIsSupported();
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }
            _window = window;

            if (new WindowInteropHelper(Window).Handle != IntPtr.Zero)
            {
                AddWindowMessageHook();
            }
            else
            {
                window.SourceInitialized += delegate
                {
                    AddWindowMessageHook();
                };
            }
        }

        /// <summary>
        /// Adds the window message hook.
        /// </summary>
        private void AddWindowMessageHook()
        {
            var interopHelper = new WindowInteropHelper(Window);
            HwndSource source = HwndSource.FromHwnd(interopHelper.EnsureHandle());
            if (source != null) source.AddHook(WndProc);
        }

        /// <summary>
        /// Occurs when composition changed.
        /// </summary>
        public event EventHandler CompositionChanged;
        /// <summary>
        /// Occurs when colorization color changed.
        /// </summary>
        public event EventHandler ColorizationColorChanged;
        /// <summary>
        /// Occurs when Aero glass effect changed.
        /// </summary>
        public event EventHandler AeroGlassEffectChanged;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WindowMessage.WM_DWMCOMPOSITIONCHANGED)
            {
                if (CompositionChanged != null)
                {
                    CompositionChanged(this, EventArgs.Empty);
                }
                if (AeroGlassEffectChanged != null)
                {
                    AeroGlassEffectChanged(this, EventArgs.Empty);
                }
                handled = true;
            }
            if (msg == WindowMessage.WM_DWMCOLORIZATIONCOLORCHANGED)
            {
                if (ColorizationColorChanged != null)
                {
                    ColorizationColorChanged(this, EventArgs.Empty);
                }
                if (AeroGlassEffectChanged != null)
                {
                    AeroGlassEffectChanged(this, EventArgs.Empty);
                }
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Enables blur behind window.
        /// </summary>
        /// <param name="enable">Set to <c>true</c> to enable blur behind window. Set to <c>false</c> to disable blur behind window.</param>
        /// <returns><c>true</c> if succeeds; otherwise, <c>false</c>.</returns>
        public bool EnableBlurBehindWindow(bool enable = true)
        {
            if (!IsCompositionEnabled)
                return false;

            IntPtr hwnd = new WindowInteropHelper(Window).EnsureHandle();

            var bb = new DWM_BLURBEHIND
            {
                dwFlags = DWM_BLURBEHIND.DWM_BB_ENABLE | DWM_BLURBEHIND.DWM_BB_BLURREGION,
                fEnable = enable,
                hRegionBlur = IntPtr.Zero
            };

            try
            {
                NativeMethods.DwmEnableBlurBehindWindow(hwnd, ref bb);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
            finally
            {
                NativeMethods.DeleteObject(bb.hRegionBlur);
            }
        }
    }
}