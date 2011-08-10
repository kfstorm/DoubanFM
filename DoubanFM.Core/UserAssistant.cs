using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Threading;

namespace DoubanFM.Core
{
    /// <summary>
    /// 处理用户的登录、注销等请求
    /// </summary>
    public class UserAssistant : DependencyObject
    {
        #region 依赖项属性

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(State), typeof(UserAssistant));
        public static readonly DependencyProperty IsLoggedOnProperty = DependencyProperty.Register("IsLoggedOn", typeof(bool), typeof(UserAssistant));
        public static readonly DependencyProperty IsLoggedOffProperty = DependencyProperty.Register("IsLoggedOff", typeof(bool), typeof(UserAssistant));
        public static readonly DependencyProperty IsLoggingOnProperty = DependencyProperty.Register("IsLoggingOn", typeof(bool), typeof(UserAssistant));
        public static readonly DependencyProperty IsLoggingOffProperty = DependencyProperty.Register("IsLoggingOff", typeof(bool), typeof(UserAssistant));
        public static readonly DependencyProperty HasCaptchaProperty = DependencyProperty.Register("HasCaptcha", typeof(bool), typeof(UserAssistant));
        public static readonly DependencyProperty ShowLogOnFailedHintProperty = DependencyProperty.Register("ShowLogOnFailedHint", typeof(bool), typeof(UserAssistant));

        #endregion

        #region 属性

