using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using KindleSpider.Common;

namespace KindleSpider.BookSP
{
    /// <summary>
    /// 书籍规则
    /// </summary>
    public class BookRule
    {
        public string name { get; set; }
        public string url { get; set; }
        public string charset { get; set; }

        /// <summary>
        /// 查询条件
        /// </summary>
        public List<RuleConditions> conditions { get; set; }
        public string title_rule { get; set; }
        public string auther_rule { get; set; }
        public string cover_rule { get; set; }
        public string introduction_rule { get; set; }
        public string catalog_rule { get; set; }
        public string content_rule { get; set; }

    }
    /// <summary>
    /// 查询规则
    /// </summary>
    public class SearchRule
    {
     
        public string name { get; set; }
        public string searchurl { get; set; }
        public string charset { get; set; }

        public string list_rule { get; set; }
        /// <summary>
        /// 查询条件
        /// </summary>
        public List<RuleConditions> conditions { get; set; }
        public string title_rule { get; set; }
        public string auther_rule { get; set; }
        public string cover_rule { get; set; }
        public string introduction_rule { get; set; }
        public string bookurl_rule { get; set; }
    }
    /// <summary>
    /// 规则条件
    /// </summary>
    public class RuleConditions
    {
        /// <summary>
        /// 节点名
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 条件名
        /// </summary>
        public string conditionName { get; set; }
        /// <summary>
        /// 条件值
        /// </summary>
        public string conditionValue { get; set; }

        public NSoup.Nodes.Element getElement(NSoup.Select.Elements elements, RuleConditions r)
        {
            var qdoc = elements;
            if (r.conditionName == "first")
            {
                return qdoc.First;
            }
            else if (r.conditionName == "eq")
            {
                return qdoc.Eq(Convert.ToInt32(r.conditionValue)).First;
            }
            else if (r.conditionName == "last")
            {
                return qdoc.Last;
            }
            else if (r.conditionName == "not")
            {
                return qdoc.Not(r.conditionValue).First;
            }
            else
                return qdoc.First;
        }
    }
    /// <summary>
    /// 爬网页规则管理
    /// </summary>
    public class RuleManager
    {
        public static string bookrulexml = "BookRule.xml";
        private static List<BookRule> _rulelist;
        public static List<BookRule> RuleList
        {
            get { return _rulelist; }
            set { _rulelist = value; }
        }

        private static List<SearchRule> _searchrulelist;
        public static List<SearchRule> SearchRuleList
        {
            get { return _searchrulelist; }
            set { _searchrulelist = value; }
        }

        public static void SaveXml(BookRule rule)
        {
            XmlHandle xmlhandle = new XmlHandle(bookrulexml);
            if (xmlhandle.GetCount("BookSite[Name=" + rule.name + "]") > 0)//更新
            {
                xmlhandle.SetValue("BookSite[Name=" + rule.name + "]", "Url", rule.url);
                xmlhandle.SetValue("BookSite[Name=" + rule.name + "]", "charset", rule.charset);
                xmlhandle.SetValue("BookSite[Name=" + rule.name + "]/title", rule.title_rule);
                xmlhandle.SetValue("BookSite[Name=" + rule.name + "]/auther", rule.auther_rule);
                xmlhandle.SetValue("BookSite[Name=" + rule.name + "]/cover", rule.cover_rule);
                xmlhandle.SetValue("BookSite[Name=" + rule.name + "]/introduction", rule.introduction_rule);
                xmlhandle.SetValue("BookSite[Name=" + rule.name + "]/catalog", rule.catalog_rule);
                xmlhandle.SetValue("BookSite[Name=" + rule.name + "]/content", rule.content_rule);
            }
            else//新增
            {
                XmlElement node = xmlhandle.AddNode("BookSite");
                node.SetAttribute("Name", rule.name);
                node.SetAttribute("Url", rule.url);
                node.SetAttribute("charset", rule.charset);

                XmlElement node1 = xmlhandle.AddNode(node, "title");
                node1.InnerText = rule.title_rule;

                XmlElement node2 = xmlhandle.AddNode(node, "auther");
                node2.InnerText = rule.auther_rule;

                XmlElement node3 = xmlhandle.AddNode(node, "cover");
                node3.InnerText = rule.cover_rule;

                XmlElement node4 = xmlhandle.AddNode(node, "introduction");
                node4.InnerText = rule.introduction_rule;

                XmlElement node5 = xmlhandle.AddNode(node, "catalog");
                node5.InnerText = rule.catalog_rule;

                XmlElement node6 = xmlhandle.AddNode(node, "content");
                node6.InnerText = rule.content_rule;
            }
            xmlhandle.SaveConfig();
        }

