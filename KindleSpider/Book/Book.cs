using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Xml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using KindleSpider.Common;

namespace KindleSpider.BookSP
{
    public class Book
    {
        public string rule { get; set; }
        public string baseurl { get; set; }

        public string title { get; set; }
        public string auther { get; set; }
        public string cover { get; set; }
        public string introduction { get; set; }
        public List<BookCatalog> catalogs { get; set; }

        public Book()
        {
        }

        public Book(string xmlfile)
        {
            LoadBook(xmlfile);
        }
        /// <summary>
        /// 导入XML
        /// </summary>
        /// <param name="filename"></param>
        public void LoadBook(string filename)
        {
            XmlHandle xmlhandle = new XmlHandle(filename);
            rule = xmlhandle.GetValue("book", "rule");
            baseurl = xmlhandle.GetValue("book", "baseurl");
            title = xmlhandle.GetValue("book/title");
            auther = xmlhandle.GetValue("book/auther");
            cover = xmlhandle.GetValue("book/cover");
            introduction = xmlhandle.GetValue("book/introduction");
            int count = xmlhandle.GetCount("book/catalogs/catalog");
            catalogs = new List<BookCatalog>();
            for (int i = 1; i <= count; i++)
            {
                BookCatalog bc = new BookCatalog();
                bc.index = Convert.ToInt32(xmlhandle.GetValue("book/catalogs/catalog[" + i + "]", "index"));
                bc.url = xmlhandle.GetValue("book/catalogs/catalog[" + i + "]", "url");
                bc.text = xmlhandle.GetValue("book/catalogs/catalog[" + i + "]");
                bc.page = new BookPage();
                bc.page.index = bc.index;
                bc.page.text = xmlhandle.GetValue("book/content/page[" + i + "]");

                catalogs.Add(bc);
            }
        }
        /// <summary>
        /// 保存XML
        /// </summary>
        /// <param name="filename"></param>
        public void SaveBook(string filename)
        {
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Directory.Exists == false)
                Directory.CreateDirectory(fileinfo.DirectoryName);

            string strxml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                                            <root>
                                            <book rule="""" baseurl="""">
                                              <title></title>
                                              <auther></auther>
                                              <cover></cover>
                                              <introduction></introduction>
                                              <catalogs>
                                              </catalogs>
                                              <content>
                                              </content>
                                            </book>
                                            </root>";
            XmlHandle xmlhandle = new XmlHandle(filename, strxml);
            xmlhandle.SetValue("book", "rule", rule);
            xmlhandle.SetValue("book", "baseurl", baseurl);
            xmlhandle.SetValue("book/title", title);
            xmlhandle.SetValue("book/auther", auther);
            xmlhandle.SetValue("book/cover", cover);
            xmlhandle.SetValue("book/introduction", introduction);
            for (int i = 0; i < catalogs.Count; i++)
            {
                XmlElement node = xmlhandle.AddNode("book/catalogs", "catalog");
                node.SetAttribute("url", catalogs[i].url);
                node.InnerText = catalogs[i].text;
                if (catalogs[i].page != null)
                {
                    node.SetAttribute("index", catalogs[i].page.index.ToString());
                    XmlElement node1 = xmlhandle.AddNode("book/content", "page");
                    node1.SetAttribute("index", catalogs[i].page.index.ToString());
                    node1.InnerText = catalogs[i].page.text;
                }
            }

            xmlhandle.SaveConfig();
        }
        /// <summary>
        /// 保存为pdf
        /// </summary>
        /// <param name="filename"></param>
        public void SavePdf(string filename)
        {
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Directory.Exists == false)
                Directory.CreateDirectory(fileinfo.DirectoryName);

            Document doc = new Document(PageSize.A5, 10, 10, 10, 10);
            PdfWriter.GetInstance(doc, new FileStream(filename, FileMode.Create));
            doc.Open();

