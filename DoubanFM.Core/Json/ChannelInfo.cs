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
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace DoubanFM.Core.Json
{
	/// <summary>
	/// JSON格式的频道列表
	/// </summary>
	[DataContract]
	class ChannelInfo
	{
		/// <summary>
		/// 私人频道
		/// </summary>
		[DataMember]
		public Cate[] personal { get; set; }
		/// <summary>
		/// 公共频道
		/// </summary>
		[DataMember(Name = "public")]
		public Cate[] pppublic { get; set; }
		/// <summary>
		/// DJ频道
		/// </summary>
		[DataMember]
		public Cate[] Dj { get; set; }

		/// <summary>
		/// 从JSON生成
		/// </summary>
		/// <param name="json">JSON字符串</param>
		/// <returns></returns>
		private static ChannelInfo FromJson(string json)
		{
			try
			{
				DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ChannelInfo));
				using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
					return (ChannelInfo)ser.ReadObject(stream);
			}
			catch
			{
				return null;
			}
		}
		/// <summary>
		/// 从HTML获取DJ频道
		/// </summary>
		/// <param name="html">HTML字符串</param>
		/// <returns></returns>
		private static Cate[] GetDjCates(string html)
		{
			List<Cate> ret = new List<Cate>();
			try
			{
				Match match = Regex.Match(html, @"channelInfo\.dj\s*=\s*(\[.*\]);", RegexOptions.IgnoreCase);
				DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ChannelInfoDotDj));
				ChannelInfoDotDj cidd = null;
				using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(match.Groups[1].Value)))
					cidd = (ChannelInfoDotDj)ser.ReadObject(ms);
				Match match2 = Regex.Match(html, @"subChannelInfo\s*=\s*({.*});", RegexOptions.IgnoreCase);
				List<SubDjCate> subdjcates = new List<SubDjCate>();
				using (var reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.Unicode.GetBytes(match2.Groups[1].Value), System.Xml.XmlDictionaryReaderQuotas.Max))
				{
					XmlDocument document = new XmlDocument();
					document.Load(reader);
					foreach (XmlNode elementCate in document.DocumentElement.ChildNodes)
					{
						SubDjCate cate = new SubDjCate();
						cate.Cate = elementCate.Attributes["item"].Value;
						List<SubDjChannel> channels = new List<SubDjChannel>();
						foreach (XmlNode elementChannel in elementCate.ChildNodes)
						{
							SubDjChannel channel = new SubDjChannel();
							foreach (XmlNode elementProperty in elementChannel.ChildNodes)
							{
								if (elementProperty.Name == "channel_id")
									channel.pid = elementProperty.FirstChild.Value;
								else if (elementProperty.Name == "name")
									channel.name = elementProperty.FirstChild.Value;
							}
							channels.Add(channel);
						}
						cate.DjChannels = channels.ToArray();
						subdjcates.Add(cate);
					}
				}

				foreach (DjChannel djchannel in cidd)
				{
					foreach (SubDjCate subdjcate in subdjcates)
						if (djchannel.channel_id == subdjcate.Cate)
						{
							var djcate = new Cate();
							djcate.cate = djchannel.name;
							List<Channel> channels = new List<Channel>();
							foreach (SubDjChannel subdjchannel in subdjcate.DjChannels)
							{
								var channel = new Channel();
								channel.channel_id = "dj";
								channel.name = subdjchannel.name;
								channel.pid = subdjchannel.pid;
								channels.Add(channel);
							}
							djcate.channels = channels.ToArray();
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
		/// 从HTML生成
		/// </summary>
		/// <param name="html">HTML字符串</param>
		/// <returns></returns>
		public static ChannelInfo FromHtml(string html)
		{
			try
			{
				html = html.Replace("&amp;", "&");
				Match match = Regex.Match(html, @"var\s*channelInfo\s*=\s*({.*}),", RegexOptions.IgnoreCase);
				ChannelInfo ci = ChannelInfo.FromJson(match.Groups[1].Value);
				ci.Dj = GetDjCates(html);

				ci.personal = new Cate[1];
				ci.personal[0] = new Cate();
				ci.personal[0].cate = "私人兆赫";
				ci.personal[0].channels = new Channel[2];

				ci.personal[0].channels[0] = new Channel();
				ci.personal[0].channels[0].channel_id = "0";
				ci.personal[0].channels[0].name = "私人兆赫";
				ci.personal[0].channels[1] = new Channel();
				ci.personal[0].channels[1].channel_id = "-3";
				ci.personal[0].channels[1].name = "红心兆赫";
				return ci;
			}
			catch
			{
				return null;
			}
		}
	}
}
