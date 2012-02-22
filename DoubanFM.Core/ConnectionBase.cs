/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace DoubanFM.Core
{
	/// <summary>
	/// 网络连接基础类
	/// </summary>
	public class ConnectionBase
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
		/// 默认HTTP头：accept
		/// </summary>
		public static string DefaultAccept = "text/html, application/xhtml+xml, */*";
		/// <summary>
		/// 默认HTTP头：contentType
		/// </summary>
		public static string DefaultContentType = "application/x-www-form-urlencoded";
		/// <summary>
		/// 默认编码
		/// </summary>
		public static Encoding DefaultEncoding = Encoding.UTF8;
		/// <summary>
		/// 空Cookie
		/// </summary>
		public static CookieContainer DefaultCookie = new CookieContainer(1000, 1000, 100000);
		/// <summary>
		/// 你懂的……
		/// </summary>
		public string UserAgent, Accept, ContentType;
		/// <summary>
		/// 编码
		/// </summary>
		public Encoding Encoding;
		/// <summary>
		/// 是否抛出异常
		/// </summary>
		public bool ThrowException;
		/// <summary>
		/// 数据保存文件夹
		/// </summary>
		public static readonly string DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"K.F.Storm\豆瓣电台");

		static ConnectionBase()
		{
			if (!LoadCookies()) cc = DefaultCookie;
		}

		public ConnectionBase(bool throwException, string userAgent, string accept, string contentType, Encoding encoding)
		{
			ThrowException = throwException;
			UserAgent = userAgent;
			Accept = accept;
			ContentType = contentType;
			Encoding = encoding;
		}

		public ConnectionBase(bool throwException = false)
			: this(DefaultEncoding, throwException)
		{
		}

		public ConnectionBase(Encoding encoding, bool throwException = false)
			: this(throwException, DefaultUserAgent, DefaultAccept, DefaultContentType, encoding)
		{
		}
		/// <summary>
		/// 用Post方法发送请求
		/// </summary>
		/// <param name="postUri">请求的地址</param>
		/// <param name="accept">Accept头</param>
		/// <param name="referer">Referer头</param>
		/// <param name="contentType">ContentType头</param>
		/// <param name="content">请求正文</param>
		/// <returns>
		/// 响应正文
		/// </returns>
		public string Post(string postUri, string accept, string referer, string contentType, byte[] content)
		{
			string file = null;

			try
			{
				HttpWebRequest request = WebRequest.Create(postUri) as HttpWebRequest;
				request.Accept = accept;
				request.AllowAutoRedirect = true;
				request.ContentLength = content.Length;
				request.ContentType = contentType;
				request.CookieContainer = cc;
				request.KeepAlive = true;
				request.Method = "POST";
				request.Referer = referer;
				request.UserAgent = UserAgent;
				request.ServicePoint.Expect100Continue = false;
				using (Stream requestStream = request.GetRequestStream())
					requestStream.Write(content, 0, content.Length);
				using (HttpWebResponse responce = request.GetResponse() as HttpWebResponse)
				using (StreamReader sr = new StreamReader(responce.GetResponseStream(), Encoding))
					file = sr.ReadToEnd();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				if (ThrowException)
					throw;
			}

			return file;
		}
		/// <summary>
		/// 用Post方法发送请求
		/// </summary>
		/// <param name="postUri">请求的地址</param>
		/// <param name="content">请求正文</param>
		/// <returns>响应正文</returns>
		public string Post(string postUri, byte[] content)
		{
			return Post(postUri, null, content);
		}
		/// <summary>
		/// 用Post方法发送请求
		/// </summary>
		/// <param name="postUri">请求的地址</param>
		/// <param name="referer">Referer头</param>
		/// <param name="content">请求正文</param>
		/// <returns>响应正文</returns>
		public string Post(string postUri, string referer, byte[] content)
		{
			return Post(postUri, Accept, referer, ContentType, content);
		}
		/// <summary>
		/// 用Get方法发送请求
		/// </summary>
		/// <param name="getUri">请求的地址</param>
		/// <param name="accept">Accept头</param>
		/// <param name="referer">Referer头</param>
		/// <returns>
		/// 响应正文
		/// </returns>
		public string Get(string getUri, string accept, string referer)
		{
			string file = null;

			try
			{
				HttpWebRequest request = WebRequest.Create(getUri) as HttpWebRequest;
				request.Accept = accept;
				request.AllowAutoRedirect = true;
				request.CookieContainer = cc;
				request.KeepAlive = true;
				request.Method = "GET";
				request.Referer = referer;
				request.UserAgent = UserAgent;
				using (HttpWebResponse responce = request.GetResponse() as HttpWebResponse)
				using (StreamReader sr = new StreamReader(responce.GetResponseStream(), Encoding))
					file = sr.ReadToEnd();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				if (ThrowException)
					throw;
			}

			return file;
		}
		/// <summary>
		/// 用Get方法发送请求
		/// </summary>
		/// <param name="getUri">请求的地址</param>
		/// <returns>响应正文</returns>
		public string Get(string getUri)
		{
			return Get(getUri, Accept, null);
		}

		/// <summary>
		/// 用Get方法发送请求
		/// </summary>
		/// <param name="getUri">请求的地址</param>
		/// <param name="referer">Referer头</param>
		/// <returns>响应正文</returns>
		public string Get(string getUri, string referer)
		{
			return Get(getUri, Accept, referer);
		}
		/// <summary>
		/// 读取Cookies
		/// </summary>
		/// <returns>成功与否</returns>
		public static bool LoadCookies()
		{
			try
			{
				using (FileStream stream = File.OpenRead(Path.Combine(DataFolder, "cookies.dat")))
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
				if (!Directory.Exists(DataFolder))
					Directory.CreateDirectory(DataFolder);
				using (FileStream stream = File.OpenWrite(Path.Combine(DataFolder, "cookies.dat")))
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
		/// <summary>
		/// 根据请求的URL和参数构造一个新的URL
		/// </summary>
		/// <param name="baseUrl">请求URL</param>
		/// <param name="parameters">参数</param>
		/// <returns>新的URL</returns>
		public static string ConstructUrlWithParameters(string baseUrl, Parameters parameters)
		{
			if (parameters == null || parameters.Count() == 0)
				return baseUrl;
			return baseUrl + "?" + parameters;
		}

		/// <summary>
		/// 设置代理
		/// </summary>
		/// <param name="host">主机名</param>
		/// <param name="port">端口</param>
		public static void SetProxy(string host, int port)
		{
			WebRequest.DefaultWebProxy = new WebProxy(host, port);
		}
		/// <summary>
		/// 使用默认代理
		/// </summary>
		public static void ResetProxy()
		{
			WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
		}
		
	}

	/// <summary>
	/// 多个URL参数
	/// </summary>
	public class Parameters : Dictionary<string, string>
	{
		/// <summary>
		/// 是否添加空参数
		/// </summary>
		public bool AddEmptyParameter { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Parameters"/> class.
		/// </summary>
		/// <param name="addEmptyParameter">是否添加空参数</param>
		public Parameters(bool addEmptyParameter = false)
			:base()
		{
			AddEmptyParameter = addEmptyParameter;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var p in this)
			{
				if (AddEmptyParameter || !string.IsNullOrEmpty(p.Value))
				{
					if (sb.Length != 0) sb.Append("&");
					sb.Append(Uri.EscapeDataString(p.Key));
					sb.Append("=");
					sb.Append(Uri.EscapeDataString(p.Value == null ? string.Empty : p.Value));
				}
			}
			return sb.ToString();
		}
	}
}
