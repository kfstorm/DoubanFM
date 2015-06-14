/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DoubanFM.Core
{
	/// <summary>
	/// 频道
	/// </summary>
	[Serializable]
	public class Channel : ICloneable, IEquatable<Channel>
	{
		/// <summary>
		/// 频道ID
		/// </summary>
		public string Id { get; private set; }
		/// <summary>
		/// 频道名称
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// 上下文
		/// </summary>
		public string Context { get; private set; }

		/// <summary>
		/// 是否是有效的频道
		/// </summary>
		public bool IsEffective { get { return !string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(Name); } }

		/// <summary>
		/// 是否是DJ频道
		/// </summary>
		public bool IsDj
		{
			get
			{
				int x = 0;
				if (!int.TryParse(Id, out x))
				{
					return false;
				}
				return x >= 1000000;
			}
		}
		/// <summary>
		/// 是否是私人频道
		/// </summary>
		public bool IsPersonal { get { return Id == PersonalId || Id == RedHeartId; } }
		/// <summary>
		/// 是否是红心频道
		/// </summary>
		public bool IsRedHeart { get { return Id == RedHeartId; } }
		/// <summary>
		/// 是否是公共频道
		/// </summary>
		public bool IsPublic { get { return !IsDj && !IsPersonal; } }
		/// <summary>
		/// 是否是特殊模式
		/// </summary>
		public bool IsSpecial { get { return !string.IsNullOrEmpty(Context); } }

		internal Channel(Json.Channel channel)
		{
			Id = channel.channel_id;
			Name = channel.name;
		}

		internal Channel(string id, string name, string context = null)
		{
			Id = id;
			Name = name;
			Context = context;
		}

		/// <summary>
		/// 由命令行参数构造Channel
		/// </summary>
		/// <param name="args">The args.</param>
		/// <returns></returns>
		public static Channel FromCommandLineArgs(List<string> args)
		{
			if (args.Contains("-channel"))
			{
				int index = args.IndexOf("-channel");
				if (index != -1)
					try
					{
						string id = Encoding.Unicode.GetString(Convert.FromBase64String(args.ElementAt(index + 1)));
						string name = Encoding.Unicode.GetString(Convert.FromBase64String(args.ElementAt(index + 2)));
						string context = Encoding.Unicode.GetString(Convert.FromBase64String(args.ElementAt(index + 3)));
						return new Channel(id, name, context);
					}
					catch { }
			}
			return null;
		}

		/// <summary>
		/// 由命令行参数构造Channel（简单版本，不适用于复杂的命令行）
		/// </summary>
		/// <param name="args">命令行</param>
		/// <returns></returns>
		public static Channel FromCommandLineArgs(string commandLine)
		{
			List<string> args = new List<string>();
			MatchCollection mc = Regex.Matches(commandLine, @"""(.*?)""|([^""\s]+)");
			foreach (Match ma in mc)
			{
				for (int i = 1; i < ma.Groups.Count; ++i)
				{
					if (ma.Groups[i].Success)
					{
						args.Add(ma.Groups[i].Value);
					}
				}
			}
			return FromCommandLineArgs(args);
		}

		/// <summary>
		/// 转换为命令行参数
		/// </summary>
		/// <returns></returns>
		public string ToCommandLineArgs()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("-channel ");

			sb.Append("\"");
			sb.Append(Convert.ToBase64String(Encoding.Unicode.GetBytes(Id)));
			sb.Append("\" ");

			sb.Append("\"");
			sb.Append(Convert.ToBase64String(Encoding.Unicode.GetBytes(Name)));
			sb.Append("\" ");

			sb.Append("\"");
			sb.Append(Convert.ToBase64String(Encoding.Unicode.GetBytes(Context == null ? "" : Context)));
			sb.Append("\" ");

			return sb.ToString();
		}

		/// <summary>
		/// 私人兆赫的频道ID
		/// </summary>
		public const string PersonalId = "0";
		/// <summary>
		/// 红心兆赫的频道ID
		/// </summary>
		public const string RedHeartId = "-3";

		/// <summary>
		/// 创建作为当前实例副本的新对象。
		/// </summary>
		/// <returns>
		/// 作为此实例副本的新对象。
		/// </returns>
		public object Clone()
		{
			return this.MemberwiseClone();
		}

		public override string ToString()
		{
			return Name;
		}
		public override bool Equals(object obj)
		{
			return this.Equals(obj as Channel);
		}
		public override int GetHashCode()
		{
			return Id.GetHashCode() ^ Name.GetHashCode() ^ (string.IsNullOrEmpty(Context) ? 0 : Context.GetHashCode());
		}

		public bool Equals(Channel other)
		{
			if (Object.ReferenceEquals(other, null))
				return false;
			if (Object.ReferenceEquals(this, other))
				return true;
			return Id == other.Id && ((string.IsNullOrEmpty(Context) && string.IsNullOrEmpty(other.Context)) || Context == other.Context);
		}

		public static bool operator ==(Channel lhs, Channel rhs)
		{
			if (Object.ReferenceEquals(lhs, null))
				if (Object.ReferenceEquals(rhs, null))
					return true;
				else return false;
			return lhs.Equals(rhs);
		}
		public static bool operator !=(Channel lhs, Channel rhs)
		{
			return !(lhs == rhs);
		}
	}
}