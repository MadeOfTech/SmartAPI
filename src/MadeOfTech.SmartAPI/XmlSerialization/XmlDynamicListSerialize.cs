using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MadeOfTech.SmartAPI.XmlSerialization
{
	[Serializable]
	public class XmlDynamicListSerialize : IXmlSerializable
	{
		public XmlDynamicListSerialize() { }

		public XmlDynamicListSerialize(List<dynamic> dynamicList, string memberName)
		{
			_dynamicList = dynamicList;
			_memberName = memberName;
		}

		private readonly List<dynamic> _dynamicList;
		private readonly string _memberName;

		public XmlSchema GetSchema()
		{
			return null;// new XmlSchema();
		}

		public void ReadXml(XmlReader reader)
		{

		}

		public void WriteXml(XmlWriter writer)
		{
			foreach (var obj in _dynamicList)
			{
				IDictionary<string, object> propertyValues = (IDictionary<string, object>)obj;
				//Provide elements for object item
				writer.WriteStartElement(_memberName);
				foreach (var keyValuePair in propertyValues)
				{
					//Provide elements for per property
					if (null == keyValuePair.Value)
						writer.WriteElementString(keyValuePair.Key, null);
					else
						writer.WriteElementString(keyValuePair.Key, XmlValueFormater.Format(keyValuePair.Value));
				}
				writer.WriteEndElement();
			}
		}
	}
}