            //指定字体库，并创建字体
            BaseFont baseFont = BaseFont.CreateFont(
                "C:\\WINDOWS\\FONTS\\SIMYOU.TTF",
                BaseFont.IDENTITY_H,
                BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font font1 = new iTextSharp.text.Font(baseFont, 18);
            iTextSharp.text.Font font2 = new iTextSharp.text.Font(baseFont, 20);

            Chapter chapter1 = new Chapter(title, 1);
            chapter1.Add(new Paragraph(title, font2));
            chapter1.Add(new Paragraph(auther, font1));
            chapter1.Add(new Paragraph(introduction, font1));
            for (int i = 0; i < catalogs.Count; i++)
            {
                Section section1 = chapter1.AddSection(catalogs[i].text);
                section1.Add(new Paragraph(catalogs[i].page.text, font1));
                section1.TriggerNewPage = true;
                section1.BookmarkOpen = false;
            }

            chapter1.BookmarkOpen = false;
            doc.Add(chapter1);
            doc.Close();
        }
        /// <summary>
        /// 保存为txt
        /// </summary>
        /// <param name="filename"></param>
        public void SaveTxt(string filename)
        {
            FileInfo fileinfo = new FileInfo(filename);
            if (fileinfo.Directory.Exists == false)
                Directory.CreateDirectory(fileinfo.DirectoryName);
            //实例化StreamWriter对象
            StreamWriter sw = new StreamWriter(filename, true,Encoding.UTF8);
            //向创建的文件中写入内容

            sw.WriteLine("书籍名称：" + title+"\t"+"作者："+auther);
            sw.WriteLine("来源地址：" + baseurl);
            sw.WriteLine("内容介绍：" + introduction);
            sw.WriteLine("目录：");
            for (int i = 0; i < catalogs.Count; i++)
            {
                sw.WriteLine(catalogs[i].text);
            }

            for (int i = 0; i < catalogs.Count; i++)
            {
                sw.WriteLine(catalogs[i].text);
                if (catalogs[i].page != null)
                {
                    string[] ss= catalogs[i].page.text.Split(new string[] { "\n" },StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in ss)
                    {
                        sw.WriteLine(s);
                    }
                }
            }

            //关闭当前文件写入流
            sw.Close();
        }
        /// <summary>
        /// 发送kindle的EMail
        /// </summary>
        /// <param name="toEmail">kindle的EMail地址</param>
        /// <param name="type">发送的文件 0:pdf；1:txt</param>
        /// <param name="filename">文件名</param>
        public void SendMail(string toEmail, int type, string filename)
        {
            //确定smtp服务器地址。实例化一个Smtp客户端
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient("smtp.qq.com");
            //生成一个发送地址
            string strFrom = "@qq.com";

            //构造一个发件人地址对象
            MailAddress from = new MailAddress(strFrom, "kakake", Encoding.UTF8);
            //构造一个收件人地址对象
            MailAddress to = new MailAddress(toEmail, "kindle", Encoding.UTF8);

            //构造一个Email的Message对象
            MailMessage message = new MailMessage(from, to);

            if (type == 0)
                SaveTxt(filename);
            else if (type == 1)
                SavePdf(filename);

            //得到文件名
            string fileName = filename;
            //判断文件是否存在
            if (File.Exists(fileName))
            {
                //构造一个附件对象
                Attachment attach = new Attachment(fileName);
                //得到文件的信息
                ContentDisposition disposition = attach.ContentDisposition;
                disposition.CreationDate = System.IO.File.GetCreationTime(fileName);
                disposition.ModificationDate = System.IO.File.GetLastWriteTime(fileName);
                disposition.ReadDate = System.IO.File.GetLastAccessTime(fileName);
                //向邮件添加附件
                message.Attachments.Add(attach);
            }
            else
            {
                throw new Exception("[" + fileName + "]文件没找到！");
            }

            //添加邮件主题和内容
            message.Subject = "书籍发送到kindle";
            message.SubjectEncoding = Encoding.UTF8;
            message.Body = "";
            message.BodyEncoding = Encoding.UTF8;

            //设置邮件的信息
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.IsBodyHtml = false;

            //如果服务器支持安全连接，则将安全连接设为true。
            //gmail支持，163不支持，如果是gmail则一定要将其设为true
            client.EnableSsl = true;

            //设置用户名和密码。
            //string userState = message.Subject;
            client.UseDefaultCredentials = false;
            string username = "";
            string passwd = "";
            //用户登陆信息
            NetworkCredential myCredentials = new NetworkCredential(username, passwd);
            client.Credentials = myCredentials;
            //发送邮件
            client.Send(message);
        }
    }

    public class BookCatalog
    {
        public string baseurl { get; set; }
        public BookRule bookrule { get; set; }

        public int index { get; set; }
        public string url { get; set; }
        public string text { get; set; }

        public BookPage page { get; set; }
    }

    public class BookPage
    {
        public int index { get; set; }
        public string text { get; set; }
    }
}
