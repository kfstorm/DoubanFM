using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace DoubanFM.Core
{
    /// <summary>
    /// 网络连接基础类
    /// </summary>
    internal class ConnectionBase
    {
        /// <summary>
        /// Cookie
        /// </summary>
        public static CookieContainer cc;
        /// <summary>
        /// 默认HTTP头：UserAgent
        /// </summary>
        public static string DefaultUserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
        /// <summary>
        /// 默认HTTP头：Accept
        /// </summary>
        public static string DefaultAccept = "text/html, application/xhtml+xml, */*";
        /// <summary>
        /// 默认HTTP头：ContentType
        /// </summary>
        public static string DefaultContentType = "application/x-www-form-urlencoded";
        /// <summary>
        /// 你懂的……
        /// </summary>
        string UserAgent, Accept, ContentType;

        static ConnectionBase()
        {
            if (!LoadCookies())
                cc = new CookieContainer(10000, 10000, 100000);
        }

        internal ConnectionBase(string UserAgent, string Accept, string ContentType)
        {
            this.UserAgent = UserAgent;
            this.Accept = Accept;
            this.ContentType = ContentType;
        }

        internal ConnectionBase()
            : this(DefaultUserAgent, DefaultAccept, DefaultContentType)
        {
        }
        /// <summary>
        /// 用Post方法发送请求
        /// </summary>
        /// <param name="PostUri">请求的地址</param>
        /// <param name="Accept">Accept头</param>
        /// <param name="Referer">Referer头</param>
        /// <param name="ContentType">ContentType头</param>
        /// <param name="Content">请求正文</param>
        /// <param name="encoding">解读响应正文的字符编码</param>
        /// <returns>响应正文</returns>
        virtual internal string Post(string PostUri, string Accept, string Referer, string ContentType, byte[] Content)
        {
            string file = string.Empty;

            try
            {
                HttpWebRequest request = WebRequest.Create(PostUri) as HttpWebRequest;
                request.Accept = Accept;
                request.AllowAutoRedirect = true;
                request.ContentLength = Content.Length;
                request.ContentType = ContentType;
                request.CookieContainer = cc;
                request.KeepAlive = true;
                request.Method = "POST";
                request.Referer = Referer;
                request.UserAgent = UserAgent;
                using (Stream requestStream = request.GetRequestStream())
                    requestStream.Write(Content, 0, Content.Length);
                using (HttpWebResponse responce = request.GetResponse() as HttpWebResponse)
                using (StreamReader sr = new StreamReader(responce.GetResponseStream()))
                    file = sr.ReadToEnd();
            }
            catch (WebException)
            {
            }

            return file;
        }
        /// <summary>
        /// 用Post方法发送请求
        /// </summary>
        /// <param name="PostUri">请求的地址</param>
        /// <param name="Content">请求正文</param>
        /// <returns>响应正文</returns>
        internal string Post(string PostUri, byte[] Content)
        {
            return Post(PostUri, null, Content);
        }
        /// <summary>
        /// 用Post方法发送请求
        /// </summary>
        /// <param name="PostUri">请求的地址</param>
        /// <param name="Referer">Referer头</param>
        /// <param name="Content">请求正文</param>
        /// <returns>响应正文</returns>
        internal string Post(string PostUri, string Referer, byte[] Content)
        {
            return Post(PostUri, Accept, Referer, ContentType, Content);
        }
        /// <summary>
        /// 用Get方法发送请求
        /// </summary>
        /// <param name="GetUri">请求的地址</param>
        /// <param name="Accept">Accept头</param>
        /// <param name="Referer">Referer头</param>
        /// <param name="encoding">解读响应正文的字符编码</param>
        /// <returns>响应正文</returns>
        virtual internal string Get(string GetUri, string Accept, string Referer)
        {
            string file = string.Empty;
            try
            {
                HttpWebRequest request = WebRequest.Create(GetUri) as HttpWebRequest;
                request.Accept = Accept;
                request.AllowAutoRedirect = true;
                request.CookieContainer = cc;
                request.KeepAlive = true;
                request.Method = "GET";
                request.Referer = Referer;
                request.UserAgent = UserAgent;
                using (HttpWebResponse responce = request.GetResponse() as HttpWebResponse)
                using (StreamReader sr = new StreamReader(responce.GetResponseStream()))
                    file = sr.ReadToEnd();
            }
            catch (WebException)
            {
            }
            return file;
        }
        /// <summary>
        /// 用Get方法发送请求
        /// </summary>
        /// <param name="GetUri">请求的地址</param>
        /// <returns>响应正文</returns>
        internal string Get(string GetUri)
        {
            return Get(GetUri, Accept, null);
        }

        /// <summary>
        /// 用Get方法发送请求
        /// </summary>
        /// <param name="GetUri">请求的地址</param>
        /// <param name="Referer">Referer头</param>
        /// <returns>响应正文</returns>
        internal string Get(string GetUri, string Referer)
        {
            return Get(GetUri, Accept, Referer);
        }
        /// <summary>
        /// 读取Cookies
        /// </summary>
        /// <returns>成功与否</returns>
        public static bool LoadCookies()
        {
            try
            {
                using (FileStream stream = File.OpenRead(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\K.F.Storm\豆瓣电台\cookies.dat"))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    cc = (CookieContainer)formatter.Deserialize(stream);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 保存Cookies
        /// </summary>
        /// <returns>成功与否</returns>
        public static bool SaveCookies()
        {
            try
            {
                string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\K.F.Storm\豆瓣电台";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using (FileStream stream = File.OpenWrite(dir + @"\cookies.dat"))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, cc);
                }
            }

            catch
            {
                return false;
            }

            return true;
        }
    }
}
