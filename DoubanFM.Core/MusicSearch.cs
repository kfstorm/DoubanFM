using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DoubanFM.Core
{
    /// <summary>
    /// 搜索音乐的类
    /// </summary>
    public class MusicSearch
    {
        /// <summary>
        /// 搜索是否完成
        /// </summary>
        public bool IsSearchFinished { get; private set; }
        /// <summary>
        /// 获取搜索文本
        /// </summary>
        public string SearchText { get; private set; }
        /// <summary>
        /// 获取搜索结果
        /// </summary>
        public SearchItem[] SearchResult { get; private set; }
        /// <summary>
        /// 获取上一页是否可用
        /// </summary>
        public bool IsPreviousPageEnabled { get { return previousPageLink != null && previousPageLink.Length > 0; } }
        /// <summary>
        /// 获取下一页是否可用
        /// </summary>
        public bool IsNextPageEnabled { get { return nextPageLink != null && nextPageLink.Length > 0; } }
        /// <summary>
        /// 搜索第Start/15 + 1页
        /// </summary>
        public int Start { get; private set; }
        /// <summary>
        /// 上一页和下一页的链接
        /// </summary>
        private string previousPageLink, nextPageLink;
        /// <summary>
        /// 搜索线程
        /// </summary>
        private Thread searchThread;

        /// <summary>
        /// Occurs when [search finished].
        /// </summary>
        public event EventHandler SearchFinished;

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicSearch"/> class.
        /// </summary>
        public MusicSearch()
        {
            IsSearchFinished = true;
        }

        /// <summary>
        /// 开始搜索
        /// </summary>
        /// <param name="text">搜索文本</param>
        /// <param name="start">Start值</param>
        public void Search(string text, int start = 0)
        {
            if (IsSearchFinished == false) return;
            IsSearchFinished = false;
            Start = start;
            SearchText = text;
            searchThread = new Thread(new ThreadStart(() =>
                {
                    string file = new ConnectionBase().Get("http://music.douban.com/subject_search?start=" + Start + "&search_text=" + SearchText);
                    HtmlAnalysis ha = new HtmlAnalysis(file);
                    SearchResult = ha.GetSearchItems();
                    IsSearchFinished = true;
                    previousPageLink = ha.GetPreviousPageLink();
                    nextPageLink = ha.GetNextPageLink();
                    RaiseSearchFinishedEvent(new EventArgs());
                }));
            searchThread.IsBackground = true;
            searchThread.Start();
        }

        /// <summary>
        /// 上一页
        /// </summary>
        public void PreviousPage()
        {
            if (!IsPreviousPageEnabled) return;
            Search(SearchText, Start - 15);
        }

        /// <summary>
        /// 下一页
        /// </summary>
        public void NextPage()
        {
            if (!IsNextPageEnabled) return;
            Search(SearchText, Start + 15);
        }

        /// <summary>
        /// 停止搜索
        /// </summary>
        public void Stop()
        {
            if (IsSearchFinished)
                return;
            try
            {
                searchThread.Abort();
                IsSearchFinished = true;
            }
            catch { }
        }

        /// <summary>
        /// Raises the search finished event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void RaiseSearchFinishedEvent(EventArgs e)
        {
            if (SearchFinished != null)
                SearchFinished(this, e);
        }

    }

    /// <summary>
    /// 搜索结果中的项
    /// </summary>
    public class SearchItem
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// Gets the picture.
        /// </summary>
        public string Picture { get; private set; }
        /// <summary>
        /// 其他信息
        /// </summary>
        public string[] Infomations { get; private set; }
        /// <summary>
        /// 是否是艺术家
        /// </summary>
        public bool IsArtist { get; private set; }
        /// <summary>
        /// 能否收听此项
        /// </summary>
        public bool CanContextPlay
        {
            get { return Context != null && Context.Length > 0; }
        }
        public string Context { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchItem"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="picture">The picture.</param>
        /// <param name="infomations">The infomations.</param>
        /// <param name="isArtist">if set to <c>true</c> [is artist].</param>
        /// <param name="context">The context.</param>
        internal SearchItem(string title, string picture, string[] infomations, bool isArtist, string context)
        {
            Title = title;
            Picture = picture;
            Infomations = infomations;
            IsArtist = isArtist;
            Context = context;
        }
    }
}
