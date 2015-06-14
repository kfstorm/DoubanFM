/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

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
using System.Windows.Shapes;

namespace DoubanFM
{
	/// <summary>
	/// 热键设置窗口
	/// </summary>
	public partial class HotKeySettingWindow : ChildWindowBase
	{
		/// <summary>
		/// 热键设置，关闭窗口前为默认设置，关闭窗口后更新为新设置
		/// </summary>
		internal HotKeys HotKeys { get; private set; }
		
		internal HotKeySettingWindow(DoubanFMWindow owner, HotKeys hotKeys)
		{
			InitializeComponent();

			HotKeys = hotKeys;
            foreach (var child in HotKeysGrid.Children)
			{
				if (child is HotKeySettingControl)
				{
					HotKeySettingControl setting = child as HotKeySettingControl;
					if (hotKeys.ContainsKey(setting.Command))
						setting.HotKey = hotKeys[setting.Command];
				}
			}
		}

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			HotKeys.Clear();
            foreach (var child in HotKeysGrid.Children)
			{
				if (child is HotKeySettingControl)
				{
					HotKeySettingControl setting = child as HotKeySettingControl;
					if (setting.HotKey != null)
						HotKeys.Add(setting.Command, setting.HotKey);
				}
			}
			this.Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}