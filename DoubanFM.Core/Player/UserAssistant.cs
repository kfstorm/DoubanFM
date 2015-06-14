/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using DoubanFM.Core.Json;

namespace DoubanFM.Core
{
	/// <summary>
	/// 处理用户的登录、注销等请求
	/// </summary>
	public class UserAssistant : DependencyObject
	{
		#region 依赖项属性

        public static readonly DependencyProperty CurrentStateProperty = DependencyProperty.Register("CurrentState", typeof(State), typeof(UserAssistant));
		public static readonly DependencyProperty IsLoggedOnProperty = DependencyProperty.Register("IsLoggedOn", typeof(bool), typeof(UserAssistant));
		public static readonly DependencyProperty IsLoggedOffProperty = DependencyProperty.Register("IsLoggedOff", typeof(bool), typeof(UserAssistant));
		public static readonly DependencyProperty IsLoggingOnProperty = DependencyProperty.Register("IsLoggingOn", typeof(bool), typeof(UserAssistant));
		public static readonly DependencyProperty IsLoggingOffProperty = DependencyProperty.Register("IsLoggingOff", typeof(bool), typeof(UserAssistant));
        //public static readonly DependencyProperty HasCaptchaProperty = DependencyProperty.Register("HasCaptcha", typeof(bool), typeof(UserAssistant));
        //public static readonly DependencyProperty CaptchaUrlProperty = DependencyProperty.Register("CaptchaUrl", typeof(string), typeof(UserAssistant));
		public static readonly DependencyProperty ShowLogOnFailedHintProperty = DependencyProperty.Register("ShowLogOnFailedHint", typeof(bool), typeof(UserAssistant));
		public static readonly DependencyProperty LogOnFailedMessageProperty = DependencyProperty.Register("LogOnFailedMessage", typeof(string), typeof(UserAssistant));

		#endregion

		#region 属性

