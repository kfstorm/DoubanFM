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

        public User(SerializationInfo info, StreamingContext context)
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
                Password = info.GetString("Password");
            }
            catch
            {
                Password = def.Password;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Username", Username);
            info.AddValue("Password", Password);
        }
    }
}
