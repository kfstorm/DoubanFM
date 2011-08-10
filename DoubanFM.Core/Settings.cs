using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.Serialization;

namespace DoubanFM.Core
{
    /// <summary>
    /// 偏好设置
    /// </summary>
    [Serializable]
    public class Settings : DependencyObject, ISerializable
    {
        #region 依赖项属性

        public static readonly DependencyProperty UserProperty = DependencyProperty.Register("User", typeof(User), typeof(Settings));
        public static readonly DependencyProperty RememberPasswordProperty = DependencyProperty.Register("RememberPassword", typeof(bool), typeof(Settings));
        public static readonly DependencyProperty AutoLogOnNextTimeProperty = DependencyProperty.Register("AutoLogOnNextTime", typeof(bool), typeof(Settings));
        public static readonly DependencyProperty RememberLastChannelProperty = DependencyProperty.Register("RememberLastChannel", typeof(bool), typeof(Settings));
        public static readonly DependencyProperty LastChannelProperty = DependencyProperty.Register("LastChannel", typeof(Channel), typeof(Settings));
        public static readonly DependencyProperty IsMutedProperty = DependencyProperty.Register("IsMuted", typeof(bool), typeof(Settings));
        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register("Volume", typeof(double), typeof(Settings));
        public static readonly DependencyProperty SlideCoverWhenMouseMoveProperty = DependencyProperty.Register("SlideCoverWhenMouseMove", typeof(bool), typeof(Settings));
        public static readonly DependencyProperty IsShadowEnabledProperty = DependencyProperty.Register("IsShadowEnabled", typeof(bool), typeof(Settings));

        #endregion

        /// <summary>
        /// 用户
        /// </summary>
        public User User
        {
            get { return (User)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }
        /// <summary>
        /// 记住密码
        /// </summary>
        public bool RememberPassword
        {
            get { return (bool)GetValue(RememberPasswordProperty); }
            set { SetValue(RememberPasswordProperty, value); }
        }
        /// <summary>
        /// 下次自动登录
        /// </summary>
        public bool AutoLogOnNextTime
        {
            get { return (bool)GetValue(AutoLogOnNextTimeProperty); }
            set { SetValue(AutoLogOnNextTimeProperty, value); }
        }
        /// <summary>
        /// 记住最后播放的频道
        /// </summary>
        public bool RememberLastChannel
        {
            get { return (bool)GetValue(RememberLastChannelProperty); }
            set { SetValue(RememberLastChannelProperty, value); }
        }
        /// <summary>
        /// 最后播放的频道
        /// </summary>
        public Channel LastChannel
        {
            get { return (Channel)GetValue(LastChannelProperty); }
            set { SetValue(LastChannelProperty, value); }
        }
        /// <summary>
        /// 静音
        /// </summary>
        public bool IsMuted
        {
            get { return (bool)GetValue(IsMutedProperty); }
            set { SetValue(IsMutedProperty, value); }
        }
        /// <summary>
        /// 音量
        /// </summary>
        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }
        /// <summary>
        /// 当鼠标移动到封面上时滑动封面
        /// </summary>
        public bool SlideCoverWhenMouseMove
        {
            get { return (bool)GetValue(SlideCoverWhenMouseMoveProperty); }
            set { SetValue(SlideCoverWhenMouseMoveProperty, value); }
        }
        /// <summary>
        /// 开启窗口阴影
        /// </summary>
        public bool IsShadowEnabled
        {
            get { return (bool)GetValue(IsShadowEnabledProperty); }
            set { SetValue(IsShadowEnabledProperty, value); }
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
            IsShadowEnabled = true;
        }
        internal Settings(string username, string password)
            : this(new User(username, password)) { }
        internal Settings()
            : this("", "") { }

        public Settings(SerializationInfo info, StreamingContext context)
        {
            Settings def = new Settings();
            try
            {
                User = (User)info.GetValue("User", typeof(User));
            }
            catch
            {
                User = def.User;
            }
            try
            {
                RememberPassword = info.GetBoolean("RememberPassword");
            }
            catch
            {
                RememberPassword = def.RememberPassword;
            }
            try
            {
                AutoLogOnNextTime = info.GetBoolean("AutoLogOnNextTime");
            }
            catch
            {
                AutoLogOnNextTime = def.AutoLogOnNextTime;
            }
            try
            {
                RememberLastChannel = info.GetBoolean("RememberLastChannel");
            }
            catch
            {
                RememberLastChannel = def.RememberLastChannel;
            }
            try
            {
                LastChannel = (Channel)info.GetValue("LastChannel", typeof(Channel));
            }
            catch
            {
                LastChannel = def.LastChannel;
            }
            try
            {
                IsMuted = info.GetBoolean("IsMuted");
            }
            catch
            {
                IsMuted = def.IsMuted;
            }
            try
            {
                Volume = info.GetDouble("Volume");
            }
            catch
            {
                Volume = def.Volume;
            }
            try
            {
                SlideCoverWhenMouseMove = info.GetBoolean("SlideCoverWhenMouseMove");
            }
            catch
            {
                SlideCoverWhenMouseMove = def.SlideCoverWhenMouseMove;
            }
            try
            {
                IsShadowEnabled = info.GetBoolean("IsShadowEnabled");
            }
            catch
            {
                IsShadowEnabled = def.IsShadowEnabled;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("User", User);
            info.AddValue("RememberPassword", RememberPassword);
            info.AddValue("AutoLogOnNextTime", AutoLogOnNextTime);
            info.AddValue("RememberLastChannel", RememberLastChannel);
            info.AddValue("LastChannel", LastChannel);
            info.AddValue("IsMuted", IsMuted);
            info.AddValue("Volume", Volume);
            info.AddValue("SlideCoverWhenMouseMove", SlideCoverWhenMouseMove);
            info.AddValue("IsShadowEnabled", IsShadowEnabled);
        }
    }
}
