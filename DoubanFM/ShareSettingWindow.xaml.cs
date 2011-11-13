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
	/// ShareSettingWindow.xaml 的交互逻辑
	/// </summary>
	public partial class ShareSettingWindow : ChildWindowBase
	{
		public ShareSetting ShareSetting { get; private set; }

		public ShareSettingWindow(ShareSetting shareSetting)
		{
			ShareSetting = shareSetting;

			this.InitializeComponent();

			foreach (var site in Enum.GetValues(typeof(Share.Sites)) as IEnumerable<Share.Sites>)
			{
				if (site == Share.Sites.None) continue;
				TextBlock tb = new TextBlock();
				tb.Text = Share.SiteName[site];
				tb.Tag = site;
				LbDisplayedSites.Items.Add(tb);
				if (ShareSetting.DisplayedSites.Contains(site))
					LbDisplayedSites.SelectedItems.Add(tb);
			}

			LbDisplayedSites.SelectionChanged += delegate
			{
				ShareSetting.DisplayedSites = new List<Share.Sites>();
				foreach (FrameworkElement tb in LbDisplayedSites.SelectedItems)
				{
					ShareSetting.DisplayedSites.Add((Share.Sites)tb.Tag);
				}
				(Owner as DoubanFMWindow).ApplyShareSetting();
			};

			foreach (var site in Enum.GetValues(typeof(Share.Sites)) as IEnumerable<Share.Sites>)
			{
				TextBlock tb = new TextBlock();
				tb.Text = Share.SiteName[site];
				tb.Tag = site;
				LbOneKeyShareSites.Items.Add(tb);
				if (ShareSetting.OneKeyShareSites.Contains(site))
					LbOneKeyShareSites.SelectedItems.Add(tb);
			}

			LbOneKeyShareSites.SelectionChanged += delegate
			{
				ShareSetting.OneKeyShareSites = new List<Share.Sites>();
				foreach (TextBlock tb in LbOneKeyShareSites.SelectedItems)
				{
					ShareSetting.OneKeyShareSites.Add((Share.Sites)tb.Tag);
				}
			};
		}
	}
}