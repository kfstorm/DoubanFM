using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization.Json;

namespace DoubanFM.Core
{
    /// <summary>
    /// HTML分析类
    /// </summary>
    internal class HtmlAnalysis
    {
        /// <summary>
        /// HTML文件
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlAnalysis"/> class.
        /// </summary>
        /// <param name="file">The file.</param>
        internal HtmlAnalysis(string file)
        {
            this.File = file == null ? "" : file;
        }
        /// <summary>
        /// 从文件中获得频道列表的JSON表示
        /// </summary>
        /// <returns>JSON</returns>
        internal ChannelInfo GetChannelInfo()
        {
            ChannelInfo ci = null;
            try
            {
                Match match = Regex.Match(File, @"var\s*channelInfo\s*=\s*(.*),", RegexOptions.IgnoreCase);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ChannelInfo));
                using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(match.Groups[1].Value)))
                    ci = (ChannelInfo)ser.ReadObject(ms);
                ci.Dj = GetDjCates();
                ci.Personal = new Cate[1];
                ci.Personal[0] = new Cate();
                ci.Personal[0].cate = "私人兆赫";
                ci.Personal[0].Channels = new Channel[1];
                ci.Personal[0].Channels[0] = new Channel();
                ci.Personal[0].Channels[0].Id = "0";
                ci.Personal[0].Channels[0].Name = "私人兆赫";
            }
            catch
            {
                return null;
            }
            return ci;
        }
        /// <summary>
        /// 获取DJ频道
        /// </summary>
        /// <returns>DJ频道</returns>
        private Cate[] GetDjCates()
        {
            List<Cate> ret = new List<Cate>();
            try
            {
                Match match = Regex.Match(File, @"channelInfo\.dj\s*=\s*(.*);", RegexOptions.IgnoreCase);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ChannelInfoDotDj));
                ChannelInfoDotDj cidd = null;
                using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(match.Groups[1].Value)))
                    cidd = (ChannelInfoDotDj)ser.ReadObject(ms);
                Match match2 = Regex.Match(File, @"subChannelInfo\s*=\s*{(.*)};", RegexOptions.IgnoreCase);
                string s = match2.Groups[1].Value;
                MatchCollection mc = Regex.Matches(s, "\"(\\w*)\":\\[([^.\\[]*)\\]", RegexOptions.None);
                List<SubDjCate> subdjcates = new List<SubDjCate>();
                foreach (Match ma in mc)
                {
                    var subdjcate = new SubDjCate();
                    subdjcate.Cate = ma.Groups[1].Value;
                    DataContractJsonSerializer ser2 = new DataContractJsonSerializer(typeof(SubDjChannel[]));
                    using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes("[" + ma.Groups[2].Value + "]")))
                        subdjcate.DjChannels = (SubDjChannel[])ser2.ReadObject(ms);
                    subdjcates.Add(subdjcate);
                }
                foreach (DjChannel djchannel in cidd)
                {
                    foreach (SubDjCate subdjcate in subdjcates)
                        if (djchannel.channel == subdjcate.Cate)
                        {
                            var djcate = new Cate();
                            djcate.cate = djchannel.name;
                            List<Channel> channels = new List<Channel>();
                            foreach (SubDjChannel subdjchannel in subdjcate.DjChannels)
                            {
                                var channel = new Channel();
                                channel.Id = "dj";
                                channel.Name = subdjchannel.name;
                                channel.pid = subdjchannel.pid;
                                channels.Add(channel);
                            }
                            djcate.Channels = channels.ToArray();
                            ret.Add(djcate);
                        }
                }
                return ret.ToArray();
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 判断用户是否成功登录
        /// </summary>
        /// <returns>成功与否</returns>
        internal bool IsLoggedOn()
        {
            Match match = Regex.Match(File, @"var\s*globalConfig\s*=\s*{\s*uid\s*:\s*'(\d*)'", RegexOptions.IgnoreCase);
            string s = match.Groups[1].Value;
            return !string.IsNullOrEmpty(s);
            //return File.IndexOf("收听记录") != -1;
        }
        /// <summary>
        /// 获取验证码的ID
        /// </summary>
        /// <returns>ID</returns>
        internal string GetCaptchaID()
        {
            Match match = Regex.Match(File, "<img src=\"http://www\\.douban\\.com/misc/captcha\\?id=(\\w*)", RegexOptions.IgnoreCase);
            return match.Groups[1].Value;
        }
        /// <summary>
        /// 获取注销的链接
        /// </summary>
        /// <returns>链接</returns>
        internal string GetLogOffLink()
        {
            Match match = Regex.Match(File, "\"(http://www\\.douban\\.com/accounts/logout\\?source=radio&[^\\s]*)\"", RegexOptions.IgnoreCase);
            return match.Groups[1].Value;
        }
    }
}
