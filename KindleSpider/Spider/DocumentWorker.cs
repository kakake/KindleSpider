using System;
using System.Net;
using System.IO;
using System.Threading;
//该源码下 载自www.aspx1.com(ａｓｐｘ１．ｃｏｍ)
namespace KindleSpider.Spider
{
	/// <summary>
	/// Perform all of the work of a single thread for the spider.
	/// This involves waiting for a URL to becomve available, download
	/// and then processing the page.
	/// 
	/// </summary>
	// 完成必须由单个工作线程执行的操作，包括
	// 等待可用的URL，下载和处理页面
	public class DocumentWorker
	{
		/// <summary>
		/// The base URI that is to be spidered.
		/// </summary>
		// 要扫描的基础URI
		private Uri m_uri;

		/// <summary>
		/// The spider that this thread "works for"
		/// </summary>
		// 
		private SpiderWorker m_spider;

		/// <summary>
		/// The thread that is being used.
		/// </summary>
		private Thread m_thread;

		/// <summary>
		/// The thread number, used to identify this worker.
		/// </summary>
		// 线程编号，用来标识当前的工作线程
		private int m_number;
		

		/// <summary>
		/// The name for default documents.
		/// </summary>
		// 缺省文档的名字
		public const string IndexFile = "index.html";

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="spider">The spider that owns this worker.</param>
		// 构造函数，参数表示拥有当前工作线程的蜘蛛程序
		public DocumentWorker(SpiderWorker spider)
		{
			m_spider = spider;
		}

		/// <summary>
		/// This method will take a URI name, such ash /images/blank.gif
		/// and convert it into the name of a file for local storage.
		/// If the directory structure to hold this file does not exist, it
		/// will be created by this method.
		/// </summary>
		/// <param name="uri">The URI of the file about to be stored</param>
		/// <returns></returns>
		// 输入参数是一个URI名称，例如/images/blank.gif.
		// 把它转换成本地文件名称。如果尚未创建相应的目录
		// 结构，则创建之
		private string convertFilename(Uri uri)
		{
			string result = m_spider.OutputPath;
			int index1;
			int index2;			

			// add ending slash if needed
			if( result[result.Length-1]!='\\' )
				result = result+"\\";

			// strip the query if needed

			String path = uri.PathAndQuery;
			int queryIndex = path.IndexOf("?");
			if( queryIndex!=-1 )
				path = path.Substring(0,queryIndex);

			// see if an ending / is missing from a directory only
			
			int lastSlash = path.LastIndexOf('/');
			int lastDot = path.LastIndexOf('.');

			if( path[path.Length-1]!='/' )
			{
				if(lastSlash>lastDot)
					path+="/"+IndexFile;
			}

			// determine actual filename		
			lastSlash = path.LastIndexOf('/');

			string filename = "";
			if(lastSlash!=-1)
			{
				filename=path.Substring(1+lastSlash);
				path = path.Substring(0,1+lastSlash);
				if(filename.Equals("") )
					filename=IndexFile;
			}

			// 必要时创建目录结构			
			index1 = 1;
			do
			{
				index2 = path.IndexOf('/',index1);
				if(index2!=-1)
				{
					String dirpart = path.Substring(index1,index2-index1);
					result+=dirpart;
					result+="\\";
				
				
					Directory.CreateDirectory(result);

					index1 = index2+1;					
				}
			} while(index2!=-1);			

			// attach name
			result+=filename;

			return result;
		}

		/// <summary>
		/// Save a binary file to disk.
		/// </summary>
		/// <param name="response">The response used to save the file</param>
		// 将二进制文件保存到磁盘
		private void SaveBinaryFile(WebResponse response)
		{
			byte []buffer = new byte[1024];

			if( m_spider.OutputPath==null )
				return;

			string filename = convertFilename( response.ResponseUri );
			Stream outStream = File.Create( filename );
			Stream inStream = response.GetResponseStream();	
			
			int l;
			do
			{
				l = inStream.Read(buffer,0,buffer.Length);
				if(l>0)
					outStream.Write(buffer,0,l);
			}
			while(l>0);
			
			outStream.Close();
			inStream.Close();

		}

		/// <summary>
		/// Save a text file.
		/// </summary>
		/// <param name="buffer">The text to save</param>
		// 保存文本文件
        private void SaveTextFile(string buffer)
        {
            if (m_spider.OutputPath == null)
                return;

            string filename = convertFilename(m_uri);
            StreamWriter outStream = new StreamWriter(filename, false, System.Text.Encoding.Default);
            outStream.Write(buffer);
            outStream.Close();
        }

