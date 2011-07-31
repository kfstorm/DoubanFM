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
        /// <summary>
        /// 标识Volume依赖项属性
        /// </summary>
        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register("Volume", typeof(double), typeof(VolumeControl));
        /// <summary>
        /// 标识IsMuted依赖项属性
        /// </summary>
        public static readonly DependencyProperty IsMutedProperty = DependencyProperty.Register("IsMuted", typeof(bool), typeof(VolumeControl));

        /// <summary>
        /// Gets or sets the volume.
        /// </summary>
        /// <value>
        /// The volume.
        /// </value>
        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is muted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is muted; otherwise, <c>false</c>.
        /// </value>
        public bool IsMuted 
        {
            get { return (bool)GetValue(IsMutedProperty); }
            set { SetValue(IsMutedProperty, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeControl"/> class.
        /// </summary>
        public VolumeControl()
		{
			this.InitializeComponent();
            Loudspeaker.Checked += new RoutedEventHandler(OnChecked);
            Loudspeaker.Unchecked += new RoutedEventHandler(OnUnchecked);
            slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(OnValueChanged);
		}

        /// <summary>
        /// Called when [checked].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnChecked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (Muted != null)
                Muted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when [unchecked].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnUnchecked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (Unmuted != null)
                Unmuted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when [value changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The instance containing the event data.</param>
        private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            e.Handled = true;
            if (VolumeChanged != null)
                VolumeChanged(this, new RoutedPropertyChangedEventArgs<double>(e.OldValue, e.NewValue));
        }

        /// <summary>
        /// Occurs when [muted].
        /// </summary>
        public event EventHandler<EventArgs> Muted;
        /// <summary>
        /// Occurs when [unmuted].
        /// </summary>
        public event EventHandler<EventArgs> Unmuted;
        /// <summary>
        /// Occurs when [volume changed].
        /// </summary>
        public event RoutedPropertyChangedEventHandler<double> VolumeChanged;
        
	}
}