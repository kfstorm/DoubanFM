using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Core
{
    /// <summary>
    /// 用户
    /// </summary>
    [Serializable]
    public class User
    {

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username
        {
            get;
            set;
        }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get;
            set;
        }

        internal User(string Username, string Password)
        {
            this.Username = Username;
            this.Password = Password;
        }
    }
}
