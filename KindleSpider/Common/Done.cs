using System;
using System.Threading;

namespace KindleSpider.Common
{
	public class Done 
	{
		private int m_activeThreads = 0;
		private bool m_started = false;


		public void WaitDone()
		{
			Monitor.Enter(this);
			while ( m_activeThreads>0 ) 
			{
				Monitor.Wait(this);
			}
			Monitor.Exit(this);
		}

		public void WaitBegin()
		{
			Monitor.Enter(this);
			while ( !m_started ) 
			{
				Monitor.Wait(this);
			}
			Monitor.Exit(this);
		}


		/// <summary>
		/// ִ�п�ʼ
		/// </summary>
		public void WorkerBegin()
		{
			Monitor.Enter(this);
			m_activeThreads++;
			m_started = true;
			Monitor.Pulse(this);
			Monitor.Exit(this);
		}

		/// <summary>
		/// ִ�����
		/// </summary>
		public void WorkerEnd()
		{
			Monitor.Enter(this);
			m_activeThreads--;
			Monitor.Pulse(this);
			Monitor.Exit(this);
		}

		/// <summary>
		/// ���³�ʼ��
		/// </summary>
		public void Reset()
		{
			Monitor.Enter(this);
			m_activeThreads = 0;
			Monitor.Exit(this);
		}
	}
}
