using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using KindleSpider.Spider;

namespace KindleSpider.Common
{
    /// <summary>
    /// 多线程执行
    /// </summary>
    public class MultiThreadingWorker
    {
        /// <summary>
        /// 确保线程全部完成
        /// </summary>
        private Done m_done = null;
        private long m_startTime = 0;
        private Queue m_workload = null;

        public MultiThreadingWorker()
        {
            m_done = new Done();
            m_workload = new Queue();
        }

        /// <summary>
        /// 线程工作的内容委托
        /// </summary>
        public WorkContent workContent { get; set; }
        /// <summary>
        /// 线程数目
        /// </summary>
        public int threadCount { get; set; }

        private bool m_quit;
        public bool Quit
        {
            get { return m_quit; }
            set { m_quit = value; }
        }
        /// <summary>
        /// 增加工作的对象到队列
        /// </summary>
        /// <param name="work"></param>
        public void AddWork(Object work)
        {
            Monitor.Enter(this);
            m_workload.Enqueue(work);
            Monitor.Pulse(this);
            Monitor.Exit(this);
        }
        /// <summary>
        /// 从队列中取出对象
        /// </summary>
        /// <returns></returns>
        private Object ObtainWork()
        {
            Monitor.Enter(this);
            while (m_workload.Count < 1)
            {
                Monitor.Wait(this);
            }
            Object next = m_workload.Dequeue();

            Monitor.Exit(this);
            return next;
        }

        /// <summary>
        /// 开始执行
        /// </summary>
        public void Start()
        {
            m_startTime = System.DateTime.Now.Ticks;
            m_done.Reset();
            m_quit = false;

            for (int i = 1; i <= threadCount; i++)
            {
                ThreadStart ts = new ThreadStart(this.Process);
                Thread m_thread = new Thread(ts);
                m_thread.Start();
            }

            m_done.WaitBegin();
            m_done.WaitDone();
        }

        /// <summary>
        /// 停止执行
        /// </summary>
        public void Stop()
        {
            m_quit = true;
        }

        private void Process()
        {
            while (m_quit==false)
            {
                Object workobj = ObtainWork();
                m_done.WorkerBegin();
                try
                {
                    workContent(workobj);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(" Error:" + e.Message);
                }
                finally
                {
                    m_done.WorkerEnd();
                }
            }
        }
    }

    /// <summary>
    /// 执行内容适用委托
    /// </summary>
    /// <param name="work"></param>
    public delegate void WorkContent(Object work);
}
