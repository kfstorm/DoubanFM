/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * Reference : http://equinox1993.blog.163.com/blog/static/32205137201031141228418/
 * */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DoubanFM.Core
{
    /// <summary>
    /// 表示一个LRC格式的歌词
    /// </summary>
    public class Lyrics
    {
        #region values

        /// <summary>
        /// 获取LRC歌词代码
        /// </summary>
        public string LrcCode { get; private set; }

        /// <summary>
        /// 获取原始字典
        /// </summary>
        public Dictionary<string, string> Dictionary { get; private set; }

        /// <summary>
        /// 时间、歌词字典
        /// </summary>
        public Dictionary<TimeSpan, string> TimeAndLyrics { get; set; }

        /// <summary>
        /// 返回歌词的标题
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// 返回歌词的专辑名称
        /// </summary>
        public string Album { get; private set; }

        /// <summary>
        /// 返回歌词的表演者
        /// </summary>
        public string Artist { get; private set; }

        /// <summary>
        /// 返回歌词的制作者
        /// </summary>
        public string LyricsMaker { get; private set; }

        /// <summary>
        /// 获取LRC歌词的偏移
        /// </summary>
        public TimeSpan Offset { get; private set; }

        #endregion

        #region build

        /// <summary>
        /// 通过指定的Lrc代码初始化LyricParser实例
        /// </summary>
        /// <param name="code">Lrc代码</param>
        public Lyrics(string code)
        {
            try
            {
                LrcCode = code;
                LrcCodeParse();
                DictionaryParse();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        #endregion

        #region protected functions

        /// <summary>
        /// 第一次处理，生成原始字典
        /// </summary>
        protected void LrcCodeParse()
        {
            Dictionary = new Dictionary<string, string>();
            string[] lines = LrcCode.Replace(@"\'", "'").Split(new char[2] {'\r', '\n'});
            int i;
            for (i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    var mc = Regex.Matches(lines[i],
                                           @"(?'titles'\[(\d+:\d+(\.\d+)?|(ti|ar|al|by|offset):.*?)\])+(?'content'.+?(?=\[(\d+:\d+(\.\d+)?|(ti|ar|al|by|offset):.*?)\])|.*$)",
                                           RegexOptions.None);
                    foreach (Match ma in mc)
                    {
                        if (ma.Success)
                        {
                            string content = ma.Groups["content"].Value;
                            foreach (Capture title in ma.Groups["titles"].Captures)
                                Dictionary[title.Value] = content; //不要用Add方法，有可能有重复项
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 第二次处理，生成时间、歌词字典，找到歌词的作者等属性
        /// </summary>
        protected void DictionaryParse()
        {
            TimeAndLyrics = new Dictionary<TimeSpan, string>();
            foreach (var keyvalue in Dictionary)
            {
                {
                    //分析时间
                    Match mc = Regex.Match(keyvalue.Key, @"\[(?'minutes'\d+):(?'seconds'\d+(\.\d+)?)\]",
                                           RegexOptions.None);
                    if (mc.Success)
                    {
                        int minutes = int.Parse(mc.Groups["minutes"].Value);
                        double seconds = double.Parse(mc.Groups["seconds"].Value);
                        TimeSpan key = new TimeSpan(0, 0, minutes, (int) Math.Floor(seconds),
                                                    (int) ((seconds - Math.Floor(seconds))*1000));
                        string value = keyvalue.Value;
                        TimeAndLyrics[key] = value;
                    }
                }
                {
                    //分析歌词的附带属性
                    Match mc = Regex.Match(keyvalue.Key, @"\[(?'title'.+?):(?'content'.*)\]", RegexOptions.None);
                    if (mc.Success)
                    {
                        string title = mc.Groups["title"].Value.ToLower();
                        string content = mc.Groups["content"].Value;
                        if (title == "ti") Title = content;
                        if (title == "ar") Artist = content;
                        if (title == "al") Album = content;
                        if (title == "by") LyricsMaker = content;
                        if (title == "offset") Offset = new TimeSpan(10000*int.Parse(content));
                    }
                }
            }
        }

        #endregion

    }
}