/*
 * Author : K.F.Storm
 * Email : yk000123 at sina.com
 * Website : http://www.kfstorm.com
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DoubanFM.Core
{
	/// <summary>
	/// 更新工作
	/// </summary>
	public class UpdateWork : IDisposable
	{
		/// <summary>
		/// 工作线程
		/// </summary>
		public Thread WorkThread { get; set; }
		/// <summary>
		/// 是否正在工作
		/// </summary>
		public bool Working
		{
			get
			{
				return WorkThread != null && (WorkThread.ThreadState & (ThreadState.Unstarted | ThreadState.Stopped)) == 0;
			}
		}

		public UpdateWork(ThreadStart start)
		{
			WorkThread = new Thread(start);
			WorkThread.IsBackground = true;
		}

		/// <summary>
		/// 启动
		/// </summary>
		public void Start()
		{
			if ((WorkThread.ThreadState & ThreadState.Unstarted) != 0)
				WorkThread.Start();
		}

		/// <summary>
		/// 中止
		/// </summary>
		public void Abort()
		{
			if (Working)
			{
				WorkThread.Abort();
				WorkThread = null;
			}
		}

		public void Dispose()
		{
			if (Working) Abort();
		}
	}

}
