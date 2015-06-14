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
using System.Windows.Shapes;
using DoubanFM.Core;
using System.Collections.ObjectModel;

namespace DoubanFM
{
	/// <summary>
	/// 分享设置窗口
	/// </summary>
	public partial class ShareSettingWindow : ChildWindowBase
	{
		public ShareSetting ShareSetting { get; private set; }

		public ShareSettingWindow(ShareSetting shareSetting)
		{
			ShareSetting = shareSetting;

			this.InitializeComponent();

			//为LbDisplayedSites添加项
			foreach (var site in Share.GetSortedSites())
			{
				if (site != Share.Sites.None)
				{
					LbDisplayedSites.Items.Add(site);
					if (ShareSetting.DisplayedSites.Contains(site))
						LbDisplayedSites.SelectedItems.Add(site);
				}
			}

			//显示的分享网站有改变
			LbDisplayedSites.SelectionChanged += delegate
			{
				ShareSetting.DisplayedSites = new List<Share.Sites>();
				foreach (Share.Sites site in LbDisplayedSites.SelectedItems)
				{
					ShareSetting.DisplayedSites.Add(site);
				}
				(Owner as DoubanFMWindow).ApplyShareSetting();
			};

			//为LbOneKeyShareSites添加项
			foreach (var site in Share.GetSortedSites())
			{
				LbOneKeyShareSites.Items.Add(site);
				if (ShareSetting.OneKeyShareSites.Contains(site))
					LbOneKeyShareSites.SelectedItems.Add(site);
			}

			//一键分享网站有改变
			LbOneKeyShareSites.SelectionChanged += delegate
			{
				ShareSetting.OneKeyShareSites = new List<Share.Sites>();
				foreach (Share.Sites site in LbOneKeyShareSites.SelectedItems)
				{
					ShareSetting.OneKeyShareSites.Add(site);
				}
				(Owner as DoubanFMWindow).ApplyShareSetting();
			};
		}
	}
}