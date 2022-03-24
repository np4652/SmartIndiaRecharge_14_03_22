using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Xml;
using Newtonsoft.Json;

namespace Fintech.AppCode.Configuration
{
    public class ToDataSet
    {
        private static Lazy<ToDataSet> Instance = new Lazy<ToDataSet>(() => new ToDataSet());
        public static ToDataSet O { get { return Instance.Value; } }
        private ToDataSet()
        {

        }
        public DataSet ReadDataFromJson(String jsonString)
        {
            try
            {
                var xd = new XmlDocument();
                jsonString = "{ \"rootNode\": {" + jsonString.Trim().TrimStart('{').TrimEnd('}') + @"} }";
                xd = JsonConvert.DeserializeXmlNode(jsonString);
                var result = new DataSet();
                result.ReadXml(new XmlNodeReader(xd));
                return result;
            }
            catch (Exception)
            {
                return new DataSet();
            }

        }
        public DataSet ReadDataFromXML(String XMLString)
        {
            try
            {
                var xd = new XmlDocument();
                xd.LoadXml(XMLString);
                var declarations = xd.ChildNodes.OfType<XmlNode>()
                    .Where(x => x.NodeType == XmlNodeType.XmlDeclaration)
                    .ToList();
                declarations.ForEach(x => xd.RemoveChild(x));
                var result = new DataSet();
                result.ReadXml(new XmlNodeReader(xd));
                return result;
            }
            catch (Exception)
            {
                return new DataSet();
            }
        }
    }
}
