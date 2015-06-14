/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using DoubanFM.Interop;
using System;
using System.Windows;
using System.Windows.Interop;
using Kfstorm.WpfExtensions;
using DwmHelper = Kfstorm.DwmHelper.DwmHelper;

namespace DoubanFM
{
    /// <summary>
    /// 窗口基类
    /// </summary>
    public class WindowBase : Window
    {
        static WindowBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowBase), new FrameworkPropertyMetadata(typeof(WindowBase)));
        }

        public WindowBase()
        {
            this.ContentRendered += delegate
            {
                //添加窗口阴影
                ShadowWindow.Attach(this);
            };

            if (DwmHelper.IsDwmSupported)
            {
                DwmHelper = new DwmHelper(this);
                DwmHelper.AeroGlassEffectChanged += DwmHelper_AeroGlassEffectChanged;
            }
        }

        protected DwmHelper DwmHelper;

        void DwmHelper_AeroGlassEffectChanged(object sender, EventArgs e)
        {
            if (DwmHelper.IsAeroGlassEffectEnabled)
            {
                DwmHelper.EnableBlurBehindWindow();
            }
            else
            {
                DwmHelper.EnableBlurBehindWindow(false);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (DwmHelper != null && DwmHelper.IsAeroGlassEffectEnabled)
            {
                DwmHelper.EnableBlurBehindWindow();
            }
        }

        /// <summary>
        /// 当前是否正在拖拽窗口
        /// </summary>
        protected bool IsDraging = false;

        /// <summary>
        /// 是否按下了鼠标左键（不要用Mouse.LeftButton去检测）
        /// </summary>
        private bool pressed = false;

        /// <summary>
        /// 鼠标相对于窗口的位置
        /// </summary>
        private Point? mousePosition;

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            pressed = true;
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseRightButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            pressed = false;
            base.OnMouseRightButtonUp(e);
        }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            mousePosition = e.GetPosition(this);
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            pressed = false;
            mousePosition = null;
            base.OnMouseLeave(e);
        }

        /// <summary>
        /// 支持拖拽窗口
        /// </summary>
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            //当鼠标没动，但有控件在鼠标下方移动时，仍会触发MouseMove事件，所以要根据鼠标位置来判断是否真的移动了。
            //只有鼠标真的移动了，才可能触发窗口拖动。
            var newPosition = e.GetPosition(this);
            bool moved = mousePosition.HasValue && newPosition != mousePosition.Value;
            mousePosition = newPosition;

            if (!IsDraging && pressed && System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                if (moved)
                {
                    IsDraging = true;
                    DragMove();
                    pressed = false;
                    IsDraging = false;
                }
            }
        }
    }
}