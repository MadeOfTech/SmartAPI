using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MadeOfTech.SmartAPI.XmlSerialization
{
	[Serializable]
	public class XmlAttributeSerialize : IXmlSerializable
	{
		public XmlAttributeSerialize() { }

		public XmlAttributeSerialize(object value)
		{
			_value = value;
		}

		private readonly object _value;

		public XmlSchema GetSchema()
		{
			return null;// new XmlSchema();
		}

		public void ReadXml(XmlReader reader)
		{

		}

		public void WriteXml(XmlWriter writer)
		{
			if (null == _value) return;
			writer.WriteValue(XmlValueFormater.Format(_value));
		}
	}
}
