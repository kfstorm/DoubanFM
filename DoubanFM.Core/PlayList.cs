using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;

namespace DoubanFM.Core
{
    /// <summary>
    /// 音乐
    /// </summary>
    [DataContract]
    public class Song
    {
        /// <summary>
        /// 专辑的豆瓣资料页面
        /// </summary>
        [DataMember]
        public string album { get; set; }
        /// <summary>
        /// 封面URL
        /// </summary>
        [DataMember]
        public string picture { get; set; }
        [DataMember]
        public string ssid { get; set; }
        /// <summary>
        /// 表演者
        /// </summary>
        [DataMember]
        public string artist { get; set; }
        /// <summary>
        /// 音乐文件URL
        /// </summary>
        [DataMember]
        public string url { get; set; }
        /// <summary>
        /// 唱片公司
        /// </summary>
        [DataMember]
        public string company { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        [DataMember]
        public string title { get; set; }
        /// <summary>
        /// 平均评分
        /// </summary>
        [DataMember]
        public double rating_avg { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        [DataMember]
        public int length { get; set; }
        [DataMember]
        public string source_url { get; set; }
        /// <summary>
        /// 普通音乐应该是""，广告应该是"T"
        /// </summary>
        [DataMember]
        public string subtype { get; set; }
        /// <summary>
        /// 发行年
        /// </summary>
        [DataMember]
        public string public_time { get; set; }
        /// <summary>
        /// 歌曲ID
        /// </summary>
        [DataMember]
        public string sid { get; set; }
        /// <summary>
        /// 专辑ID
        /// </summary>
        [DataMember]
        public string aid { get; set; }
        /// <summary>
        /// 专辑
        /// </summary>
        [DataMember]
        public string albumtitle { get; set; }
        /// <summary>
        /// 当前用户喜欢与否
        /// </summary>
        [DataMember]
        public bool like { get; set; }
    }
    /// <summary>
    /// 播放列表
    /// </summary>
    [DataContract]
    public class PlayList
    {
        [DataMember]
        public int r { get; set; }
        /// <summary>
        /// 播放列表里的歌曲
        /// </summary>
        [DataMember(Name="song")]
        public List<Song> Songs { get; set; }

        public PlayList()
        {
            Songs = new List<Song>();
        }
        /// <summary>
        /// 获取播放列表
        /// </summary>
        /// <param name="sid">上次播放的音乐的sid</param>
        /// <param name="channel">频道</param>
        /// <param name="type">请求类型</param>
        /// <param name="h">播放历史</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// 播放列表
        /// </returns>
        public static PlayList GetPlayList(string sid, Channel channel, string type, string h, string context = null)
        {
#if DEBUG
            if (System.Environment.GetCommandLineArgs().Contains("-LocalMusic"))
            {
                try
                {
                    using (FileStream fs = File.OpenRead(new FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName).DirectoryName + "/LocalMusic_PlayList.dat"))
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(PlayList));
                        return (PlayList)ser.ReadObject(fs);
                    }
                }
                catch
                { }
            }
#endif
            PlayList ret = null;
            while (true)
            {
                try
                {
                    string url = "http://douban.fm/j/mine/playlist?sid=" + sid + "&channel=" + channel.Id + "&type=" + type + "&h=" + h;
                    if (context != null && context.Length > 0)
                        url = url + "&context=" + context;
                    if (channel.Id == "dj")
                        url += "&pid=" + channel.pid;
                    string PlayListString = new ConnectionBase().Get(url, @"application/json, text/javascript, */*; q=0.01", @"http://douban.fm");
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(PlayList));
                    using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(PlayListString)))
                        ret = (PlayList)ser.ReadObject(stream);
                }
                catch { }
                if (ret == null || ret.Songs == null)
                    Thread.Sleep(1000);
                else break;
            }
            foreach (Song song in ret.Songs)
                song.picture = song.picture.Replace("/mpic/", "/lpic/").Replace("//otho.", "//img3.");
            return ret;
        }
    }
}
