using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace KindleSpider.Common
{
    /// <summary>
    /// NSoup封装类
    /// </summary>
    public class NSoupHelper
    {
        public static NSoup.Nodes.Document GetNSoupDoc(string url, string html_charset, out string html)
        {
            try
            {
                WebResponse response = null;
                Stream stream = null;
                StreamReader reader = null;

                Uri m_uri = new Uri(url);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_uri);

                response = request.GetResponse();
                stream = response.GetResponseStream();

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
                charset = string.IsNullOrEmpty(charset) ? html_charset : charset;
                reader = new StreamReader(stream, System.Text.Encoding.GetEncoding(charset));

                while ((line = reader.ReadLine()) != null)
                {
                    buffer += line + "\r\n";
                }
                html = buffer;
                NSoup.Nodes.Document doc = GetNSoupDoc(html);
                return doc;
            }
            catch (Exception e)
            {
                html = "";
                System.Console.WriteLine("Invalid URI:" + url + " Error:" + e.Message);
                return null;
            }
        }
        
        public static NSoup.Nodes.Document GetNSoupDoc(string html)
        {
            NSoup.Nodes.Document doc = NSoup.NSoupClient.Parse(html);
            return doc;
        }

    }
}
