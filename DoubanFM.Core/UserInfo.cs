using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace DoubanFM.Core
{
    /// <summary>
    /// 用户信息
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        public UserInfo(string Username, string Password)
        {
            this.Username = Username;
            this.Password = Password;
        }
    }
}
