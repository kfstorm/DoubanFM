/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * Reference : http://equinox1993.blog.163.com/blog/static/32205137201031141228418/
 * */

using System;
using System.Collections.Generic;

namespace DoubanFM
{
    /// <summary>
    /// 管理歌词在桌面上的显示
    /// </summary>
    class LyricsShower
    {
        /// <summary>
        /// 歌词本身
        /// </summary>
        protected Core.Lyrics Lyrics { get; private set; }

        /// <summary>
        /// 时间、歌词字典
        /// </summary>
        public Dictionary<TimeSpan, string> TimeAndLyrics { get; set; }

        /// <summary>
        /// 排过序的时间列表
        /// </summary>
        public List<TimeSpan> SortedTimes { get; private set; }

        /// <summary>
        /// 返回当前的歌词,使用前请先调用Refresh()函数
        /// </summary>
        public string CurrentLyrics { get; private set; }
        /// <summary>
        /// 返回下一个歌词,使用前请先调用Refresh()函数
        /// </summary>
        public string NextLyrics { get; private set; }
        /// <summary>
        /// 返回上一个歌词,使用前请先调用Refresh()函数
        /// </summary>
        public string PreviousLyrics { get; private set; }
        /// <summary>
        /// 返回当前歌词的Index,使用前请先调用Refresh()函数
        /// </summary>
        public int CurrentIndex { get; private set; }

        private TimeSpan currentTime;

        /// <summary>
        /// 表示歌词显示的当前时刻（不同的时刻意味着不同的显示内容）
        /// </summary>
        public TimeSpan CurrentTime
        {
            get { return currentTime; }
            set
            {
                if (currentTime != value)
                {
                    currentTime = value;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LyricsShower"/> class.
        /// </summary>
        /// <param name="lyrics">歌词</param>
        public LyricsShower(Core.Lyrics lyrics)
        {
            Lyrics = lyrics;

            TimeAndLyrics = new Dictionary<TimeSpan, string>();
            foreach (var pair in Lyrics.TimeAndLyrics)
            {
                TimeAndLyrics.Add(pair.Key - Lyrics.Offset, pair.Value);
            }

            SortedTimes = new List<TimeSpan>(TimeAndLyrics.Keys);
            SortedTimes.Sort();

            //当相邻的空歌词与非空歌词相差时间太大时，插入一行空歌词，
            //以避免双行显示歌词时出现长时间第一行为空、第二行有字的情况。
            Reset();
            TimeSpan distance = TimeSpan.FromSeconds(2);
            var addedTimes = new List<TimeSpan>();
            for (int i = -1; i + 1 < SortedTimes.Count; ++i)
            {
                if (i == -1 && SortedTimes[0] == TimeSpan.Zero) continue;
                var time1 = i == -1 ? TimeSpan.Zero : SortedTimes[i];
                var time2 = SortedTimes[i + 1];

                CurrentTime = time1;
                if (time2 - time1 > distance)
                {
                    if (string.IsNullOrWhiteSpace(CurrentLyrics) && !string.IsNullOrWhiteSpace(NextLyrics))
                    {
                        addedTimes.Add(time2 - distance);
                    }
                }
            }
            foreach (var time in addedTimes)
            {
                TimeAndLyrics.Add(time, string.Empty);
            }
            SortedTimes = new List<TimeSpan>(TimeAndLyrics.Keys);
            SortedTimes.Sort();

            Reset();
        }

        /// <summary>
        /// 根据当前时间刷新歌词状态
        /// </summary>
        protected void Refresh()
        {
            if (SortedTimes.Count == 0)
            {
                CurrentIndex = -1;
                PreviousLyrics = null;
                CurrentLyrics = null;
                NextLyrics = null;
            }
            else
            {
                while (true)
                {
                    if (CurrentIndex >= 0 && CurrentIndex < SortedTimes.Count && SortedTimes[CurrentIndex] > CurrentTime)
                        --CurrentIndex;
                    else if (CurrentIndex + 1 < SortedTimes.Count && SortedTimes[CurrentIndex + 1] <= CurrentTime)
                        ++CurrentIndex;
                    else break;
                }
                if (CurrentIndex - 1 >= 0 && CurrentIndex - 1 < SortedTimes.Count)
                    PreviousLyrics = TimeAndLyrics[SortedTimes[CurrentIndex - 1]];
                else PreviousLyrics = null;
                if (CurrentIndex >= 0 && CurrentIndex < SortedTimes.Count)
                    CurrentLyrics = TimeAndLyrics[SortedTimes[CurrentIndex]];
                else CurrentLyrics = null;
                if (CurrentIndex + 1 >= 0 && CurrentIndex + 1 < SortedTimes.Count)
                    NextLyrics = TimeAndLyrics[SortedTimes[CurrentIndex + 1]];
                else NextLyrics = null;
            }
        }

        /// <summary>
        /// 重置歌词状态
        /// </summary>
        public void Reset()
        {
            CurrentIndex = -1;
            CurrentTime = TimeSpan.MinValue;
        }
    }
}
