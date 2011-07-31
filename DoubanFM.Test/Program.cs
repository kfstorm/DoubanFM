using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DoubanFM.Core;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace DoubanFM.Test
{
    /// <summary>
    /// 测试用
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //WritePlayList1();
            //Test2();
            //TestChannelInfo();
            //TestChannelInfo2();
            //DownloadWebpageWithFileStream();
            //TestHSLColor();
            //GenarateHSLColor(8);

            var list = args.ToList();
            int index = list.IndexOf("-GeneratePlayList");
            if (index != -1 && index < list.Count - 1)
                GeneratePlayList(list[index + 1]);
        }
        /// <summary>
        /// 指定播放列表，并序列化
        /// </summary>
        static void WritePlayList1()
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(PlayList));
            //FileStream output = File.OpenWrite(new FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName).DirectoryName + "/LocalMusic_PlayList.dat");
            using (FileStream output = File.OpenWrite("F:/LocalMusic_PlayList.dat"))
            {

                PlayList pl = new PlayList();
                Song song;

                song = new Song();
                song.albumtitle = "For Your Entertainment";
                song.artist = "Adam Lambert";
                song.picture = "F:\\我的音乐\\iTunes\\iTunes Media\\Music\\Mariah Carey\\E=MC2\\Folder.jpg";
                song.title = "Music Again";
                song.url = @"F:\我的音乐\iTunes\iTunes Media\Music\Adam Lambert\For Your Entertainment\01 Music Again.m4a";
                pl.Songs.Add(song);

                ser.WriteObject(output, pl);
            }

        }
        /// <summary>
        /// 从网络获取播放列表，并序列化
        /// </summary>
        static void Test2()
        {
            //DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(PlayList));
            //PlayList pl = PlayList.GetNewPlayList("n");
            //FileStream output = File.OpenWrite("F:/testout.txt");
            //ser.WriteObject(output, pl);
            //output.Close();
        }
        /// <summary>
        /// 测试ChannelInfo类
        /// </summary>
        static void TestChannelInfo()
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ChannelInfo));

            using (FileStream input = File.OpenRead("F:/TestChannelInfo.txt"))
            {
                StreamReader sr = new StreamReader(input);
                using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(sr.ReadToEnd())))
                {
                    //ms.Position = 0;
                    ChannelInfo ci = (ChannelInfo)ser.ReadObject(ms);
                    using (FileStream output = File.OpenWrite("F:/TestChannelInfoOutput.txt"))
                        ser.WriteObject(output, ci);
                }
            }

            //ChannelInfo ci = new ChannelInfo();
            //ci.personal = new Cates[0];
            //ci.publ1c = new Cates[0];
            //MemoryStream ms = new MemoryStream();
            //ser.WriteObject(ms, ci);
            //ms.Position = 0;
            //StreamReader sr = new StreamReader(ms);
            //string s = sr.ReadToEnd();
        }
        /// <summary>
        /// 从网络获取频道列表，并序列化
        /// </summary>
        static void TestChannelInfo2()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://douban.fm");
            using (HttpWebResponse responce =(HttpWebResponse) request.GetResponse())
            {
                using (Stream stream = responce.GetResponseStream())
                {
                    string data = new StreamReader(stream).ReadToEnd();
                    //Match match = Regex.Match(data, @"var channelInfo = {[^}]*}[^{}]*(((?'Open'{[^}]*})[^{}]*)+((?'-Open'{/})[^{}]*)+)*(?(Open)(?!)){/}");
                    Match match = Regex.Match(data, @"var channelInfo = {(?'Open'.*)}");
                    string b = "{" + match.Groups["Open"].Value + "}";

                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ChannelInfo));

                    using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(b)))
                    {
                        ChannelInfo ci = (ChannelInfo)ser.ReadObject(ms);
                        using (FileStream output = File.OpenWrite("F:/TestChannelInfoOutput.txt"))
                            ser.WriteObject(output, ci);
                    }
                }
            }
        }
        /// <summary>
        /// 测试用FileStream下载网页，已失败。
        /// </summary>
        static void DownloadWebpageWithFileStream()
        {
            using (FileStream fs = File.OpenRead("http://douban.fm"))
                Console.WriteLine(new StreamReader(fs).ReadToEnd());
        }
        /// <summary>
        /// 从iTunes导出的播放列表文件获取播放列表
        /// </summary>
        /// <param name="filepath">文件</param>
        /// <returns>播放列表</returns>
        static PlayList GetPlayListFromTxtFile(string filepath)
        {
            using (FileStream fs = File.OpenRead(filepath))
            {
                StreamReader sr = new StreamReader(fs);
                PlayList pl = new PlayList();
                sr.ReadLine(); //跳过首行
                while (!sr.EndOfStream)
                {
                    Song song = new Song();
                    string s = sr.ReadLine();
                    int pos1 = 0, pos2 = 0;
                    pos2 = s.IndexOf('\t', pos1);
                    //名称
                    song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //表演者
                    song.artist = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //作曲者
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //专辑
                    song.albumtitle = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //归类
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //风格
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //大小
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //时间
                    song.length = int.Parse(s.Substring(pos1, pos2 - pos1));

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //光盘号码
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //光盘统计
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //轨道号码
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //轨道统计
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //年份
                    song.public_time = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //修改日期
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //添加日期
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //比特率
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //采样速率
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //音量调整
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //种类
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //均衡器
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //注释
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //播放次数
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //最后播放
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //跳过次数
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //最后被跳过的
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.IndexOf('\t', pos1);
                    //我的评价
                    //song.title = s.Substring(pos1, pos2 - pos1);

                    pos1 = pos2 + 1;
                    pos2 = s.Length;
                    //位置
                    song.url = s.Substring(pos1, pos2 - pos1);


                    song.picture = song.url.Substring(0, song.url.LastIndexOf('\\') + 1) + "Folder.jpg";

                    pl.Songs.Add(song);
                }
                return pl;
            }
        }
        /// <summary>
        /// 将播放列表序列化
        /// </summary>
        /// <param name="filepath">文件路径</param>
        /// <param name="pl">播放列表</param>
        static void WritePlayList(string filepath, PlayList pl)
        {
            if (File.Exists(filepath))
                File.Delete(filepath);
            using (FileStream fs = File.OpenWrite(filepath))
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(PlayList));
                ser.WriteObject(fs, pl);
            }
        }
        /// <summary>
        /// 从iTunes导出的播放列表生成本地播放列表
        /// </summary>
        /// <param name="filepath"></param>
        static void GeneratePlayList(string filepath)
        {
            try
            {
                WritePlayList("LocalMusic_PlayList.dat", GetPlayListFromTxtFile(filepath));
            }
            catch { }
        }
        /// <summary>
        /// 测试HSLColor类
        /// </summary>
        static void TestHSLColor()
        {
            Random ran = new Random();
            while (true)
            {
                byte a = (byte)ran.Next(256);
                byte r = (byte)ran.Next(256);
                byte g = (byte)ran.Next(256);
                byte b = (byte)ran.Next(256);
                a = 255; r = 0xE4; g = 0xDC; b = 0xDB;
                Color RGB = Color.FromArgb(a, r, g, b);
                HSLColor HSL = new HSLColor(RGB);
                Color RGB2 = HSL.ToRGB();
                Console.WriteLine("RGB:" + "\t" + RGB.A + "\t" + RGB.R + "\t" + RGB.G + "\t" + RGB.B);
                if (RGB != RGB2)
                {
                    Console.WriteLine("RGB2:" + "\t" + RGB2.A + "\t" + RGB2.R + "\t" + RGB2.G + "\t" + RGB2.B);
                    Console.WriteLine("Difference Found!!!!");
                }
            }
        }
        /// <summary>
        /// 为Metro UI生成颜色
        /// </summary>
        /// <param name="count">颜色数</param>
        static void GenarateHSLColor(int count)
        {
            HSLColor color = new HSLColor(0, 0.8, 0.55);
            for (int i = 0; i < count; ++i)
            {
                Console.WriteLine("<SolidColorBrush Color=\"" + color + "\"/>");
                color.Hue += 360.0 / count;
            }
            Console.ReadLine();
        }
    }
}
