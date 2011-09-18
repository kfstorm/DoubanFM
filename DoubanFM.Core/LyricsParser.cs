/**
 * 本文件原作者为Equinox
 * 代码地址：http://equinox1993.blog.163.com/blog/static/32205137201031141228418/
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DoubanFM.Core
{
	/// <summary>
	/// 表示一个LRC格式的歌词
	/// </summary>
	public class LyricsParser
	{
		#region values
		String lrcCode;
		/// <summary>
		/// 获取或设置LRC歌词代码
		/// </summary>
		public String LrcCode
		{
			get
			{
				return lrcCode;
			}
			set
			{
				lrcCode = value;
				lyricsDictionary = lrcToDictionary(lrcCode);
				timeArray = sortedKeysInDictionary(lyricsDictionary);
				try
				{
					title = lyricsDictionary["ti"].ToString();
				}
				catch { }
				try
				{
					artist = lyricsDictionary["ar"].ToString();
				}
				catch { }
				try
				{
					lyricsMaker = lyricsDictionary["by"].ToString();
				}
				catch { }
				try
				{
					album = lyricsDictionary["al"].ToString();
				}
				catch { }
				switch (isOffsetEnabled)
				{
					case true:
						if (lyricsDictionary["offset"] != null)
							int.TryParse(lyricsDictionary["offset"].ToString(), out offset);
						else
							offset = 0;
						break;
					case false:
						offset = 0;
						break;
				}
			}
		}
		int offset;
		/// <summary>
		/// 获取或设置LRC歌词的偏移
		/// </summary>
		public int Offset
		{
			get
			{
				return offset;
			}
			set
			{
				offset = value;
			}
		}
		System.Collections.Hashtable lyricsDictionary;
		/// <summary>
		/// 获取或设置歌词时间为Key,内容为Value的哈希表
		/// </summary>
		public Hashtable LyricsDictionary
		{
			get
			{
				return lyricsDictionary;
			}
			set
			{
				lyricsDictionary = value;
				timeArray = sortedKeysInDictionary(lyricsDictionary);
				try
				{
					title = lyricsDictionary["ti"].ToString();
				}
				catch { }
				try
				{
					artist = lyricsDictionary["ar"].ToString();
				}
				catch { }
				try
				{
					lyricsMaker = lyricsDictionary[@"by"].ToString();
				}
				catch { }
				try
				{
					album = lyricsDictionary["al"].ToString();
				}
				catch { }
				switch (isOffsetEnabled)
				{
					case true:
						if (lyricsDictionary["offset"] != null)
							int.TryParse(lyricsDictionary["offset"].ToString(), out offset);
						else
							offset = 0;
						break;
					case false:
						offset = 0;
						break;
				}
			}
		}
		System.Collections.ArrayList timeArray;
		String currentLyrics;
		/// <summary>
		/// 返回当前的歌词,使用前请先调用Refresh()函数
		/// </summary>
		public String CurrentLyrics
		{
			get
			{
				return currentLyrics;
			}
			set
			{
				currentLyrics = value;
			}
		}
		String nextLyrics;
		/// <summary>
		/// 返回下一个歌词,使用前请先调用Refresh()函数
		/// </summary>
		public String NextLyrics
		{
			get
			{
				return nextLyrics;
			}
			set
			{
				nextLyrics = value;
			}
		}
		String previousLyrics;
		/// <summary>
		/// 返回上一个歌词,使用前请先调用Refresh()函数
		/// </summary>
		public String PreviousLyrics
		{
			get
			{
				return previousLyrics;
			}
			set
			{
				previousLyrics = value;
			}
		}
		int currentIndex;
		/// <summary>
		/// 返回当前歌词的Index,使用前请先调用Refresh()函数
		/// </summary>
		public int CurrentIndex
		{
			get
			{
				return currentIndex;
			}
			set
			{
				currentIndex = value;
			}
		}
		String title;
		/// <summary>
		/// 返回歌词的标题
		/// </summary>
		public String Title
		{
			get
			{
				return title;
			}
		}
		String album;
		/// <summary>
		/// 返回歌词的专辑名称
		/// </summary>
		public String Album
		{
			get
			{
				return album;
			}
		}
		String artist;
		/// <summary>
		/// 返回歌词的表演者
		/// </summary>
		public String Artist
		{
			get
			{
				return artist;
			}
		}
		String lyricsMaker;
		/// <summary>
		/// 返回歌词的制作者
		/// </summary>
		public String LyricsMaker
		{
			get
			{
				return lyricsMaker;
			}
		}
		bool isOffsetEnabled;
		/// <summary>
		/// 获取或设置是否启动偏移
		/// </summary>
		public bool OffsetEnabled
		{
			set
			{
				switch (value)
				{
					case true:
						offset = (int)lyricsDictionary["offset"];
						break;
					case false:
						offset = 0;
						break;
				}
				isOffsetEnabled = value;
			}
			get
			{
				return isOffsetEnabled;
			}
		}
		#endregion
		#region build
		/// <summary>
		/// 初始化新的LyricParser实例
		/// </summary>
		public LyricsParser()
		{
			OffsetEnabled = true;
			offset = 0;
			currentIndex = 0;
		}
		/// <summary>
		/// 通过指定的Lrc文件初始化LyricParser实例
		/// </summary>
		/// <param name="filePath">文件路径</param>
		/// <param name="enc">文件编码</param>
		public LyricsParser(String filePath, Encoding enc)
		{
			isOffsetEnabled = true;
			offset = 0;
			currentIndex = 0;
			StreamReader streamReader = new StreamReader(filePath, enc);
			LrcCode = streamReader.ReadToEnd();
		}
		/// <summary>
		///  通过指定的Lrc代码初始化LyricParser实例
		/// </summary>
		/// <param name="code">Lrc代码</param>
		public LyricsParser(String code)
		{
			isOffsetEnabled = true;
			offset = 0;
			currentIndex = 0;
			LrcCode = code;
		}
		#endregion
		#region protected functions
		protected static Hashtable lrcToDictionary(String lrc)
		{
			String lrct = lrc.Replace("\r", "\n");
			Hashtable md = new Hashtable();
			String aline;
			String[] av = lrct.Split('\n');
			int i;
			for (i = 0; i < av.GetLength(0); i++)
			{
				if (av[i] != "")
				{
					aline = av[i].Replace("[", "");
					if (aline.IndexOf("]") != -1)
					{
						if (aline.Split(']').GetLength(0) == 2)
						{
							if (aline.IndexOf("ti:") != -1
							|| aline.IndexOf("ar:") != -1
							|| aline.IndexOf("al:") != -1
							|| aline.IndexOf("by:") != -1
							|| aline.IndexOf("offset:") != -1)
							{
								aline = aline.Replace("]", "");
								md[aline.Split(':').GetValue(0)] = aline.Split(':').GetValue(1);
							}
							else
							{
								md[aline.Split(']').GetValue(0)] = aline.Split(']').GetValue(1);
							}
						}
						else
						{
							int subi;
							for (subi = 0; subi < aline.Split(']').GetLength(0); subi++)
							{
								if (subi < aline.Split(']').GetLength(0) - 1)
									md[aline.Split(']').GetValue(subi)] = aline.Split(']').GetValue(aline.Split(']').GetLength(0) - 1);
							}
						}
					}
				}
			}
			return md;
		}
		protected static ArrayList sortedKeysInDictionary(Hashtable dictionary)
		{
			String[] av = new String[dictionary.Keys.Count];
			ArrayList al;
			dictionary.Keys.CopyTo(av, 0);
			al = new ArrayList(av);
			al.Sort();
			return al;
		}
		protected static String intervalToString(double interval)
		{
			int min;
			float sec;
			min = (int)interval / 60;
			sec = (float)(interval - (float)min * 60.0);
			String smin = String.Format("{0:d2}", min);
			String ssec = String.Format("{0:00.00}", sec);
			return smin + ":" + ssec;
		}
		protected static double stringToInterval(String str)
		{
			try
			{
				double min = double.Parse(str.Split(':').GetValue(0).ToString());
				double sec = double.Parse(str.Split(':').GetValue(1).ToString());
				return min * 60.0 + sec;
			}
			catch
			{
				return uint.MaxValue;
			}
		}
		#endregion
		#region refresh functions
		/// <summary>
		/// 使用指定的时间刷新实例的当前歌词
		/// </summary>
		/// <param name="time">时间</param>
		public void Refresh(double time)
		{
			if (time - (double)offset / 1000.0 >= stringToInterval(timeArray[currentIndex].ToString()) && time - (double)offset / 1000.0 < stringToInterval(timeArray[currentIndex + 1].ToString()))
			{
				currentLyrics = lyricsDictionary[timeArray[currentIndex]].ToString();
				if (currentIndex + 1 < timeArray.Count)
					nextLyrics = lyricsDictionary[timeArray[currentIndex + 1]].ToString();
				if (currentIndex - 1 >= 0)
					previousLyrics = lyricsDictionary[timeArray[currentIndex - 1]].ToString();
			}
			else if (time - (double)offset / 1000.0 >= stringToInterval(timeArray[currentIndex + 1].ToString()) && time - (double)offset / 1000.0 < stringToInterval(timeArray[currentIndex + 2].ToString()))
			{
				currentIndex++;
				currentLyrics = lyricsDictionary[timeArray[currentIndex]].ToString();
				if (currentIndex + 1 < timeArray.Count)
					nextLyrics = lyricsDictionary[timeArray[currentIndex + 1]].ToString();
				if (currentIndex - 1 >= 0)
					previousLyrics = lyricsDictionary[timeArray[currentIndex - 1]].ToString();
			}
			else
			{
				int i;
				for (i = 0; i < timeArray.Count; i++)
				{
					if (time - (double)offset / 1000.0 >= stringToInterval(timeArray[i].ToString()) && time - (double)offset / 1000.0 < stringToInterval(timeArray[i + 1].ToString()))
					{
						currentIndex = i;
						currentLyrics = lyricsDictionary[timeArray[i]].ToString();
						if (i + 1 < timeArray.Count)
							nextLyrics = lyricsDictionary[timeArray[i + 1]].ToString();
						if (i - 1 >= 0)
							previousLyrics = lyricsDictionary[timeArray[i - 1]].ToString();
						break;
					}
				}
			}
		}
		/// <summary>
		/// 使用指定的时间字符串刷新实例的当前歌词,格式为"mm:ss.ss"
		/// </summary>
		/// <param name="aString">时间字符串</param>
		public void Refresh(String aString)
		{
			Refresh(stringToInterval(aString));
		}
		#endregion
		#region directly get lyrics functions
		/// <summary>
		/// 直接获取指定时间点的歌词,若没有此时间点则返回null
		/// </summary>
		/// <param name="time">时间点</param>
		/// <returns>歌词</returns>
		public String Lyrics(double time)
		{
			try
			{
				return lyricsDictionary[intervalToString(time - (double)offset / 1000.0)].ToString();
			}
			catch
			{
				return null;
			}
		}
		/// <summary>
		/// 直接获取指定时间点字符串的歌词,若没有此时间点则返回null
		/// </summary>
		/// <param name="time">时间点字符串</param>
		/// <returns>歌词</returns>
		public String Lyrics(String aString)
		{
			try
			{
				return lyricsDictionary[intervalToString(stringToInterval(aString) - (double)offset / 1000.0)].ToString();
			}
			catch
			{
				return null;
			}
		}
		/// <summary>
		/// 获取指定索引的歌词
		/// </summary>
		/// <param name="index">索引</param>
		/// <returns>歌词</returns>
		public String LyricsAtIndex(int index)
		{
			try
			{
				return lyricsDictionary[timeArray[index]].ToString();
			}
			catch
			{
				return null;
			}
		}
		#endregion
	}
}