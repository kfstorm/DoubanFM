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
	public class User : DependencyObject, ISerializable
	{
		#region

		public static readonly DependencyProperty UsernameProperty = DependencyProperty.Register("Username", typeof(string), typeof(User));
		public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password", typeof(string), typeof(User));
		public static readonly DependencyProperty NicknameProperty = DependencyProperty.Register("Nickname", typeof(string), typeof(User));
		public static readonly DependencyProperty PlayedProperty = DependencyProperty.Register("Played", typeof(int), typeof(User));
		public static readonly DependencyProperty LikedProperty = DependencyProperty.Register("Liked", typeof(int), typeof(User));
		public static readonly DependencyProperty BannedProperty = DependencyProperty.Register("Banned", typeof(int), typeof(User));
		#endregion

		/// <summary>
		/// 用户名
		/// </summary>
		public string Username
		{
			get { return (string)GetValue(UsernameProperty); }
			set { SetValue(UsernameProperty, value); }
		}
		/// <summary>
		/// 密码
		/// </summary>
		public string Password
		{
			get { return (string)GetValue(PasswordProperty); }
			set { SetValue(PasswordProperty, value); }
		}
		/// <summary>
		/// 昵称
		/// </summary>
		public string Nickname
		{
			get { return (string)GetValue(NicknameProperty); }
			set { SetValue(NicknameProperty, value); }
		}
		/// <summary>
		/// 累计播放数量
		/// </summary>
		public int Played
		{
			get { return (int)GetValue(PlayedProperty); }
			set { SetValue(PlayedProperty, value); }
		}
		/// <summary>
		/// 加红心数量
		/// </summary>
		public int Liked
		{
			get { return (int)GetValue(LikedProperty); }
			set { SetValue(LikedProperty, value); }
		}
		/// <summary>
		/// 不再播放数量
		/// </summary>
		public int Banned
		{
			get { return (int)GetValue(BannedProperty); }
			set { SetValue(BannedProperty, value); }
		}

		internal User(string Username, string Password)
		{
			this.Username = Username;
			this.Password = Password;
		}

		private User()
		{
			Username = string.Empty;
			Password = string.Empty;
		}

		protected User(SerializationInfo info, StreamingContext context)
		{
			User def = new User();
			try
			{
				Username = info.GetString("Username");
			}
			catch
			{
				Username = def.Username;
			}
			try
			{
				Password = Encryption.Decrypt(info.GetString("Password"));
			}
			catch
			{
				Password = def.Password;
			}
			try
			{
				Nickname = info.GetString("Nickname");
			}
			catch
			{
				Nickname = def.Nickname;
			}
			try
			{
				Played = info.GetInt32("Played");
			}
			catch
			{
				Played = def.Played;
			}
			try
			{
				Liked = info.GetInt32("Liked");
			}
			catch
			{
				Liked = def.Liked;
			}
			try
			{
				Banned = info.GetInt32("Banned");
			}
			catch
			{
				Banned = def.Banned;
			}
			
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Username", Username);
			info.AddValue("Password", Encryption.Encrypt(Password ?? string.Empty));
			info.AddValue("Nickname", Nickname);
			info.AddValue("Played", Played);
			info.AddValue("Liked", Liked);
			info.AddValue("Banned", Banned);
		}
	}
}
