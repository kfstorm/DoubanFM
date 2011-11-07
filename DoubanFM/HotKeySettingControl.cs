using System;
using System.Collections.Generic;
using System.Linq;
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
	/// 热键设置控件
	/// </summary>
	[TemplatePartAttribute(Name = "PART_HotKeyText", Type = typeof(TextBox))]
	[TemplatePartAttribute(Name = "PART_Clear", Type = typeof(Button))]
	public class HotKeySettingControl : ContentControl
	{
		/// <summary>
		/// 标识Command依赖项属性
		/// </summary>
		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(DoubanFMWindow.Commands), typeof(HotKeySettingControl), new PropertyMetadata(DoubanFMWindow.Commands.None));
		/// <summary>
		/// 标识HotKey依赖项属性
		/// </summary>
		public static readonly DependencyProperty HotKeyProperty = DependencyProperty.Register("HotKey", typeof(HotKey), typeof(HotKeySettingControl));
		/// <summary>
		/// 标识HotKeyText依赖项属性
		/// </summary>
		public static readonly DependencyProperty HotKeyTextProperty = DependencyProperty.Register("HotKeyText", typeof(string), typeof(HotKeySettingControl));

		/// <summary>
		/// 热键将执行的命令
		/// </summary>
		public DoubanFMWindow.Commands Command
		{
			get { return (DoubanFMWindow.Commands)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <summary>
		/// 触发命令的热键
		/// </summary>
		public HotKey HotKey
		{
			get { return (HotKey)GetValue(HotKeyProperty); }
			set
			{
				SetValue(HotKeyProperty, value);
				HotKeyText = HotKey == null ? null : HotKey.ToString();
			}
		}

		/// <summary>
		/// 热键相应的文本
		/// </summary>
		public string HotKeyText
		{
			get { return (string)GetValue(HotKeyTextProperty); }
			private set
			{
				SetValue(HotKeyTextProperty, value);
				TextBox hotKeyText = this.Template.FindName("PART_HotKeyText", this) as TextBox;
				if (hotKeyText != null)
					hotKeyText.Text = HotKeyText;
			}
		}

		static HotKeySettingControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(HotKeySettingControl), new FrameworkPropertyMetadata(typeof(HotKeySettingControl)));
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			TextBox hotKeyText = this.Template.FindName("PART_HotKeyText", this) as TextBox;
			if (hotKeyText != null)
			{
				hotKeyText.Text = HotKeyText;
				hotKeyText.PreviewKeyDown += new KeyEventHandler((sender, e) =>
				{
					HotKey.ControlKeys control = HotKey.ControlKeys.None;
					Key key = Key.None;
					foreach (Key kkey in Enum.GetValues(typeof(Key)))
						try
						{
							if (Keyboard.IsKeyDown(kkey))
							{
								if (kkey == Key.LeftCtrl || kkey == Key.RightCtrl)
									control |= HotKey.ControlKeys.Ctrl;
								else if (kkey == Key.LeftShift || kkey == Key.RightShift)
									control |= HotKey.ControlKeys.Shift;
								else if (kkey == Key.LeftAlt || kkey == Key.RightAlt)
									control |= HotKey.ControlKeys.Alt;
								else if (kkey == Key.LWin || kkey == Key.RWin)
									control |= HotKey.ControlKeys.Win;
								else key = kkey;
							}
						}
						catch { }
					HotKey = new HotKey(control, key);
					e.Handled = true;
				});
			}
			Button clear = this.Template.FindName("PART_Clear", this) as Button;
			//Button clear = GetTemplateChild("PART_Clear") as Button;
			if (clear != null)
				clear.Click += new RoutedEventHandler((sender, e) =>
			{
				HotKey = null;
			});
		}
	}
}