using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Fintech.AppCode.HelperClass
{
    public class XMLHelper
    {
        private static Lazy<XMLHelper> Instance = new Lazy<XMLHelper>(() => new XMLHelper());
        public static XMLHelper O => Instance.Value;
        private XMLHelper() { }
        public string SerializeToXml<T>(T obj, string root, bool IsXmlDeclartion = false)
        {
            XmlRootAttribute xmlRootAttribute = new XmlRootAttribute(root);
            XmlSerializer serializer = new XmlSerializer(obj.GetType(), xmlRootAttribute);
            var xml = "";
            try
            {
                using (var sw = new StringWriter())
                {
                    using (XmlWriter xw = XmlWriter.Create(sw))
                    {
                        serializer.Serialize(xw, obj);
                        xml = sw.ToString();
                    }
                }
                if (!IsXmlDeclartion)
                {
                    root = string.IsNullOrEmpty(root) ? obj.GetType().Name : root;
                    XmlDocument xmld = new XmlDocument();
                    xmld.LoadXml(xml);
                    var nodeList = xmld.SelectNodes(root);
                    string newXml = "";
                    foreach (XmlNode node in nodeList)
                    {
                        newXml += node.InnerXml;
                    }
                    xml = "<" + root + ">" + newXml + "</" + root + ">";
                }
            }
            catch (Exception ex)
            {
                xml = ex.Message + "\n" + xml;
            }
            return xml;
        }
        public  string SerializeObject<T>(T obj)
        {
            var serializer = new XmlSerializer(typeof(T));

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UnicodeEncoding(true, true);
            settings.Indent = true;
            //settings.OmitXmlDeclaration = true;  

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, obj, ns);
                }

                return textWriter.ToString();  
            }
        }
        public T DesrializeToObject<T>(T obj, string xmlStr, string replaceRoot, bool IsRootReplace = false)
        {
            xmlStr = ReplaceDeclaration(xmlStr);
            string root = obj.GetType().Name;
            xmlStr = IsRootReplace && !string.IsNullOrEmpty(replaceRoot) ? xmlStr.Replace(replaceRoot, root) : xmlStr;
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(xmlStr));
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            obj = (T)serializer.Deserialize(ms);
            return obj;
        }
        private string ReplaceDeclaration(string xml)
        {
            int LastindexOf = (xml ?? string.Empty).LastIndexOf("?>") + 2;
            if (LastindexOf >= 2)
                return xml.Substring(LastindexOf, xml.Length - LastindexOf);
            return xml;
        }

        public string EncodeForXml(string data)
        {
            System.Text.RegularExpressions.Regex badAmpersand = new System.Text.RegularExpressions.Regex("&(?![a-zA-Z]{2,6};|#[0-9]{2,4};)");
            data = badAmpersand.Replace(data, "&amp;");

            return data.Replace("<", "&lt;").Replace("\"", "&quot;").Replace(">", "&gt;");
        }

    }
}
