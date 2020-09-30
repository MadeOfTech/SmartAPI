using System;

namespace MadeOfTech.SmartAPI.XmlSerialization
{
    public class XmlValueFormater
    {
        /// <summary>
        /// Prepare value to be outputed depending on their type.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Format(object value)
        {
            if (null == value)
                return null;
            else if (value.GetType().Equals(typeof(System.Byte[])))
                return ((byte[])value).Hex();
            else if (value.GetType().Equals(typeof(DateTime)))
                return ((DateTime)value).ISO8601();
            else
                return value.ToString();
        }
    }
}
