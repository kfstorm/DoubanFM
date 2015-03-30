/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using DoubanFM.Core;

namespace DoubanFM
{
	[Serializable]
	public class ShareSetting : DependencyObject, ISerializable
	{
		/// <summary>
		/// 数据保存文件夹
		/// </summary>
		private static string _dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"K.F.Storm\豆瓣电台");

		/// <summary>
		/// 是否启用一键分享
		/// </summary>
		public bool EnableOneKeyShare
		{
			get { return (bool)GetValue(EnableOneKeyShareProperty); }
			set { SetValue(EnableOneKeyShareProperty, value); }
		}

		public static readonly DependencyProperty EnableOneKeyShareProperty =
			DependencyProperty.Register("EnableOneKeyShare", typeof(bool), typeof(ShareSetting), new UIPropertyMetadata(false));

		/// <summary>
		/// 要一键分享的网站
		/// </summary>
		public List<Share.Sites> OneKeyShareSites
		{
			get { return (List<Share.Sites>)GetValue(OneKeyShareSitesProperty); }
			set { SetValue(OneKeyShareSitesProperty, value); }
		}

		public static readonly DependencyProperty OneKeyShareSitesProperty =
			DependencyProperty.Register("OneKeyShareSites", typeof(List<Share.Sites>), typeof(ShareSetting), new UIPropertyMetadata(new List<Share.Sites>()));

		/// <summary>
		/// 显示在主界面上的分享网站
		/// </summary>
		public List<Share.Sites> DisplayedSites
		{
			get { return (List<Share.Sites>)GetValue(DisplayedSitesProperty); }
			set { SetValue(DisplayedSitesProperty, value); }
		}

		public static readonly DependencyProperty DisplayedSitesProperty =
			DependencyProperty.Register("DisplayedSites", typeof(List<Share.Sites>), typeof(ShareSetting),
				new UIPropertyMetadata(new List<Share.Sites>(
					from site in (Enum.GetValues(typeof(Share.Sites)) as IEnumerable<Share.Sites>)
					where site != Share.Sites.None
					select site)));


		/// <summary>
		/// 加载设置
		/// </summary>
		internal static ShareSetting Load()
		{
			ShareSetting setting = null;
			try
			{
				using (FileStream stream = File.OpenRead(Path.Combine(_dataFolder, "ShareSetting.dat")))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					setting = (ShareSetting)formatter.Deserialize(stream);
				}
			}
			catch
			{
				setting = new ShareSetting();
			}
			return setting;
		}
		/// <summary>
		/// 保存设置
		/// </summary>
		internal void Save()
		{
			try
			{
				if (!Directory.Exists(_dataFolder))
					Directory.CreateDirectory(_dataFolder);
				using (FileStream stream = File.OpenWrite(Path.Combine(_dataFolder, "ShareSetting.dat")))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, this);
				}
			}
			catch { }
		}

		protected ShareSetting(SerializationInfo info, StreamingContext context)
		{
			ShareSetting def = new ShareSetting();

			try { EnableOneKeyShare = info.GetBoolean("EnableOneKeyShare"); }
			catch { }

			#region 兼容1.7.4及更早版本的保存方式
			bool isOld = false;
			try
			{
				OneKeyShareSites = info.GetValue("OneKeyShareSites", typeof(List<Share.Sites>)) as List<Share.Sites>;
				isOld = true;
			}
			catch
			{
				OneKeyShareSites = def.OneKeyShareSites;
			}
			try
			{
				DisplayedSites = info.GetValue("DisplayedSites", typeof(List<Share.Sites>)) as List<Share.Sites>;
				isOld = true;
			}
			catch
			{
				DisplayedSites = def.DisplayedSites;
			}
			if (isOld)
			{
				foreach (var site in Share.GetSortedSites())
				{
					//如果出现了1.7.4及之前版本中没出现的网站，则默认显示
					//这里列出的是1.7.4版本中已定义的网站列表
					if (site != Share.Sites.None
						&& site != Share.Sites.Douban
						&& site != Share.Sites.Weibo
						&& site != Share.Sites.Msn
						&& site != Share.Sites.Kaixin
						&& site != Share.Sites.Renren
						&& site != Share.Sites.TencentWeibo
						&& site != Share.Sites.Fanfou
						&& site != Share.Sites.Facebook
						&& site != Share.Sites.Twitter)
					{
						DisplayedSites.Add(site);
					}
				}
			}
			#endregion

			#region 1.7.5及更高版本的保存方式
			if (!isOld)
			{
				DisplayedSites = new List<Share.Sites>();
				OneKeyShareSites = new List<Share.Sites>();
				foreach (var site in Share.GetSortedSites())
				{
					try
					{
						bool value = info.GetBoolean("DisplayedSites_" + site);
						if (value)
						{
							DisplayedSites.Add(site);
						}
					}
					catch
					{
						//说明这是更新为新版本后新增加的网站
						DisplayedSites.Add(site);
					}

					try
					{
						bool value = info.GetBoolean("OneKeyShareSites_" + site);
						if (value)
						{
							OneKeyShareSites.Add(site);
						}
					}
					catch
					{
						//新增加的网站默认不设置一键分享
					}
				}
			}
			#endregion
		}

		public ShareSetting()
		{ }

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("EnableOneKeyShare", EnableOneKeyShare);
			foreach (var site in Share.GetSortedSites())
			{
				info.AddValue("OneKeyShareSites_" + site, OneKeyShareSites.Contains(site));
				info.AddValue("DisplayedSites_" + site, DisplayedSites.Contains(site));
			}
		}
	}
}