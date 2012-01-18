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
		//public static readonly DependencyProperty NicknameProperty = DependencyProperty.Register("Nickname", typeof(string), typeof(UserAssistant));
		public static readonly DependencyProperty LogOnFailedMessageProperty = DependencyProperty.Register("LogOnFailedMessage", typeof(string), typeof(UserAssistant));

		#endregion

		#region 属性

		/// <summary>
		/// 用户
		/// </summary>
		public Settings Settings { get; internal set; }
		///// <summary>
		///// 昵称
		///// </summary>
		//public string Nickname
		//{
		//    get { return (string)GetValue(NicknameProperty); }
		//    protected set { SetValue(NicknameProperty, value); }
		//}
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
		//        if (HasCaptcha) return "https://www.douban.com/misc/captcha?id=" + _captchaId + "&amp;size=s";
		//        else return null;
		//    }
		//}
		///// <summary>
		///// 是否要求输入验证码
		///// </summary>
		//public bool HasCaptcha
		//{
		//    get { return (bool)GetValue(HasCaptchaProperty); }
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

		///// <summary>
		///// 验证码ID
		///// </summary>
		//private string _captchaId;
		//private string CaptchaId
		//{
		//    get { return _captchaId; }
		//    set
		//    {
		//        _captchaId = value;
		//        SetValue(HasCaptchaProperty, !string.IsNullOrEmpty(_captchaId));
		//    }
		//}
		
		///// <summary>
		///// 注销链接
		///// </summary>
		//private string _logOffLink;

		#endregion

		#region 成员方法

		/// <summary>
		/// 更新
		/// </summary>
		/// <param name="html">HTML文件</param>
		internal void Update(string html)
		{
			//_captchaId = GetCaptchaId(html);          //目前从http://douban.fm登录肯定不需要验证码，所以这里注释掉
			//_logOffLink = GetLogOffLink(html);
			//System.Diagnostics.Debug.WriteLine("注销链接：");
			//System.Diagnostics.Debug.WriteLine(_logOffLink);
			string s = null;
			if (!string.IsNullOrEmpty(html))
			{
				Match match = Regex.Match(html, @"var\s*globalConfig\s*=\s*{\s*uid\s*:\s*'(\d*)'", RegexOptions.IgnoreCase);
				s = match.Groups[1].Value;
				Match match2 = Regex.Match(html, @"id=""fm-user"">(?!{{)(.*)<i></i></a>", RegexOptions.None);
				string nickname = match2.Groups[1].Value;
				Match match3 = Regex.Match(html, @"累积收听.*?(\d+).*?首");
				int played = 0;
				int.TryParse(match3.Groups[1].Value, out played);
				Match match4 = Regex.Match(html, @"加红心.*?(\d+).*?首");
				int liked = 0;
				int.TryParse(match4.Groups[1].Value, out liked);
				Match match5 = Regex.Match(html, @"(\d+).*?首不再播放");
				int banned = 0;
				int.TryParse(match5.Groups[1].Value, out banned);

				Dispatcher.Invoke(new Action(() =>
				{
					/*System.Diagnostics.Debug.WriteLine("**********************************************************************");
					System.Diagnostics.Debug.WriteLine(DateTime.Now + " 以下是本次“登录/注销”返回的结果页面");
					System.Diagnostics.Debug.Indent();
					System.Diagnostics.Debug.WriteLine(html);
					System.Diagnostics.Debug.Unindent();
					System.Diagnostics.Debug.WriteLine("**********************************************************************");
					*/
					if (!string.IsNullOrEmpty(s))
					{
						Settings.User.Nickname = nickname;
						Settings.User.Played = played;
						Settings.User.Liked = liked;
						Settings.User.Banned = banned;
						System.Diagnostics.Debug.WriteLine("已登录");
						CurrentState = State.LoggedOn;
					}
					else
					{
						Settings.User.Nickname = string.Empty;
						System.Diagnostics.Debug.WriteLine("已注销");
						CurrentState = State.LoggedOff;
					}
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
		/// 更新登录结果
		/// </summary>
		internal void UpdateWhenLogOn(Json.LogOnResult result)
		{
			if (result != null && result.r == false && result.user_info != null)
			{
				//_logOffLink = "http://douban.fm/partner/logout?source=radio&ck=" + result.user_info.ck + "&no_login=y";
				Dispatcher.Invoke(new Action(() =>
				{
					//System.Diagnostics.Debug.WriteLine("注销链接：");
					//System.Diagnostics.Debug.WriteLine(_logOffLink);
					/*System.Diagnostics.Debug.WriteLine("**********************************************************************");
					System.Diagnostics.Debug.WriteLine(DateTime.Now + " 以下是本次“登录/注销”返回的结果");
					System.Diagnostics.Debug.Indent();
					System.Diagnostics.Debug.WriteLine(html);
					System.Diagnostics.Debug.Unindent();
					System.Diagnostics.Debug.WriteLine("**********************************************************************");
					*/
					Settings.User.Nickname = result.user_info.name;
					Settings.User.Played = result.user_info.play_record.played;
					Settings.User.Liked = result.user_info.play_record.liked;
					Settings.User.Banned = result.user_info.play_record.banned;
					LogOnFailedMessage = null;
					System.Diagnostics.Debug.WriteLine("已登录");
					CurrentState = State.LoggedOn;
				}));
			}
			else
				Dispatcher.Invoke(new Action(() =>
				{
					if (result != null && !string.IsNullOrEmpty(result.err_msg))
					{
						LogOnFailedMessage = result.err_msg;
					}
					else
					{
						LogOnFailedMessage = "未知错误";
					}
					if (CurrentState == State.LoggingOn) CurrentState = State.LoggedOff;
					else if (CurrentState == State.LoggingOff) CurrentState = State.LoggedOn;
					else if (CurrentState == State.Unknown) CurrentState = State.LoggedOff;
					else CurrentState = State.LoggedOff;
				}));
		}
		///// <summary>
		///// 刷新登录页面
		///// </summary>
		//public void Refresh()
		//{
		//    ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
		//        {
		//            string file = new ConnectionBase().Get("https://www.douban.com/accounts");
		//            Dispatcher.Invoke(new Action(() => { UpdateWhenLogOn(file); }));
		//        }));
		//}
		///// <summary>
		///// 获取验证码ID
		///// </summary>
		///// <param name="html">HTML文件</param>
		///// <returns></returns>
		//string GetCaptchaId(string html)
		//{
		//    if (html == null) return null;
		//    Match match = Regex.Match(html, "<img src=\"http[s]?://www\\.douban\\.com/misc/captcha\\?id=(\\w*)", RegexOptions.IgnoreCase);
		//    return match.Groups[1].Value;
		//}
		/// <summary>
		/// 获取注销链接
		/// </summary>
		/// <param name="html">HTML文件</param>
		/// <returns></returns>
		static string GetLogOffLink(string html)
		{
			if (html == null) return null;
			Match match = Regex.Match(html, "\"(http://.*logout[^\\s]*)\"", RegexOptions.IgnoreCase);
			return match.Groups[1].Value;
		}
		/// <summary>
		/// 登录
		/// </summary>
		public void LogOn()
		{
			if (CurrentState != State.LoggedOff) return;
			CurrentState = State.LoggingOn;
			Parameters parameters = new Parameters();
			parameters.Add("source", "radio");
			parameters.Add("alias", Settings.User.Username);
			parameters.Add("form_password", Settings.User.Password);
			//parameters.Add("captcha-solution", Captcha);
			//parameters.Add("captcha-id", _captchaId);
			if (Settings.AutoLogOnNextTime)
				parameters.Add("remember", "on");
			ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
				{
					string file = new ConnectionBase().Post("http://douban.fm/j/login", Encoding.UTF8.GetBytes(parameters.ToString()));
					System.Diagnostics.Debug.WriteLine("登录结果：");
					System.Diagnostics.Debug.WriteLine(file);
					var result = Json.LogOnResult.FromJson(file);
					Dispatcher.Invoke(new Action(() => { UpdateWhenLogOn(result); }));
				}));
		}
		/// <summary>
		/// 注销
		/// </summary>
		public void LogOff()
		{
			if (CurrentState != State.LoggedOn) return;
			CurrentState = State.LoggingOff;
			ConnectionBase.cc = new System.Net.CookieContainer(1000, 1000, 100000);
			//_logOffLink = string.Empty;
			Settings.User.Nickname = string.Empty;
			Settings.User.Played = 0;
			Settings.User.Liked = 0;
			Settings.User.Banned = 0;
			CurrentState = State.LoggedOff;
		}

		/// <summary>
		/// 强制注销，非后台
		/// </summary>
		internal void ForceLogOff()
		{
			LogOff();
		}
		#endregion
	}
}