        /// <summary>
        /// 用户
        /// </summary>
        public Settings Settings { get; internal set; }
        /// <summary>
        ///  状态枚举
        /// </summary>
        public enum State { Unknown, LoggedOff, LoggingOn, LoggedOn, LoggingOff };
        /// <summary>
        /// 当前状态
        /// </summary>
        public State CurrentState
        {
            get { return (State)GetValue(StateProperty); }
            set
            {
                State lastState = CurrentState;
                SetValue(StateProperty, value);
                SetValue(IsLoggedOnProperty, CurrentState == State.LoggedOn);
                SetValue(IsLoggedOffProperty, CurrentState == State.LoggedOff);
                SetValue(IsLoggingOnProperty, CurrentState == State.LoggingOn);
                SetValue(IsLoggingOffProperty, CurrentState == State.LoggingOff);
                ShowLogOnFailedHint = false;
                if (lastState == State.LoggingOn)
                    if (CurrentState == State.LoggedOn)
                        RaiseLogOnSucceedEvent();
                    else if (CurrentState == State.LoggedOff)
                    {
                        ShowLogOnFailedHint = true;
                        RaiseLogOnFailedEvent();
                    }
                if (lastState == State.LoggingOff)
                    if (CurrentState == State.LoggedOff)
                        RaiseLogOffSucceedEvent();
                    else if (CurrentState == State.LoggedOn)
                        RaiseLogOffFailedEvent();
                //if (lastState == State.Unknown && CurrentState == State.LoggedOff)      //目前从http://douban.fm登录肯定不需要验证码，所以这里注释掉
                    //Refresh();
            }
        }
        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsLoggedOn { get { return (bool)GetValue(IsLoggedOnProperty); } }
        /// <summary>
        /// 是否正在登录
        /// </summary>
        public bool IsLoggingOn { get { return (bool)GetValue(IsLoggingOnProperty); } }
        /// <summary>
        /// 是否已注销
        /// </summary>
        public bool IsLoggedOff { get { return (bool)GetValue(IsLoggedOffProperty); } }
        /// <summary>
        /// 是否正在注销
        /// </summary>
        public bool IsLoggingOff { get { return (bool)GetValue(IsLoggingOffProperty); } }
        /// <summary>
        /// 是否显示登录失败的提示信息
        /// </summary>
        public bool ShowLogOnFailedHint
        {
            get { return (bool)GetValue(ShowLogOnFailedHintProperty); }
            private set { SetValue(ShowLogOnFailedHintProperty, value); }
        }
        /// <summary>
        /// 验证码URL
        /// </summary>
        public string CaptchaUrl
        {
            get
            {
                if (HasCaptcha) return "https://www.douban.com/misc/captcha?id=" + _captchaId + "&amp;size=s";
                else return null;
            }
        }
        /// <summary>
        /// 是否要求输入验证码
        /// </summary>
        public bool HasCaptcha
        {
            get { return (bool)GetValue(HasCaptchaProperty); }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 当登录成功时发生。
        /// </summary>
        public event EventHandler LogOnSucceed;
        /// <summary>
        /// 当注销成功时发生。
        /// </summary>
        public event EventHandler LogOffSucceed;
        /// <summary>
        /// 当登录失败时发生。
        /// </summary>
        public event EventHandler LogOnFailed;
        /// <summary>
        /// 当注销失败时发生。
        /// </summary>
        public event EventHandler LogOffFailed;
        void RaiseLogOnSucceedEvent()
        {
            if (LogOnSucceed != null)
                LogOnSucceed(this, EventArgs.Empty);
        }
        void RaiseLogOffSucceedEvent()
        {
            if (LogOffSucceed != null)
                LogOffSucceed(this, EventArgs.Empty);
        }
        void RaiseLogOnFailedEvent()
        {
            if (LogOnFailed != null)
                LogOnFailed(this, EventArgs.Empty);
        }
        void RaiseLogOffFailedEvent()
        {
            if (LogOffFailed != null)
                LogOffFailed(this, EventArgs.Empty);
        }

        #endregion

        #region 私用变量

        /// <summary>
        /// 验证码ID
        /// </summary>
        private string _captchaId;
        private string CaptchaId
        {
            get { return _captchaId; }
            set
            {
                _captchaId = value;
                SetValue(HasCaptchaProperty, !string.IsNullOrEmpty(_captchaId));
            }
        }
        
        /// <summary>
        /// 注销链接
        /// </summary>
        private string _logOffLink;

        #endregion

        #region 成员方法

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="html">HTML文件</param>
        internal void Update(string html)
        {
            //_captchaId = GetCaptchaId(html);          //目前从http://douban.fm登录肯定不需要验证码，所以这里注释掉
            _logOffLink = GetLogOffLink(html);
            string s = null;
            if (!string.IsNullOrEmpty(html))
            {
                Match match = Regex.Match(html, @"var\s*globalConfig\s*=\s*{\s*uid\s*:\s*'(\d*)'", RegexOptions.IgnoreCase);
                s = match.Groups[1].Value;
                Dispatcher.Invoke(new Action(() =>
                {
                    if (!string.IsNullOrEmpty(s)) CurrentState = State.LoggedOn;
                    else CurrentState = State.LoggedOff;
                }));
            }
            else
                Dispatcher.Invoke(new Action(() =>
                    {
                        if (CurrentState == State.LoggingOn) CurrentState = State.LoggedOff;
                        else if (CurrentState == State.LoggingOff) CurrentState = State.LoggedOn;
                        else if (CurrentState == State.Unknown) CurrentState = State.LoggedOff;
                        else CurrentState = State.LoggedOff;
                    }));
        }
        /// <summary>
        /// 刷新登录页面
        /// </summary>
        public void Refresh()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
                {
                    string file = new ConnectionBase().Get("https://www.douban.com/accounts/login");
                    Dispatcher.Invoke(new Action(() => { Update(file); }));
                }));
        }
        /// <summary>
        /// 获取验证码ID
        /// </summary>
        /// <param name="html">HTML文件</param>
        /// <returns></returns>
        string GetCaptchaId(string html)
        {
            if (html == null) return null;
            Match match = Regex.Match(html, "<img src=\"http[s]?://www\\.douban\\.com/misc/captcha\\?id=(\\w*)", RegexOptions.IgnoreCase);
            return match.Groups[1].Value;
        }
        /// <summary>
        /// 获取注销链接
        /// </summary>
        /// <param name="html">HTML文件</param>
        /// <returns></returns>
        string GetLogOffLink(string html)
        {
            if (html == null) return null;
            Match match = Regex.Match(html, "\"(http://www\\.douban\\.com/accounts/logout\\?source=radio&[^\\s]*)\"", RegexOptions.IgnoreCase);
            return match.Groups[1].Value;
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="Captcha">验证码</param>
        public void LogOn(string Captcha)
        {
            if (CurrentState != State.LoggedOff) return;
            CurrentState = State.LoggingOn;
            string postData = "source=radio&form_email=" + Settings.User.Username
                + "&form_password=" + Settings.User.Password;
            if (_captchaId != null && _captchaId.Length != 0)
                postData += "&captcha-solution=" + Captcha + "&captcha-id=" + _captchaId;
            if (Settings.AutoLogOnNextTime)
                postData += "&remember=on";
            ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
                {
                    string file = new ConnectionBase().Post("https://www.douban.com/accounts/login",
                        "http://www.douban.com/accounts/login?source=radio", Encoding.Default.GetBytes(postData));
                    Dispatcher.Invoke(new Action(() => { Update(file); }));
                }));
        }
        /// <summary>
        /// 注销
        /// </summary>
        public void LogOff()
        {
            if (CurrentState != State.LoggedOn) return;
            CurrentState = State.LoggingOff;
            ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
                {
                    new ConnectionBase().Get(_logOffLink);
                    string file = new ConnectionBase().Get("http://douban.fm");
                    Dispatcher.Invoke(new Action(() => { Update(file); }));
                }));
        }

        /// <summary>
        /// 强制注销，非后台
        /// </summary>
        internal void ForceLogOff()
        {
            new ConnectionBase().Get(_logOffLink);
        }
        #endregion
    }
}