        public static void SaveXml(SearchRule rule)
        {
            XmlHandle xmlhandle = new XmlHandle(bookrulexml);
            if (xmlhandle.GetCount("SearchSite[Name=" + rule.name + "]") > 0)//更新
            {
                xmlhandle.SetValue("SearchSite[Name=" + rule.name + "]", "SearchUrl", rule.searchurl);
                xmlhandle.SetValue("SearchSite[Name=" + rule.name + "]", "charset", rule.charset);
                xmlhandle.SetValue("SearchSite[Name=" + rule.name + "]", "list", rule.list_rule);

                xmlhandle.SetValue("SearchSite[Name=" + rule.name + "]/title", rule.title_rule);
                xmlhandle.SetValue("SearchSite[Name=" + rule.name + "]/auther", rule.auther_rule);
                xmlhandle.SetValue("SearchSite[Name=" + rule.name + "]/cover", rule.cover_rule);
                xmlhandle.SetValue("SearchSite[Name=" + rule.name + "]/introduction", rule.introduction_rule);
                xmlhandle.SetValue("SearchSite[Name=" + rule.name + "]/bookurl", rule.bookurl_rule);
            }
            else//新增
            {
                XmlElement node = xmlhandle.AddNode("SearchSite");
                node.SetAttribute("Name", rule.name);
                node.SetAttribute("SearchUrl", rule.searchurl);
                node.SetAttribute("charset", rule.charset);
                node.SetAttribute("list", rule.list_rule);

                XmlElement node1 = xmlhandle.AddNode(node, "title");
                node1.InnerText = rule.title_rule;

                XmlElement node2 = xmlhandle.AddNode(node, "auther");
                node2.InnerText = rule.auther_rule;

                XmlElement node3 = xmlhandle.AddNode(node, "cover");
                node3.InnerText = rule.cover_rule;

                XmlElement node4 = xmlhandle.AddNode(node, "introduction");
                node4.InnerText = rule.introduction_rule;

                XmlElement node5 = xmlhandle.AddNode(node, "bookurl");
                node5.InnerText = rule.bookurl_rule;
            }
            xmlhandle.SaveConfig();
        }

