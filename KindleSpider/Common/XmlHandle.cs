using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace KindleSpider.Common
{
    public class XmlHandle
    {
        private System.Xml.XmlDocument xmlDoc = null;
        private string configfile = "";//System.Windows.Forms.Application.StartupPath + "\\Config\\SettingConfig.xml";

        public XmlHandle(string file)
        {
            configfile = file;
            xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(configfile);
        }


        public XmlHandle(string file,string xmlstr)
        {
            configfile = file;
            xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(xmlstr);
        }

        public string GetValue(string keyname)
        {
            return xmlDoc.DocumentElement.SelectNodes(keyname)[0].InnerText;
        }

        public XmlAttributeCollection GetAttributes(string keyname)
        {
            return xmlDoc.DocumentElement.SelectNodes(keyname)[0].Attributes;
        }

        public int GetCount(string keyname)
        {
            return xmlDoc.DocumentElement.SelectNodes(keyname).Count;
        }

        public string GetValue(string keyname, string attrname)
        {
            return xmlDoc.DocumentElement.SelectNodes(keyname)[0].Attributes[attrname].Value.ToString();
        }

        public void SetValue(string keyname, string value)
        {
            value = value == null ? "" : value;
            xmlDoc.DocumentElement.SelectNodes(keyname)[0].InnerText = value;
        }

        public void SetValue(string keyname, string attrname, string value)
        {
            value = value == null ? "" : value;
            xmlDoc.DocumentElement.SelectNodes(keyname)[0].Attributes[attrname].Value = value;
        }

        public XmlElement AddNode(string nodename)
        {
            XmlElement node = xmlDoc.CreateElement(nodename);
            xmlDoc.DocumentElement.AppendChild(node);
            return node;
        }

        public XmlElement AddNode(XmlElement Node,string nodename)
        {
            XmlElement node = xmlDoc.CreateElement(nodename);
            Node.AppendChild(node);
            return node;
        }

        public XmlElement AddNode(string keyname, string nodename)
        {
            XmlElement node = xmlDoc.CreateElement(nodename);
             xmlDoc.DocumentElement.SelectSingleNode(keyname).AppendChild(node);
             return node;
        }

        public void SaveConfig()
        {
            xmlDoc.Save(configfile);
        }
    }
}