		/// <summary>
		/// 用户
		/// </summary>
		public Settings Settings { get; internal set; }
		/// <summary>
		///  登录状态
		/// </summary>
		public enum State
		{
			/// <summary>
			/// 未知
			/// </summary>
			Unknown,
			/// <summary>
			/// 已注销
			/// </summary>
			LoggedOff,
			/// <summary>
			/// 正在登录
			/// </summary>
			LoggingOn,
			/// <summary>
			/// 已登录
			/// </summary>
			LoggedOn,
			/// <summary>
			/// 正在注销
			/// </summary>
			LoggingOff
		};
		/// <summary>
		/// 当前状态
		/// </summary>
		public State CurrentState
		{
            get { return (State)GetValue(CurrentStateProperty); }
			set
			{
				if (CurrentState != value)
				{
					State lastState = CurrentState;
                    SetValue(CurrentStateProperty, value);
                    SetValue(IsLoggedOnProperty, CurrentState == State.LoggedOn);
					SetValue(IsLoggedOffProperty, CurrentState == State.LoggedOff);
					SetValue(IsLoggingOnProperty, CurrentState == State.LoggingOn);
					SetValue(IsLoggingOffProperty, CurrentState == State.LoggingOff);
                    RaiseCurrentStateChangedEvent(lastState, value);
                    //Settings.LastTimeLoggedOn = IsLoggedOn;
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
                    ////假定登录时始终需要验证码
                    //if (lastState == State.LoggingOn && CurrentState == State.LoggedOff) // && (HasCaptcha || ErrorNo == 1011))
                    //{
                    //    UpdateCaptcha();
                    //}
                    ////假定登录时始终需要验证码
                    //else if (lastState == State.LoggingOff && CurrentState == State.LoggedOff)// && HasCaptcha)
                    //{
                    //    UpdateCaptcha();
                    //}
				}
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
		/// 登录失败的提示消息
		/// </summary>
		public string LogOnFailedMessage
		{
			get { return (string)GetValue(LogOnFailedMessageProperty); }
			protected set { SetValue(LogOnFailedMessageProperty, value); }
		}
        ///// <summary>
        ///// 验证码URL
        ///// </summary>
        //public string CaptchaUrl
        //{
        //    get
        //    {
        //        return (string)GetValue(CaptchaUrlProperty);
        //    }
        //    protected set
        //    {
        //        SetValue(CaptchaUrlProperty, value);
        //    }
        //}
        ///// <summary>
        ///// 是否要求输入验证码
        ///// </summary>
        //public bool HasCaptcha
        //{
        //    get { return (bool)GetValue(HasCaptchaProperty); }
        //    set { SetValue(HasCaptchaProperty, value); }
        //}

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
        /// <summary>
        /// 当状态改变时发生。
        /// </summary>
        public event DependencyPropertyChangedEventHandler CurrentStateChanged;
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
        void RaiseCurrentStateChangedEvent(State oldState, State newState)
        {
            if (CurrentStateChanged != null)
                CurrentStateChanged(this, new DependencyPropertyChangedEventArgs(CurrentStateProperty, oldState, newState));
        }

		#endregion

		#region 私有变量

        ///// <summary>
        ///// 验证码ID
        ///// </summary>
        //private string captchaId;
		
		#endregion

		#region 成员方法

        ///// <summary>
        ///// 更新验证码
        ///// </summary>
        //public void UpdateCaptcha()
        //{
        //    HasCaptcha = true;
        //    captchaId = null;
        //    CaptchaUrl = null;
        //    ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
        //        {
        //            captchaId = new ConnectionBase().Get("http://douban.fm/j/new_captcha");
        //            Dispatcher.BeginInvoke(new Action(() =>
        //                {
        //                    if (string.IsNullOrEmpty(captchaId))
        //                    {
        //                        HasCaptcha = false;
        //                        captchaId = null;
        //                        CaptchaUrl = null;
        //                    }
        //                    else
        //                    {
        //                        captchaId = captchaId.Trim('\"');
        //                        HasCaptcha = true;
        //                        CaptchaUrl = @"http://douban.fm/misc/captcha?size=m&id=" + captchaId;
        //                    }
        //                }));
        //        }));
        //}

        ///// <summary>
        ///// 根据douban.fm的主页的HTML代码更新登录状态
        ///// </summary>
        ///// <param name="html">HTML文件</param>
        //internal void Update(string html)
        //{
        //    if (!string.IsNullOrEmpty(html))
        //    {
        //        //获取昵称和播放记录
        //        Match match2 = Regex.Match(html, @"""user_name""[^<>]*?>(?!{{)([^<>]*?)\s*<", RegexOptions.Singleline);

        //        string nickname = match2.Groups[1].Value;
        //        Match match3 = Regex.Match(html, @"累积收听.*?(\d+).*?首");
        //        int played = 0;
        //        int.TryParse(match3.Groups[1].Value, out played);
        //        Match match4 = Regex.Match(html, @"加红心.*?(\d+).*?首");
        //        int liked = 0;
        //        int.TryParse(match4.Groups[1].Value, out liked);
        //        Match match5 = Regex.Match(html, @"(\d+).*?首不再播放");
        //        int banned = 0;
        //        int.TryParse(match5.Groups[1].Value, out banned);

        //        //更改属性
        //        Dispatcher.Invoke(new Action(() =>
        //        {
        //            /*System.Diagnostics.Debug.WriteLine("**********************************************************************");
        //            System.Diagnostics.Debug.WriteLine(DateTime.Now + " 以下是本次“登录/注销”返回的结果页面");
        //            System.Diagnostics.Debug.Indent();
        //            System.Diagnostics.Debug.WriteLine(html);
        //            System.Diagnostics.Debug.Unindent();
        //            System.Diagnostics.Debug.WriteLine("**********************************************************************");
        //            */
        //            if (!string.IsNullOrEmpty(nickname))
        //            {
        //                Settings.User.Nickname = nickname;
        //                Settings.User.Played = played;
        //                Settings.User.Liked = liked;
        //                Settings.User.Banned = banned;
        //                System.Diagnostics.Debug.WriteLine("已登录");
        //                CurrentState = State.LoggedOn;
        //            }
        //            else
        //            {
        //                Settings.User.Nickname = string.Empty;
        //                System.Diagnostics.Debug.WriteLine("已注销");
        //                CurrentState = State.LoggedOff;
        //            }
        //        }));
        //    }
        //    else
        //        Dispatcher.Invoke(new Action(() =>
        //            {
        //                if (CurrentState == State.LoggingOn) CurrentState = State.LoggedOff;
        //                else if (CurrentState == State.LoggingOff) CurrentState = State.LoggedOn;
        //                else if (CurrentState == State.Unknown) CurrentState = State.LoggedOff;
        //                else CurrentState = State.LoggedOff;
        //            }));
        //}
	    /// <summary>
	    /// 根据服务器返回的登录结果更新登录状态
	    /// </summary>
	    private void UpdateWhenLogOn(LogOnResult result)
	    {
	        Debug.Assert(result != null, "result != null");
	        string errorMessage = null;

	        if (!result.r)
	        {
	            Settings.User.UserID = result.user_id;
	            Settings.User.Token = result.token;
	            Settings.User.Expire = result.expire;
	            Settings.User.Nickname = result.user_name;
	            Settings.User.Email = result.email;

	            UpdateUserInfo();
	        }
	        else
	        {
	            Debug.WriteLine(result.err);
	            switch (result.err)
	            {
	                case "invalid_username":
	                    errorMessage = Resources.Resources.InvalidUsername;
	                    break;
	                case "wrong_password":
	                    errorMessage = Resources.Resources.WrongPassword;
	                    break;
	                case null:
	                case "":
	                case "unknown_error":
	                    errorMessage = Resources.Resources.UnknownError;
	                    break;
	                default:
	                    errorMessage = result.err;
	                    break;
	            }
	        }
	        Dispatcher.Invoke(new Action(() =>
	            {
	                LogOnFailedMessage = errorMessage;
	                if (!string.IsNullOrEmpty(Settings.User.Token))
	                {
	                    CurrentState = State.LoggedOn;
	                }
	                else
	                {
	                    ResetUser();
	                }
	            }));
	    }

	    /// <summary>
		/// 登录
		/// </summary>
		public void LogOn()
		{
        	if (CurrentState != State.LoggedOff) return;
			CurrentState = State.LoggingOn;
		    var username = Settings.User.Username ?? string.Empty;
		    var password = Settings.User.Password ?? string.Empty;
		    ThreadPool.QueueUserWorkItem(state =>
		        {
		            LogOnResult result = null;
		            List<string> errorMessages = new List<string>();
		            if (Regex.Match(username, @"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*$", RegexOptions.IgnoreCase).Success)
		            {
		                var resultEmail = LogOnWithEmail(username, password);
		                if (resultEmail != null)
		                {
		                    if (!resultEmail.r)
		                    {
		                        result = resultEmail;
		                    }
		                    else
		                    {
		                        errorMessages.Add(resultEmail.err);
		                    }
		                }
		            }
		            if (result == null)
		            {
		                var resultUsername = LogOnWithUsername(username, password);
		                if (resultUsername != null)
		                {
		                    if (!resultUsername.r)
		                    {
		                        result = resultUsername;
		                    }
		                    else
		                    {
		                        errorMessages.Add(resultUsername.err);
		                    }
		                }
		            }
		            if (result == null)
		            {
		                result = new LogOnResult {r = true};
		                var invalidUsername = new[] {"invalid_user_name", "invalidate_email", "wrong_email"};
		                errorMessages =
		                    (from message in errorMessages
		                     where !string.IsNullOrEmpty(message)
		                     select invalidUsername.Contains(message) ? "invalid_username" : message).Distinct().ToList();
		                if (errorMessages.Count == 0)
		                {
		                    result.err = "unknown_error";
		                }
		                else
		                {
		                    result.err = errorMessages.FirstOrDefault(message => message != "invalid_username") ??
		                                 errorMessages[0];
		                }
		            }
		            UpdateWhenLogOn(result);
		        });
		}
        /// <summary>
        /// 使用用户名登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>登录结果</returns>
        private LogOnResult LogOnWithUsername(string username, string password)
        {
            Parameters parameters = new Parameters();
            parameters["app_name"] = "radio_desktop_win";
            parameters["version"] = "100";
            parameters["username"] = username;
            parameters["password"] = password;
            string file = new ConnectionBase().Post("http://www.douban.com/j/app/login",
                                                    Encoding.UTF8.GetBytes(parameters.ToString()));
            return JsonHelper.FromJson<LogOnResult>(file);
        }
        /// <summary>
        /// 使用邮箱登录
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <param name="password">密码</param>
        /// <returns>登录结果</returns>
        private LogOnResult LogOnWithEmail(string email, string password)
        {
            Parameters parameters = new Parameters();
            parameters["app_name"] = "radio_desktop_win";
            parameters["version"] = "100";
            parameters["email"] = email;
            parameters["password"] = password;
            string file = new ConnectionBase().Post("http://www.douban.com/j/app/login",
                                                    Encoding.UTF8.GetBytes(parameters.ToString()));
            return JsonHelper.FromJson<LogOnResult>(file);
        }

        /// <summary>
        /// 重置与用户有关的所有附加信息
        /// </summary>
        private void ResetUser()
        {
            CurrentState = State.LoggedOff;
            Settings.User = new User(Settings.User.Username, Settings.User.Password);
        }
        /// <summary>
		/// 注销
		/// </summary>
		public void LogOff()
		{
        	ResetUser();
			LogOnFailedMessage = null;
		}

	    /// <summary>
	    /// 初始化用户状态
	    /// </summary>
	    public void Initialize()
	    {
	        bool loggedOn = false;
	        bool expired = false;
	        if (!string.IsNullOrEmpty(Settings.User.UserID) && !string.IsNullOrEmpty(Settings.User.Token) &&
	            !string.IsNullOrEmpty(Settings.User.Expire))
	        {
	            loggedOn = UpdateUserInfo();
	            if (!loggedOn) expired = true;
	        }
	        Dispatcher.Invoke(new Action(() =>
	            {
	                CurrentState = loggedOn ? State.LoggedOn : State.LoggedOff;
                    if (expired)
                    {
                        Settings.User = new User(Settings.User.Username, Settings.User.Password);
                    }
	            }));
	    }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <returns>是否成功</returns>
	    private bool UpdateUserInfo()
	    {
	        Debug.Assert(!string.IsNullOrEmpty(Settings.User.Token), "!string.IsNullOrEmpty(Settings.User.Token)");
	        Parameters parameters = new Parameters();
	        parameters["app_name"] = "radio_desktop_win";
	        parameters["version"] = "100";
	        parameters["user_id"] = Settings.User.UserID;
	        parameters["token"] = Settings.User.Token;
	        parameters["expire"] = Settings.User.Expire;
	        string file = new ConnectionBase().Post("http://www.douban.com/j/app/radio/user_info",
	                                                Encoding.UTF8.GetBytes(parameters.ToString()));
	        var userInfo = JsonHelper.FromJson<UserInfo>(file);
	        if (userInfo == null || userInfo.r)
	        {
	            return false;
	        }

	        Settings.User.Played = userInfo.played_num;
	        Settings.User.Liked = userInfo.liked_num;
	        Settings.User.Banned = userInfo.banned_num;
	        if (userInfo.pro_status == "S")
	        {
	            if (!Settings.User.IsPro)
	            {
	                Settings.User.IsPro = true;
	                Settings.User.ProRate = ProRate.Kbps64;
	            }
	        }
	        else
	        {
	            Settings.User.IsPro = false;
	            Settings.User.ProRate = ProRate.Kbps64;
	        }

	        return true;
	    }

	    #endregion
	}
}
