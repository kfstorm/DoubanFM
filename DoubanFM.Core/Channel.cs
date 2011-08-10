using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// DJ频道特有的属性，节目ID（Program ID?）
        /// </summary>
        public string ProgramId { get; private set; }
        /// <summary>
        /// 上下文
        /// </summary>
        public string Context { get; private set; }

        /// <summary>
        /// 是否是DJ频道
        /// </summary>
        public bool IsDj { get { return Id == DjId; } }
        /// <summary>
        /// 是否是私人频道
        /// </summary>
        public bool IsPersonal { get { return Id == PersonalId; } }
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
            ProgramId = channel.pid;
        }

        internal Channel(string id, string name, string programId, string context = null)
        {
            Id = id;
            Name = name;
            ProgramId = programId;
            Context = context;
        }

        internal const string PersonalId = "0";
        internal const string DjId = "dj";

        public static readonly Channel PersonalChannel = new Channel(PersonalId, "私人兆赫", null);
        
        public object Clone()
        {
            return new Channel(Id, Name, ProgramId, Context);
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
            return Id.GetHashCode() ^ Name.GetHashCode() ^ (string.IsNullOrEmpty(ProgramId) ? 0 : ProgramId.GetHashCode()) ^ (string.IsNullOrEmpty(Context) ? 0 : Context.GetHashCode());
        }

        public bool Equals(Channel other)
        {
            if (Object.ReferenceEquals(other, null))
                return false;
            if (Object.ReferenceEquals(this, other))
                return true;
            if (this.GetType() != other.GetType())
                return false;
            return Id == other.Id && Name == other.Name &&
                ((string.IsNullOrEmpty(ProgramId) && string.IsNullOrEmpty(other.ProgramId)) || ProgramId == other.ProgramId) &&
                ((string.IsNullOrEmpty(Context) && string.IsNullOrEmpty(other.Context)) || Context == other.Context);
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