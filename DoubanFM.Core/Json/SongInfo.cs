/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DoubanFM.Core.Json
{
    /// <summary>
    /// 包含歌曲的信息。（目前主要用于获取歌词）
    /// </summary>
    [DataContract]
    internal class SongInfo
    {
        [DataMember(Name = "artist_region")]
        public string ArtistRegion { get; set; }

        [DataMember(Name = "album_stars")]
        public int AlbumStars { get; set; }

        [DataMember(Name = "album_rate")]
        public double AlbumRate { get; set; }

        [DataMember(Name = "artist_name")]
        public string ArtistName { get; set; }

        //[DataMember(Name = "photos")]
        //public SongPhoto Photos { get; set; }

        [DataMember(Name = "lyric")]
        public string Lyric { get; set; }

        //[DataMember(Name = "albums")]
        //public SongAlbum[] Albums { get; set; }

        [DataMember(Name = "artist_birth")]
        public string ArtistBirth { get; set; }

        [DataMember(Name = "subject_id")]
        public string SubjectID { get; set; }

        [DataMember(Name = "album_intro")]
        public string AlbumIntro { get; set; }

        [DataMember(Name = "artist_intro")]
        public string ArtistIntro { get; set; }

        [DataMember(Name = "artist_id")]
        public int ArtistID { get; set; }

        [DataMember(Name = "artist_gene")]
        public string ArtistGene { get; set; }

    }
}