		/// <summary>
		/// Download a page
		/// </summary>
		/// <returns>The data downloaded from the page</returns>
		// 下载一个页面
		private string GetPage()
		{
			WebResponse response = null;
			Stream stream = null;
			StreamReader reader = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_uri);

                response = request.GetResponse();
                stream = response.GetResponseStream();

                if (!response.ContentType.ToLower().StartsWith("text/"))
                {
                    SaveBinaryFile(response);
                    return null;
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
                SaveTextFile(buffer);

                //解析网页转换为书本
                if (m_spider.bookParse != null)
                    m_spider.bookParse(m_uri.ToString(), buffer);
                return buffer;
            }
            catch (WebException e)
            {
                System.Console.WriteLine("下载失败，错误：" + e);
                return null;
            }
            catch (IOException e)
            {
                System.Console.WriteLine("下载失败，错误：" + e);
                return null;
            }
			finally
			{
				if( reader!=null ) reader.Close();
				if( stream!=null ) stream.Close();
				if( response!=null ) response.Close();
			}
		}

		/// <summary>
		/// Process each link encountered. The link will be recorded
		/// for later spidering if it is an http or https docuent, 
		/// has not been visited before(determined by spider class),
		/// and is in the same host as the original base URL.
		/// </summary>
		/// <param name="link">The URL to process</param>
		private void ProcessLink(string link)
		{
			Uri url;

			// fully expand this URL if it was a relative link
			try
			{
				url = new Uri(m_uri,link,false);
			}
			catch(UriFormatException e)
			{
				System.Console.WriteLine( "Invalid URI:" + link +" Error:" + e.Message);
				return;
			}

			if(!url.Scheme.ToLower().Equals("http") &&
				!url.Scheme.ToLower().Equals("https") )
				return;

			// comment out this line if you would like to spider
			// the whole Internet (yeah right, but it will try)
			if( !url.Host.ToLower().Equals( m_uri.Host.ToLower() ) )
				return;

			//System.Console.WriteLine( "Queue:"+url );
			m_spider.addURI( url );



		}

		/// <summary>
		/// Process a URL
		/// </summary>
		/// <param name="page">the URL to process</param>
		private void ProcessPage(string page)
		{
            try
            {
                //ParseHTML parse = new ParseHTML();
                //parse.Source = page;

                //while (!parse.Eof())
                //{
                //    char ch = parse.Parse();
                //    if (ch == 0)
                //    {
                //        Attribute a = parse.GetTag()["HREF"];
                //        if (a != null)
                //            ProcessLink(a.Value);

                //        a = parse.GetTag()["SRC"];
                //        if (a != null)
                //            ProcessLink(a.Value);
                //    }
                //}

                NSoup.Nodes.Document doc = NSoup.NSoupClient.Parse(page);
                var query = doc.Select("link[href],script[src],a[href],img[src]");
                foreach (var q in query)
                {
                    string url = q.Attr("href");
                    if (string.IsNullOrEmpty(url) == false)
                        ProcessLink(url);
                    url = q.Attr("src");
                    if (string.IsNullOrEmpty(url) == false)
                        ProcessLink(url);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(" Error:" + e.Message);
                return;
            }
		}


		/// <summary>
		/// This method is the main loop for the spider threads.
		/// This method will wait for URL's to become available, 
		/// and then process them. 
		/// </summary>
        public void Process()
        {
            while (!m_spider.Quit)
            {
                m_uri = m_spider.ObtainWork();
                try
                {

                    m_spider.SpiderDone.WorkerBegin();
                    System.Console.WriteLine("Download(" + this.Number + "):" + m_uri);


                    string page = GetPage();
                    if (page != null)
                        ProcessPage(page);
                    m_spider.SpiderDone.WorkerEnd();

                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Invalid URI:" + m_uri.ToString() + " Error:" + e.Message);
                }
            }
        }

		/// <summary>
		/// Start the thread.
		/// </summary>
		public void start()
		{
			ThreadStart ts = new ThreadStart( this.Process );
			m_thread = new Thread(ts);
			m_thread.Start();
		}

		/// <summary>
		/// The thread number. Used only to identify this thread.
		/// </summary>
		public int Number 
		{
			get
			{
				return m_number;
			}

			set
			{
				m_number = value;
			}
		
		}
	}
}