        public static void LoadXml()
        {
            XmlHandle xmlhandle = new XmlHandle(bookrulexml);
            int num = xmlhandle.GetCount("BookSite");
            if (num > 0) _rulelist = new List<BookRule>();
            for (int i = 1; i <= num; i++)
            {
                BookRule rule = new BookRule();
                rule.name = xmlhandle.GetValue("BookSite[" + i + "]", "Name");
                rule.url = xmlhandle.GetValue("BookSite[" + i + "]", "Url");
                rule.charset = xmlhandle.GetValue("BookSite[" + i + "]", "charset");
                rule.title_rule = xmlhandle.GetValue("BookSite[" + i + "]/title");
                rule.auther_rule = xmlhandle.GetValue("BookSite[" + i + "]/auther");
                rule.cover_rule = xmlhandle.GetValue("BookSite[" + i + "]/cover");
                rule.introduction_rule = xmlhandle.GetValue("BookSite[" + i + "]/introduction");
                rule.catalog_rule = xmlhandle.GetValue("BookSite[" + i + "]/catalog");
                rule.content_rule = xmlhandle.GetValue("BookSite[" + i + "]/content");
                _rulelist.Add(rule);
            }

            num = xmlhandle.GetCount("SearchSite");
            if (num > 0) _searchrulelist = new List<SearchRule>();
            for (int i = 1; i <= num; i++)
            {
                SearchRule rule = new SearchRule();
                rule.name = xmlhandle.GetValue("SearchSite[" + i + "]", "Name");
                rule.searchurl = xmlhandle.GetValue("SearchSite[" + i + "]", "SearchUrl");
                rule.charset = xmlhandle.GetValue("SearchSite[" + i + "]", "charset");

                rule.list_rule = xmlhandle.GetValue("SearchSite[" + i + "]", "list");

                rule.conditions =new List<RuleConditions>();
                rule.title_rule = xmlhandle.GetValue("SearchSite[" + i + "]/title");
                XmlAttributeCollection attcoll = xmlhandle.GetAttributes("SearchSite[" + i + "]/title");
                foreach (XmlAttribute v in attcoll)
                {
                    RuleConditions cond = new RuleConditions();
                    cond.name = "title";
                    cond.conditionName = v.Name;
                    cond.conditionValue = v.Value;
                    rule.conditions.Add(cond);
                }
                rule.auther_rule = xmlhandle.GetValue("SearchSite[" + i + "]/auther");
                attcoll = xmlhandle.GetAttributes("SearchSite[" + i + "]/auther");
                foreach (XmlAttribute v in attcoll)
                {
                    RuleConditions cond = new RuleConditions();
                    cond.name = "auther";
                    cond.conditionName = v.Name;
                    cond.conditionValue = v.Value;
                    rule.conditions.Add(cond);
                }
                rule.cover_rule = xmlhandle.GetValue("SearchSite[" + i + "]/cover");
                attcoll = xmlhandle.GetAttributes("SearchSite[" + i + "]/cover");
                foreach (XmlAttribute v in attcoll)
                {
                    RuleConditions cond = new RuleConditions();
                    cond.name = "cover";
                    cond.conditionName = v.Name;
                    cond.conditionValue = v.Value;
                    rule.conditions.Add(cond);
                }

                rule.introduction_rule = xmlhandle.GetValue("SearchSite[" + i + "]/introduction");
                attcoll = xmlhandle.GetAttributes("SearchSite[" + i + "]/introduction");
                foreach (XmlAttribute v in attcoll)
                {
                    RuleConditions cond = new RuleConditions();
                    cond.name = "introduction";
                    cond.conditionName = v.Name;
                    cond.conditionValue = v.Value;
                    rule.conditions.Add(cond);
                }

                rule.bookurl_rule = xmlhandle.GetValue("SearchSite[" + i + "]/bookurl");
                attcoll = xmlhandle.GetAttributes("SearchSite[" + i + "]/bookurl");
                foreach (XmlAttribute v in attcoll)
                {
                    RuleConditions cond = new RuleConditions();
                    cond.name = "bookurl";
                    cond.conditionName = v.Name;
                    cond.conditionValue = v.Value;
                    rule.conditions.Add(cond);
                }

                _searchrulelist.Add(rule);
            }
        }

        public static BookRule GetBookRule(string name, string url)
        {
            BookRule rule = null;
            if (_rulelist == null)
                LoadXml();
            if (string.IsNullOrEmpty(name) == false)
            {
                rule = RuleList.Find(x => x.name == name);
            }
            if (string.IsNullOrEmpty(url) == false && rule == null)
            {
                rule = RuleList.Find(x => x.url == url);
            }

            return rule;
        }

        public static SearchRule GetSearchRule(string name)
        {
            SearchRule rule = null;
            if (_searchrulelist == null)
                LoadXml();
            if (string.IsNullOrEmpty(name) == false)
            {
                rule = SearchRuleList.Find(x => x.name == name);
            }
            return rule;
        }

        public static List<SearchRule> GetSearchRules()
        {
            if (_searchrulelist == null)
                LoadXml();
            return _searchrulelist;
        }
    }
}
