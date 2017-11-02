using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace MyNLP.Helpers
{
    public class XmlHelper
    {
        public static void AppendAttribute(XmlElement element, string elementName, object elementValue)
        {
            XmlDocument doc = element.OwnerDocument;
            XmlAttribute attr = doc.CreateAttribute(elementName);
            attr.Value = elementValue.ToString();
            element.Attributes.Append(attr);
        }
    }
}
