using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using KindleSpider.Spider;
using KindleSpider.BookSP;
using KindleSpider.Common;

namespace KindleSpider
{
    public class BookParse
    {
        /// <summary>
        /// 解析预览
        /// </summary>
        /// <param name="url"></param>
        /// <param name="rule"></param>
        /// <returns>返回书对象</returns>
        public static Book ParsePreviewBook(string url, BookRule rule)
        {
            try
            {
                Uri uri = new Uri(url);
                Book book = new Book();
                book.rule = rule.name;
                book.baseurl = url;

                ParseCatalog(uri, book, rule);
                ParseContent(uri, book, rule, 5);//预览5章
                return book;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public static Book ParseSingleBook(string url)
        {
            //Rule rule = new Rule();
            //rule.name = "劝学网";
            //rule.title_rule = ".title";
            //rule.auther_rule = ".auther";
            //rule.cover_rule = "";
            //rule.introduction_rule = ".index_center_td p";
            //rule.catalog_rule = ".index_left_td a,.index_center_td a";
            //rule.content_rule = "div.title,div.item,p";
            try
            {
                Uri uri = new Uri(url);
                BookRule rule = RuleManager.GetBookRule("", "http://" + uri.Host);

                Book book = new Book();
                book.rule = rule.name;
                book.baseurl = url;

                ParseCatalog(uri, book, rule);
                ParseContent(uri, book, rule);
                return book;
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        /// <summary>
        /// 解析整个站点
        /// </summary>
        /// <param name="url"></param>
        public static void ParseWebsite(string url,string savepath,ReportToEvent report)
        {
            try
            {
                SpiderBook spbook = new SpiderBook();
                spbook.OutputPath=savepath;
                spbook.ReportTo = report;
                spbook.bookParse = new BookParseEvent(ParseHtml);
                spbook.StartSpider(url);

            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private static bool ParseHtml(string baseurl,string html)
        {
            Uri baseuri = new Uri(baseurl);
            BookRule rule = RuleManager.GetBookRule("", "http://" + baseuri.Host);

            Book book = new Book();
            book.rule = rule.name;
            book.baseurl = baseuri.ToString();

            ParseCatalog(baseuri,html, book, rule);
            ParseContent(baseuri, book, rule);

            //判断不是一本书就返回
            if (string.IsNullOrEmpty(book.title) || string.IsNullOrEmpty(book.auther) || book.catalogs == null)
            {
                return false;
            }
            book.SaveBook("bookxml/" + book.title + ".xml");
            return true;
        }

        private static void ParseCatalog(Uri uri, Book book, BookRule rule)
        {
            ParseCatalog(uri, null, book, rule);
        }

        private static void ParseCatalog(Uri uri,string html, Book book, BookRule rule)
        {
            string buffer;
            NSoup.Nodes.Document doc = html == null ? NSoupHelper.GetNSoupDoc(uri.ToString(), rule.charset, out buffer) : NSoupHelper.GetNSoupDoc(html);
            if (doc == null) return;

            if (rule.title_rule != "")
            {
                book.title = doc.Select(rule.title_rule).Text;
            }
            if (rule.auther_rule != "")
            {
                book.auther = doc.Select(rule.auther_rule).Text;
            }
            if (rule.cover_rule != "")
            {
                book.cover = doc.Select(rule.cover_rule).Text;
            }
            if (rule.introduction_rule != "")
            {
                book.introduction = doc.Select(rule.introduction_rule).Text;
            }
            if (rule.catalog_rule != "")
            {
                var catalog = doc.Select(rule.catalog_rule);
                if (catalog.Count > 0)
                {
                    book.catalogs = new List<BookCatalog>();
                    for (int i = 0; i < catalog.Count; i++)
                    {
                        BookCatalog bc = new BookCatalog();
                        bc.index = i;
                        bc.url = catalog[i].Attr("href");
                        bc.text = catalog[i].Text();
                        bc.baseurl = uri.ToString();
                        bc.bookrule = rule;

                        book.catalogs.Add(bc);
                    }
                }
            }
        }

        private static void ParseContent(Uri uri, Book book, BookRule rule)
        {
            ParseContent(uri, book, rule, 0);
        }

        private static void ParseContent(Uri uri, Book book, BookRule rule,int pagenum)
        {
            if (rule.content_rule != "" && book.catalogs!=null)
            {
                MultiThreadingWorker thWork = new MultiThreadingWorker();
                thWork.threadCount = 20;
                thWork.workContent = new WorkContent(ParsePage);
                for (int i = 0; i < book.catalogs.Count; i++)
                {
                    if (pagenum > 0 && pagenum < i + 1) break;//预览

                    thWork.AddWork(book.catalogs[i]);
                }
                thWork.Start();
            }
        }

        private static void ParsePage(Object _catalog)
        {
            BookCatalog catalog = (BookCatalog)_catalog;
            Uri c_uri = new Uri(new Uri(catalog.baseurl), catalog.url, false);
            string buffer;
            NSoup.Nodes.Document doc = NSoupHelper.GetNSoupDoc(c_uri.ToString(), catalog.bookrule.charset, out buffer);
            if (doc == null) return;
            var content = doc.Select(catalog.bookrule.content_rule);

            catalog.page = new BookPage();
            catalog.page.index = catalog.index;
            catalog.page.text = HtmlRemoval.StripTagsCharArray(content.Html());

            System.Console.WriteLine("URI:" + catalog.url + " title:" + catalog.text);
        }
    }
}
