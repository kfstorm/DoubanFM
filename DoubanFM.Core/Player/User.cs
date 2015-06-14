/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.Serialization;

namespace DoubanFM.Core
{
	/// <summary>
	/// 用户
	/// </summary>
	[Serializable]
	public class User : ISerializable, ICloneable
	{
		/// <summary>
		/// 用户名
		/// </summary>
		public string Username { get; set; }
		/// <summary>
		/// 密码
		/// </summary>
		public string Password { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string Nickname { get; set; }
        /// <summary>
        /// 电子邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Expire
        /// </summary>
        public string Expire { get; set; }
        /// <summary>
        /// 累计播放数量
        /// </summary>
        public int Played { get; set; }
        /// <summary>
        /// 加红心数量
        /// </summary>
        public int Liked { get; set; }
        /// <summary>
        /// 不再播放数量
        /// </summary>
        public int Banned { get; set; }

        /// <summary>
        /// 是否使用Pro服务
        /// </summary>
        public bool IsPro { get; set; }

        /// <summary>
        /// Pro服务比特率设置
        /// </summary>
        public ProRate ProRate { get; set; }

		internal User(string username, string password)
		{
			Username = username;
			Password = password;
		}

		private User()
		{
			Username = string.Empty;
			Password = string.Empty;
		}

	    protected User(SerializationInfo info, StreamingContext context)
	    {
	        try
	        {
	            Username = info.GetString("Username");
	        }
	        catch
	        {
	        }
	        try
	        {
	            Password = Encryption.Decrypt(info.GetString("Password"));
	        }
	        catch
	        {
	        }
	        try
	        {
	            UserID = info.GetString("UserID");
	        }
	        catch
	        {
	        }
	        try
	        {
	            Nickname = info.GetString("Nickname");
	        }
	        catch
	        {
	        }
	        try
	        {
	            Email = info.GetString("Email");
	        }
	        catch
	        {
	        }
	        try
	        {
	            Token = info.GetString("Token");
	        }
	        catch
	        {
	        }
	        try
	        {
	            Expire = info.GetString("Expire");
	        }
	        catch
	        {
	        }
	        try
	        {
	            Played = info.GetInt32("Played");
	        }
	        catch
	        {
	        }
	        try
	        {
	            Liked = info.GetInt32("Liked");
	        }
	        catch
	        {
	        }
	        try
	        {
	            Banned = info.GetInt32("Banned");
	        }
	        catch
	        {
	        }
	        try
	        {
	            IsPro = info.GetBoolean("IsPro");
	        }
	        catch{}
            try
            {
                ProRate = (ProRate) info.GetValue("ProRate", typeof (ProRate));
            }
            catch
            {
                
            }
	    }

	    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Username", Username);
			info.AddValue("Password", Encryption.Encrypt(Password ?? string.Empty));
            info.AddValue("UserID", UserID);
            info.AddValue("Nickname", Nickname);
            info.AddValue("Email", Email);
            info.AddValue("Token", Token);
            info.AddValue("Expire", Expire);
            info.AddValue("Played", Played);
            info.AddValue("Liked", Liked);
            info.AddValue("Banned", Banned);
            info.AddValue("IsPro", IsPro);
            info.AddValue("ProRate", ProRate);
        }

	    public object Clone()
	    {
	        return MemberwiseClone();
	    }
	}
}
