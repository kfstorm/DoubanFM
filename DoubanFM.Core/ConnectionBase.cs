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
		/// 默认HTTP头：Accept
		/// </summary>
		public static string DefaultAccept = "text/html, application/xhtml+xml, */*";
		/// <summary>
		/// 默认HTTP头：ContentType
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

		internal ConnectionBase(bool throwException, string userAgent, string accept, string contentType, Encoding encoding)
		{
			ThrowException = throwException;
			UserAgent = userAgent;
			Accept = accept;
			ContentType = contentType;
			Encoding = encoding;
		}

		internal ConnectionBase(bool throwException = false)
			: this(DefaultEncoding, throwException)
		{
		}

		internal ConnectionBase(Encoding encoding, bool throwException = false)
			: this(throwException, DefaultUserAgent, DefaultAccept, DefaultContentType, encoding)
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
		/// <returns>
		/// 响应正文
		/// </returns>
		internal string Post(string PostUri, string Accept, string Referer, string ContentType, byte[] Content)
		{
			string file = null;

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
				using (StreamReader sr = new StreamReader(responce.GetResponseStream(), Encoding))
					file = sr.ReadToEnd();
			}
			catch (Exception e)
			{
				if (ThrowException)
					throw e;
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
		/// <returns>
		/// 响应正文
		/// </returns>
		internal string Get(string GetUri, string Accept, string Referer)
		{
			string file = null;

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
				using (StreamReader sr = new StreamReader(responce.GetResponseStream(), Encoding))
					file = sr.ReadToEnd();
			}
			catch (Exception e)
			{
				if (ThrowException)
					throw e;
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
		internal static bool LoadCookies()
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
		internal static bool SaveCookies()
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
		internal static string ConstructUrlWithParameters(string baseUrl, Parameters parameters)
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
	/// URL参数
	/// </summary>
	class UrlParameter
	{
		/// <summary>
		/// 参数名
		/// </summary>
		public string Key { get; set; }
		/// <summary>
		/// 参数值
		/// </summary>
		public string Value { get; set; }

		internal UrlParameter(string key, string value)
		{
			Key = key;
			Value = value;
		}
	}
	/// <summary>
	/// 多个URL参数
	/// </summary>
	class Parameters : List<UrlParameter>
	{
		/// <summary>
		/// 是否添加空参数
		/// </summary>
		public bool AddEmptyParameter { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Parameters"/> class.
		/// </summary>
		/// <param name="addEmptyParameter">是否添加空参数</param>
		internal Parameters(bool addEmptyParameter = false)
			:base()
		{
			AddEmptyParameter = addEmptyParameter;
		}

		/// <summary>
		/// 添加参数
		/// </summary>
		internal void Add(string key, string value)
		{
			if (!AddEmptyParameter && string.IsNullOrEmpty(value)) return;
			Add(new UrlParameter(key, value));
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var p in this)
			{
				if (!string.IsNullOrEmpty(p.Value))
				{
					if (sb.Length != 0) sb.Append("&");
					sb.Append(Uri.EscapeDataString(p.Key));
					sb.Append("=");
					sb.Append(Uri.EscapeDataString(p.Value));
				}
			}
			return sb.ToString();
		}
	}
}
