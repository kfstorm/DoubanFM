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
    public class Channel : ICloneable
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
            if (System.Object.ReferenceEquals(this, obj))
                return true;
            Channel rh = obj as Channel;
            if (rh == null)
                return false;
            return Id == rh.Id && Name == rh.Name && ProgramId == rh.ProgramId && Context == rh.Context;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Name.GetHashCode() ^ (ProgramId == null ? 0 : ProgramId.GetHashCode()) ^ (Context == null ? 0 : Context.GetHashCode());
        }
    }
}