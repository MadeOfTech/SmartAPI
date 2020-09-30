using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MadeOfTech.SmartAPI.XmlSerialization
{
	[Serializable]
	public class XmlDynamicSerialize : IXmlSerializable
	{
		public XmlDynamicSerialize() { }

		public XmlDynamicSerialize(dynamic dynamicObject)
		{
			_dynamicObject = dynamicObject;
		}

		private readonly dynamic _dynamicObject;

		public XmlSchema GetSchema()
		{
			return null;// new XmlSchema();
		}

		public void ReadXml(XmlReader reader)
		{

		}

		public void WriteXml(XmlWriter writer)
		{
			IDictionary<string, object> propertyValues = (IDictionary<string, object>)_dynamicObject;
			//Provide elements for object item

			foreach (var keyValuePair in propertyValues)
			{
				//Provide elements for per property
				if (null == keyValuePair.Value)
					writer.WriteElementString(keyValuePair.Key, null);
				else
					writer.WriteElementString(keyValuePair.Key, XmlValueFormater.Format(keyValuePair.Value));
			}
		}
	}
}
