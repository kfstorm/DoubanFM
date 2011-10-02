using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace DoubanFM
{
	/// <summary>
	/// 歌词设置
	/// </summary>
	[Serializable]
	public class LyricsSetting : DependencyObject, ISerializable
	{
		public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(LyricsSetting));
		public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(LyricsSetting), new PropertyMetadata(48.0));
		public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(LyricsSetting));

		public FontFamily FontFamily
		{
			get { return (FontFamily)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}
		public FontWeight FontWeight
		{
			get { return (FontWeight)GetValue(FontWeightProperty); }
			set { SetValue(FontWeightProperty, value); }
		}

		/// <summary>
		/// 数据保存文件夹
		/// </summary>
		private static string _dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\K.F.Storm\豆瓣电台\";
		/// <summary>
		/// 加载设置
		/// </summary>
		internal static LyricsSetting Load()
		{
			LyricsSetting setting = null;
			try
			{
				using (FileStream stream = File.OpenRead(_dataFolder + "LyricsSetting.dat"))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					setting = (LyricsSetting)formatter.Deserialize(stream);
				}
			}
			catch
			{
				setting = new LyricsSetting();
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
				using (FileStream stream = File.OpenWrite(_dataFolder + "LyricsSetting.dat"))
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, this);
				}
			}
			catch { }
		}
		public LyricsSetting()
		{

		}

		public LyricsSetting(SerializationInfo info, StreamingContext context)
		{
			LyricsSetting def = new LyricsSetting();
			try
			{
				FontFamily = new FontFamily(info.GetString("FontFamily"));
			}
			catch
			{
				FontFamily = def.FontFamily;
			}
			try
			{
				FontSize = info.GetDouble("FontSize");
			}
			catch
			{
				FontSize = def.FontSize;
			}
			try
			{
				string weight = info.GetString("FontWeight");
				switch (weight)
				{
					case "Thin":
						FontWeight = FontWeights.Thin;
						break;
					case "ExtraLight":
						FontWeight = FontWeights.ExtraLight;
						break;
					case "UltraLight":
						FontWeight = FontWeights.UltraLight;
						break;
					case "Light":
						FontWeight = FontWeights.Light;
						break;
					case "Normal":
						FontWeight = FontWeights.Normal;
						break;
					case "Regular":
						FontWeight = FontWeights.Regular;
						break;
					case "Medium":
						FontWeight = FontWeights.Medium;
						break;
					case "DemiBold":
						FontWeight = FontWeights.DemiBold;
						break;
					case "SemiBold":
						FontWeight = FontWeights.SemiBold;
						break;
					case "Bold":
						FontWeight = FontWeights.Bold;
						break;
					case "ExtraBold":
						FontWeight = FontWeights.ExtraBold;
						break;
					case "UltraBold":
						FontWeight = FontWeights.UltraBold;
						break;
					case "Black":
						FontWeight = FontWeights.Black;
						break;
					case "Heavy":
						FontWeight = FontWeights.Heavy;
						break;
					case "ExtraBlack":
						FontWeight = FontWeights.ExtraBlack;
						break;
					case "UltraBlack":
						FontWeight = FontWeights.UltraBlack;
						break;
					default:
						FontWeight = def.FontWeight;
						break;
				}
			}
			catch
			{
				FontWeight = def.FontWeight;
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (FontFamily != null)
			{
				info.AddValue("FontFamily", FontFamily.ToString());
			}
			info.AddValue("FontSize", FontSize);
			info.AddValue("FontWeight", FontWeight.ToString());
		}
	}
}
