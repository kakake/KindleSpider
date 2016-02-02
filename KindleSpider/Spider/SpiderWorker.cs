using System;
using System.Collections;
using System.Net;
using System.IO;
using System.Threading;
using KindleSpider.Common;
namespace KindleSpider.Spider
{
    /// <summary>
    ///蜘蛛爬虫
    /// </summary>
    public class SpiderWorker
    {
        /// <summary>
        /// The URL's that have already been processed.
        /// </summary>
        private Hashtable m_already;

        /// <summary>
        /// URL's that are waiting to be processed.
        /// </summary>
        private Queue m_workload;

        /// <summary>
        /// The first URL to spider. All other URL's must have the
        /// same hostname as this URL. 
        /// </summary>
        private Uri m_base;

        /// <summary>
        /// The directory to save the spider output to.
        /// </summary>
        private string m_outputPath;

        /// <summary>
        /// The form that the spider will report its 
        /// progress to.
        /// </summary>
        private ReportToEvent m_spiderForm;

        /// <summary>
        /// How many URL's has the spider processed.
        /// </summary>
        private int m_urlCount = 0;

        /// <summary>
        /// When did the spider start working
        /// </summary>
        private long m_startTime = 0;

        /// <summary>
        /// Used to keep track of when the spider might be done.
        /// </summary>
        private Done m_done = new Done();

        /// <summary>
        /// Used to tell the spider to quit.
        /// </summary>
        private bool m_quit;

        /// <summary>
        /// The status for each URL that was processed.
        /// </summary>
        enum Status { STATUS_FAILED, STATUS_SUCCESS, STATUS_QUEUED };


        /// <summary>
        /// The constructor
        /// </summary>
        public SpiderWorker()
        {
            reset();
        }

        /// <summary>
        /// Call to reset from a previous run of the spider
        /// </summary>
        public void reset()
        {
            m_already = new Hashtable();
            //?从本地导入索引文件
            m_workload = new Queue();
            m_quit = false;
        }

        /// <summary>
        /// Add the specified URL to the list of URI's to spider.
        /// This is usually only used by the spider, itself, as
        /// new URL's are found.
        /// </summary>
        /// <param name="uri">The URI to add</param>
        public void addURI(Uri uri)
        {
            Monitor.Enter(this);
            if (!m_already.Contains(uri))
            {
                m_already.Add(uri, Status.STATUS_QUEUED);
                //?写入本地索引文件
                m_workload.Enqueue(uri);
            }
            Monitor.Pulse(this);
            Monitor.Exit(this);
        }

        /// <summary>
        /// The URI that is to be spidered
        /// </summary>
        public Uri BaseURI
        {
            get
            {
                return m_base;
            }

            set
            {
                m_base = value;
            }
        }

        /// <summary>
        /// The local directory to save the spidered files to
        /// </summary>
        public string OutputPath
        {
            get
            {
                return m_outputPath;
            }

            set
            {
                m_outputPath = value;
            }
        }

        /// <summary>
        /// The object that the spider reports its
        /// results to.
        /// </summary>
        public ReportToEvent ReportTo
        {
            get
            {
                return m_spiderForm;
            }

            set
            {
                m_spiderForm = value;
            }
        }

        public BookParseEvent bookParse { get; set; }

        /// <summary>
        /// Set to true to request the spider to quit.
        /// </summary>
        public bool Quit
        {
            get
            {
                return m_quit;
            }

            set
            {
                m_quit = value;
            }
        }

        /// <summary>
        /// Used to determine if the spider is done, 
        /// this object is usually only used internally
        /// by the spider.
        /// </summary>
        public Done SpiderDone
        {
            get
            {
                return m_done;
            }

        }

        /// <summary>
        /// Called by the worker threads to obtain a URL to
        /// to process.
        /// </summary>
        /// <returns>The next URL to process.</returns>
        public Uri ObtainWork()
        {
            Monitor.Enter(this);
            while (m_workload.Count < 1)
            {
                Monitor.Wait(this);
            }


            Uri next = (Uri)m_workload.Dequeue();
            if (m_spiderForm != null)
            {
                long etime = (System.DateTime.Now.Ticks - m_startTime) / 10000000L;
                long urls = (etime == 0) ? 0 : m_urlCount / etime;

                //m_spiderForm.SetLastURL(next.ToString());
                //m_spiderForm.SetProcessedCount(""+(m_urlCount++));
                //m_spiderForm.SetElapsedTime( etime/60 + " minutes (" + urls +" urls/sec)" );
                m_spiderForm(next.ToString(), etime / 60 + " minutes (" + urls + " urls/sec)", (m_urlCount++).ToString());
            }

            Monitor.Exit(this);
            return next;
        }

        /// <summary>
        /// Start the spider.
        /// </summary>
        /// <param name="baseURI">The base URI to spider</param>
        /// <param name="threads">The number of threads to use</param>
        public void Start(Uri baseURI, int threads)
        {
            // init the spider
            m_quit = false;

            m_base = baseURI;
            addURI(m_base);
            m_startTime = System.DateTime.Now.Ticks;
            m_done.Reset();

            // startup the threads

            for (int i = 1; i < threads; i++)
            {
                DocumentWorker worker = new DocumentWorker(this);
                worker.Number = i;
                worker.start();
            }

            // now wait to be done

            m_done.WaitBegin();
            m_done.WaitDone();
        }
    }

    /// <summary>
    /// 界面显示爬虫正在爬网也的状态信息
    /// </summary>
    /// <param name="currentUrl">当前正在处理的页面</param>
    /// <param name="elapsed">处理时间</param>
    /// <param name="processedUrlCount">处理个数</param>
    public delegate void ReportToEvent(string currentUrl, string elapsed, string processedUrlCount);
    /// <summary>
    /// 解析为书本
    /// </summary>
    /// <param name="url">需要解析Url</param>
    /// <param name="html">页面的内容</param>
    /// <returns>是否为书本</returns>
    public delegate bool BookParseEvent(string baseurl, string html);
}
