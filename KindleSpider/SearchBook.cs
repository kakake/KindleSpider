using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using KindleSpider.BookSP;
using KindleSpider.Common;

namespace KindleSpider
{
    public class SearchBook
    {
        public static List<ResultBook> Search(string keychar)
        {
            List<ResultBook> rbList = new List<ResultBook>();

            List<SearchRule> SRList = RuleManager.GetSearchRules();
            foreach (SearchRule rule in SRList)
            {
                Uri uri = new Uri(string.Format(rule.searchurl, keychar));
                string html;
                NSoup.Nodes.Document doc = NSoupHelper.GetNSoupDoc(uri.ToString(), rule.charset, out html);

                var listdoc = doc.Select(rule.list_rule);
                for (int i = 0; i < listdoc.Count; i++)
                {
                    NSoup.Nodes.Element titledoc = null, autherdoc = null, coverdoc = null, introductiondoc = null, bookurldoc = null;

                    if (rule.title_rule != "")
                        titledoc = listdoc[i].Select(rule.title_rule).First();
                    if (rule.auther_rule != "")
                        autherdoc = listdoc[i].Select(rule.auther_rule).First();
                    if (rule.cover_rule != "")
                        coverdoc = listdoc[i].Select(rule.cover_rule).First();
                    if (rule.introduction_rule != "")
                        introductiondoc = listdoc[i].Select(rule.introduction_rule).First();
                    if (rule.bookurl_rule != "")
                    {
                        //bookurldoc = listdoc.Select(rule.bookurl_rule).Eq(1).First();
                        var qdoc = listdoc[i].Select(rule.bookurl_rule);
                        List<RuleConditions> ruleCons = rule.conditions.FindAll(x => x.name == "bookurl");
                        if (ruleCons.Count > 0)
                        {
                            RuleConditions r = ruleCons[0];
                            bookurldoc = r.getElement(qdoc, r);
                        }
                        else
                            bookurldoc = qdoc.First;
                    }
                    ResultBook retbook = new ResultBook();
                    retbook.title = titledoc == null ? "" : titledoc.Text();
                    retbook.auther = autherdoc == null ? "" : autherdoc.Text();
                    retbook.cover = coverdoc == null ? "" : coverdoc.Attr("src");
                    retbook.introduction = introductiondoc == null ? "" : introductiondoc.Text();
                    retbook.bookurl = bookurldoc == null ? "" : bookurldoc.Attr("href");
                    rbList.Add(retbook);
                }
            }
            return rbList;
        }
    }

    public class ResultBook
    {
        public string title { get; set; }
        public string auther { get; set; }
        public string cover { get; set; }
        public string introduction { get; set; }
        public string bookurl { get; set; }
    }
}
