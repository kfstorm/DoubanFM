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
    /// 频道类
    /// </summary>
    [DataContract]
    [Serializable]
    public class Channel
    {
        /// <summary>
        /// 频道ID
        /// </summary>
        [DataMember(Name = "channel_id")]
        public string Id { get; set; }
        /// <summary>
        /// 频道名称
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }
        /// <summary>
        /// DJ频道特有的属性，节目ID（Program ID?）
        /// </summary>
        [DataMember]
        public string pid { get; set; }
    }
    /// <summary>
    /// 门类
    /// </summary>
    [DataContract]
    public class Cate
    {
        /// <summary>
        /// 该门类所含频道
        /// </summary>
        [DataMember(Name = "channels")]
        virtual public Channel[] Channels { get; set; }
        /// <summary>
        /// 门类名
        /// </summary>
        [DataMember]
        public string cate { get; set; }
    }
    /// <summary>
    /// DJ频道类，仅用于序列化，实际使用时请使用Cate类
    /// </summary>
    [DataContract]
    internal class DjChannel
    {
        /// <summary>
        /// DJ频道的ID，通常为一串6位数字
        /// </summary>
        /// <value>
        /// 频道ID
        /// </value>
        [DataMember(Name="channel_id")]
        public string channel { get; set; }
        /// <summary>
        /// 频道名称
        /// </summary>
        /// <value>
        /// 名称
        /// </value>
        [DataMember]
        public string name { get; set; }
        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        [DataMember]
        public string timestamp { get; set; }
        /// <summary>
        /// （可能是）更新的节目数量
        /// </summary>
        /// <value>
        /// 数量
        /// </value>
        [DataMember]
        public int update { get; set; }
    }
    /// <summary>
    /// DJ节目类，仅用于序列化，实际使用时请使用Channel类
    /// </summary>
    [DataContract]
    internal class SubDjChannel
    {
        /// <summary>
        /// 通常为5位数字，实为某DJ频道的某节目ID
        /// </summary>
        /// <value>
        /// 节目ID
        /// </value>
        [DataMember(Name = "channel_id")]
        public string pid { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string name { get; set; }
    }
    /// <summary>
    /// DJ频道类，仅用于序列化，实际使用时请使用Cate类
    /// </summary>
    internal class SubDjCate
    {
        /// <summary>
        /// 频道编号
        /// </summary>
        /// <value>
        /// 编号
        /// </value>
        public string Cate { get; set; }
        /// <summary>
        /// 节目们
        /// </summary>
        /// <value>
        /// 节目们
        /// </value>
        public SubDjChannel[] DjChannels { get; set; }
    }
    internal class ChannelInfoDotDj : List<DjChannel> { }
    /// <summary>
    /// 频道列表
    /// </summary>
    [DataContract]
    public class ChannelInfo
    {
        /// <summary>
        /// 私人频道
        /// </summary>
        [DataMember(Name = "personal")]
        public Cate[] Personal { get; set; }
        /// <summary>
        /// 公共频道
        /// </summary>
        [DataMember(Name = "public")]
        public Cate[] Public { get; set; }
        /// <summary>
        /// DJ频道
        /// </summary>
        [DataMember]
        public Cate[] Dj { get; set; }
        /// <summary>
        /// 获取频道列表
        /// </summary>
        /// <returns>频道列表</returns>
        public static ChannelInfo GetChannelInfo()
        {
#if DEBUG
            if (System.Environment.GetCommandLineArgs().Contains("-LocalMusic"))
            {
                try
                {
                    string JsonString = null;
                    using (FileStream fs = File.OpenRead(new FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName).DirectoryName + "/LocalMusic_ChannelInfo.dat"))
                    {
                        StreamReader sr = new StreamReader(fs);
                        JsonString = sr.ReadToEnd();
                    }
                    using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(JsonString)))
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ChannelInfo));
                        return (ChannelInfo)ser.ReadObject(ms);
                    }
                }
                catch { }
            }
#endif
            ChannelInfo ret = null;
            while (true)
            {
                try
                {
                    string url = "http://douban.fm";
                    ret = GetChannelInfo(new ConnectionBase().Get(url));
                }
                catch { }
                if (ret == null || ret.Personal == null && ret.Public == null)
                    Thread.Sleep(1000);
                else break;
            }

            return ret;
        }
        /// <summary>
        /// 从HTML文件里获取频道列表
        /// </summary>
        /// <param name="file">HTML文件</param>
        /// <returns>频道列表</returns>
        public static ChannelInfo GetChannelInfo(string file)
        {
            return new HtmlAnalysis(file).GetChannelInfo();
        }
    }
}