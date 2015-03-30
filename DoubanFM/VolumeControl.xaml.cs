/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DoubanFM
{
	/// <summary>
	/// VolumeControl.xaml 的交互逻辑
	/// </summary>
	public partial class VolumeControl : UserControl
	{
        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register("Volume", typeof(double), typeof(VolumeControl));
        public static readonly DependencyProperty IsMutedProperty = DependencyProperty.Register("IsMuted", typeof(bool), typeof(VolumeControl));

        /// <summary>
        /// 音量
        /// </summary>
        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }
        /// <summary>
        /// 是否静音
        /// </summary>
		public bool IsMuted 
        {
            get { return (bool)GetValue(IsMutedProperty); }
            set { SetValue(IsMutedProperty, value); }
        }

        public VolumeControl()
		{
			this.InitializeComponent();
            Loudspeaker.Checked += new RoutedEventHandler(OnChecked);
            Loudspeaker.Unchecked += new RoutedEventHandler(OnUnchecked);
            slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(OnValueChanged);
		}

		private void OnChecked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (Muted != null)
                Muted(this, EventArgs.Empty);
        }

        private void OnUnchecked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (Unmuted != null)
                Unmuted(this, EventArgs.Empty);
        }

        private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            e.Handled = true;
            if (VolumeChanged != null)
                VolumeChanged(this, new RoutedPropertyChangedEventArgs<double>(e.OldValue, e.NewValue));
        }

		/// <summary>
		/// 当静音时发生。
		/// </summary>
        public event EventHandler<EventArgs> Muted;
		/// <summary>
		/// 当取消静音时发生。
		/// </summary>
        public event EventHandler<EventArgs> Unmuted;
		/// <summary>
		/// 当音量改变时发生。
		/// </summary>
        public event RoutedPropertyChangedEventHandler<double> VolumeChanged;
        
	}
}