using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using KindleSpider.Common;
using System.IO;
using System.Net;

namespace KindleSpider.Spider
{
    public class SpiderBook
    {
        private Hashtable m_already;
        private int m_urlCount = 0;
        private long m_startTime = 0;
        enum Status { STATUS_FAILED, STATUS_SUCCESS, STATUS_QUEUED };
        // 缺省文档的名字
        public const string IndexFile = "index.html";

        private string m_outputPath;
        public string OutputPath
        {
            get { return m_outputPath; }
            set { m_outputPath = value; }
        }

        private ReportToEvent m_Report;
        public ReportToEvent ReportTo
        {
            get { return m_Report; }
            set { m_Report = value; }
        }

        public BookParseEvent bookParse { get; set; }

        private Uri m_base;
        private MultiThreadingWorker thWork = null;

        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="baseurl"></param>
        public void StartSpider(string baseurl)
        {
            m_already = new Hashtable();
            m_base = new Uri(baseurl);
            m_startTime = System.DateTime.Now.Ticks;

            thWork = new MultiThreadingWorker();
            thWork.threadCount = 20;
            thWork.workContent = new WorkContent(GetPage);
            if (!m_already.Contains(m_base))
            {
                m_already.Add(m_base, Status.STATUS_QUEUED);
                thWork.AddWork(m_base);
            }
            
            thWork.Start();
        }
        /// <summary>
        /// 停止
        /// </summary>
        public void StopSpider()
        {
            thWork.Quit = true;
        }

        private void GetPage(object url)
        {
            WebResponse response = null;
            Stream stream = null;
            StreamReader reader = null;

            try
            {
                Uri m_uri = new Uri(url.ToString());
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_uri);

                response = request.GetResponse();
                stream = response.GetResponseStream();

                if (!response.ContentType.ToLower().StartsWith("text/"))
                {
                    SaveBinaryFile(response);
                    return;
                }

                string buffer = "", line;

                #region 获取charset
                String charset = null;
                //如果发现content-type头
                String ctype = response.Headers["content-type"];
                if (ctype != null)
                {
                    int ind = ctype.IndexOf("charset=");
                    if (ind != -1)
                    {
                        charset = ctype.Substring(ind + 8);
                    }
                }
                #endregion
                charset = string.IsNullOrEmpty(charset) ? "gb2312" : charset;
                reader = new StreamReader(stream, System.Text.Encoding.GetEncoding(charset));

                while ((line = reader.ReadLine()) != null)
                {
                    buffer += line + "\r\n";
                }
                SaveTextFile(buffer, m_uri);

                if (buffer != null)
                {
                    if (m_Report != null)
                    {
                        long etime = (System.DateTime.Now.Ticks - m_startTime) / 10000000L;
                        long urls = (etime == 0) ? 0 : m_urlCount / etime;
                        m_Report(m_uri.ToString(), etime / 60 + " minutes (" + urls + " urls/sec)", (m_urlCount++).ToString());
                    }
                    //解析网页转换为书本
                    if (bookParse != null)
                        bookParse(m_uri.ToString(), buffer);

                    //处理页面中的URL
                    ProcessPage(buffer,m_uri);
                }
            }
            catch (WebException e)
            {
                System.Console.WriteLine("下载失败，错误：" + e);
            }
            catch (IOException e)
            {
                System.Console.WriteLine("下载失败，错误：" + e);
            }
            finally
            {
                if (reader != null) reader.Close();
                if (stream != null) stream.Close();
                if (response != null) response.Close();
            }
        }

        //处理URL
        private void ProcessLink(string link, Uri m_uri)
        {
            Uri url;

            // fully expand this URL if it was a relative link
            try
            {
                url = new Uri(m_uri, link, false);
            }
            catch (UriFormatException e)
            {
                System.Console.WriteLine("Invalid URI:" + link + " Error:" + e.Message);
                return;
            }

            if (!url.Scheme.ToLower().Equals("http") &&
                !url.Scheme.ToLower().Equals("https"))
                return;

            if (!url.Host.ToLower().Equals(m_uri.Host.ToLower()))
                return;

            if (!m_already.Contains(url))
            {
                m_already.Add(url, Status.STATUS_QUEUED);
                thWork.AddWork(url);
            }
        }

        //查找页面中所有URL
        private void ProcessPage(string page, Uri m_uri)
        {
            try
            {
                NSoup.Nodes.Document doc = NSoup.NSoupClient.Parse(page);
                var query = doc.Select("link[href],script[src],a[href],img[src]");
                foreach (var q in query)
                {
                    string url = q.Attr("href");
                    if (string.IsNullOrEmpty(url) == false)
                        ProcessLink(url,m_uri);
                    url = q.Attr("src");
                    if (string.IsNullOrEmpty(url) == false)
                        ProcessLink(url,m_uri);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(" Error:" + e.Message);
                return;
            }
        }

        // 输入参数是一个URI名称，例如/images/blank.gif.
        // 把它转换成本地文件名称。如果尚未创建相应的目录
        // 结构，则创建之
        private string convertFilename(Uri uri)
        {
            string result = m_outputPath;
            int index1;
            int index2;

            // add ending slash if needed
            if (result[result.Length - 1] != '\\')
                result = result + "\\";

            // strip the query if needed

            String path = uri.PathAndQuery;
            int queryIndex = path.IndexOf("?");
            if (queryIndex != -1)
                path = path.Substring(0, queryIndex);

            // see if an ending / is missing from a directory only

            int lastSlash = path.LastIndexOf('/');
            int lastDot = path.LastIndexOf('.');

            if (path[path.Length - 1] != '/')
            {
                if (lastSlash > lastDot)
                    path += "/" + IndexFile;
            }

            // determine actual filename		
            lastSlash = path.LastIndexOf('/');

            string filename = "";
            if (lastSlash != -1)
            {
                filename = path.Substring(1 + lastSlash);
                path = path.Substring(0, 1 + lastSlash);
                if (filename.Equals(""))
                    filename = IndexFile;
            }

            // 必要时创建目录结构			
            index1 = 1;
            do
            {
                index2 = path.IndexOf('/', index1);
                if (index2 != -1)
                {
                    String dirpart = path.Substring(index1, index2 - index1);
                    result += dirpart;
                    result += "\\";


                    Directory.CreateDirectory(result);

                    index1 = index2 + 1;
                }
            } while (index2 != -1);

            // attach name
            result += filename;

            return result;
        }
        // 将二进制文件保存到磁盘
        private void SaveBinaryFile(WebResponse response)
        {
            byte[] buffer = new byte[1024];

            if (m_outputPath == null)
                return;

            string filename = convertFilename(response.ResponseUri);
            Stream outStream = File.Create(filename);
            Stream inStream = response.GetResponseStream();

            int l;
            do
            {
                l = inStream.Read(buffer, 0, buffer.Length);
                if (l > 0)
                    outStream.Write(buffer, 0, l);
            }
            while (l > 0);

            outStream.Close();
            inStream.Close();
        }
        // 保存文本文件
        private void SaveTextFile(string buffer, Uri m_uri)
        {
            if (m_outputPath == null)
                return;

            string filename = convertFilename(m_uri);
            StreamWriter outStream = new StreamWriter(filename, false, System.Text.Encoding.Default);
            outStream.Write(buffer);
            outStream.Close();
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
