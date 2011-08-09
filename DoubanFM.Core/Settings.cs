using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubanFM.Core
{
    /// <summary>
    /// 偏好设置
    /// </summary>
    [Serializable]
    public class Settings
    {
        /// <summary>
        /// 用户
        /// </summary>
        public User User
        {
            get;
            set;
        }
        /// <summary>
        /// 记住密码
        /// </summary>
        public bool RememberPassword
        {
            get;
            set;
        }
        /// <summary>
        /// 下次自动登录
        /// </summary>
        public bool AutoLogOnNextTime
        {
            get;
            set;
        }
        /// <summary>
        /// 记住最后播放的频道
        /// </summary>
        public bool RememberLastChannel
        {
            get;
            set;
        }
        /// <summary>
        /// 最后播放的频道
        /// </summary>
        public Channel LastChannel
        {
            get;
            set;
        }
        /// <summary>
        /// 静音
        /// </summary>
        public bool IsMuted
        {
            get;
            set;
        }
        /// <summary>
        /// 音量
        /// </summary>
        public double Volume
        {
            get;
            set;
        }
        /// <summary>
        /// 当鼠标移动到封面上时滑动封面
        /// </summary>
        public bool SlideCoverWhenMouseMove
        {
            get;
            set;
        }

        internal Settings(User user)
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
        internal Settings(string username, string password)
            : this(new User(username, password)) { }
        internal Settings()
            : this("", "") { }

    }
}
