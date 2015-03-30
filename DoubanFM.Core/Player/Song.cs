/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Core
{
	/// <summary>
	/// 歌曲
	/// </summary>
	public class Song : ICloneable, IEquatable<Song>
	{
		/// <summary>
		/// 音乐文件URL
		/// </summary>
		public string FileUrl { get; set; }
		/// <summary>
		/// 标题
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// 表演者
		/// </summary>
		public string Artist { get; set; }
		/// <summary>
		/// 专辑
		/// </summary>
		public string Album { get; set; }
		/// <summary>
		/// 唱片公司
		/// </summary>
		public string Company { get; set; }
		/// <summary>
		/// 封面URL
		/// </summary>
		public string Picture { get; set; }
		/// <summary>
		/// 长度
		/// </summary>
		public TimeSpan Length { get; set; }
		/// <summary>
		/// 发行年
		/// </summary>
		public string PublicTime { get; set; }
		/// <summary>
		/// 专辑的豆瓣资料页面
		/// </summary>
		public string AlbumInfo { get; set; }
		public string SSId { get; set; }
		/// <summary>
		/// 平均评分
		/// </summary>
		public double Rate { get; set; }
		public string SourceUrl { get; set; }
		/// <summary>
		/// 普通音乐应该是""，广告应该是"T"
		/// </summary>
		public string Type { get; set; }
		/// <summary>
		/// 歌曲ID
		/// </summary>
		public string SongId { get; set; }
		/// <summary>
		/// 专辑ID
		/// </summary>
		public string AlbumId { get; set; }
		/// <summary>
		/// 当前用户喜欢与否
		/// </summary>
		public bool Like { get; set; }

		/// <summary>
		/// 是否是广告
		/// </summary>
		public bool IsAd { get { return Type == "T"; } }

		private Song()
		{
		}

		internal Song(DoubanFM.Core.Json.Song song)
		{
			FileUrl = song.url;
			Title = song.title;
			Artist = song.artist;
			Album = song.albumtitle;
			Company = song.company;
			Picture = song.picture;
            Length = TimeSpan.FromSeconds(song.length);
			PublicTime = song.public_time;
			AlbumInfo = song.album;
			SSId = song.ssid;
			Rate = song.rating_avg;
			SourceUrl = song.source_url;
			Type = song.subtype;
			SongId = song.sid;
			AlbumId = song.aid;
			Like = song.like;
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Title);
			sb.Append(" - ");
			sb.Append(Artist);
			sb.Append(" - ");
			sb.Append(Album);
			return sb.ToString();
		}

		public bool Equals(Song other)
		{
			if (Object.ReferenceEquals(other, null))
				return false;
			if (Object.ReferenceEquals(this, other))
				return true;
			return this.FileUrl == other.FileUrl;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Song);
		}

		public override int GetHashCode()
		{
			if (FileUrl == null) return 0;
			return FileUrl.GetHashCode();
		}

		public static bool operator ==(Song lhs, Song rhs)
		{
			if (Object.ReferenceEquals(lhs, null))
				if (Object.ReferenceEquals(rhs, null))
					return true;
				else return false;
			return lhs.Equals(rhs);
		}
		public static bool operator !=(Song lhs, Song rhs)
		{
			return !(lhs == rhs);
		}
	}
}