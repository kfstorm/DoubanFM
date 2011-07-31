using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DoubanFM.Core
{
    /// <summary>
    /// 偏好设置
    /// </summary>
    [Serializable]
    public class Settings
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        public UserInfo User { get; set; }
        /// <summary>
        /// 记住密码
        /// </summary>
        public bool RememberPassword { get; set; }
        /// <summary>
        /// 下次自动登录
        /// </summary>
        public bool AutoLogOnNextTime { get; set; }
        /// <summary>
        /// 记住最后播放的频道
        /// </summary>
        public bool RememberLastChannel { get; set; }
        /// <summary>
        /// 最后播放的频道
        /// </summary>
        public Channel LastChannel { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is muted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is muted; otherwise, <c>false</c>.
        /// </value>
        public bool IsMuted { get; set; }
        /// <summary>
        /// Gets or sets the volume.
        /// </summary>
        /// <value>
        /// The volume.
        /// </value>
        public double Volume { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [slide cover when mouse move].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [slide cover when mouse move]; otherwise, <c>false</c>.
        /// </value>
        public bool SlideCoverWhenMouseMove { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        public Settings(UserInfo user)
        {
            User = user;
            RememberPassword = false;
            AutoLogOnNextTime = true;
            RememberLastChannel = true;
            LastChannel = null;
            IsMuted = false;
            Volume = 1;
            SlideCoverWhenMouseMove = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public Settings(string username, string password)
            : this(new UserInfo(username, password)) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
            : this("", "") { }

    }
}
