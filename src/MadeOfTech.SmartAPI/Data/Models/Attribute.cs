using MadeOfTech.SmartAPI.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MadeOfTech.SmartAPI.Data.Models
{
    public class Attribute
    {
        public int collection_id { get; set; }
        public string attributename { get; set; }
        public string columnname { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string format { get; set; }
        public bool autovalue { get; set; }
        public bool nullable { get; set; }
        public int? keyindex { get; set; }
        public int? fiqlkeyindex { get; set; }
        /// <summary>
        /// This method parse all possible type of attribute.
        /// </summary>
        /// <remarks>
        /// Please note that the first thing that this method try to do is to
        /// transform the given value into a string. The reason for that is that
        /// we want to be able to work on route derived key the same way than on
        /// body values.
        /// </remarks>
        /// <param name="attribute">description of the attribute we wan't to parse the value</param>
        /// <param name="value">raw value of the attribute</param>
        /// <returns></returns>
        public object ParseAttributeValue(dynamic value)
        {
            if (null == value)
            {
                return null;
            }
            else
            {
                try
                {
                    string stringValue = value.ToString();
                    if (this.type == "boolean")
                    {
                        if (0 == "true".CompareTo(stringValue.ToLower())) return true;
                        else if (0 == "false".CompareTo(stringValue.ToLower())) return false;
                        else throw new Exception("can't parse boolean");
                    }
                    #region string
                    else if (this.type == "string" && this.format == "0xbyte")
                    {
                        if (String.IsNullOrEmpty(value))
                        {
                            return (byte[])null;
                        }
                        else if (stringValue.StartsWith("0x"))
                        {
                            return stringValue.UnHex();
                        }
                    }
                    else if (this.type == "string" && this.format == "byte")
                    {
                        if (String.IsNullOrEmpty(value))
                        {
                            return (byte[])null;
                        }
                        else
                        {
                            return Convert.FromBase64String(stringValue);
                        }
                    }
                    else if (this.type == "string" && this.format == "date")
                    {
                        if (String.IsNullOrEmpty(value))
                        {
                            return (DateTime?)null;
                        }
                        else
                        {
                            return DateTime.ParseExact(stringValue, new string[] { "yyyyMMdd", "yyyy-MM-dd" },
                                CultureInfo.InvariantCulture, DateTimeStyles.None | DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
                        }
                    }
                    else if (this.type == "string" && this.format == "date-time")
                    {
                        if (String.IsNullOrEmpty(value))
                        {
                            return (DateTime?)null;
                        }
                        else
                        {
                            return stringValue.UnISO8601();
                        }
                    }
                    #endregion
                    #region integer
                    else if (this.type == "integer" && this.format == "int32")
                    {
                        return String.IsNullOrEmpty(value) ? (int?)null : int.Parse(value.ToString());
                    }
                    else if (this.type == "integer")
                    {
                        // any not formatted integer will be considered as a long
                        return String.IsNullOrEmpty(value) ? (long?)null : long.Parse(value.ToString());
                    }
                    #endregion
                    #region number
                    else if (this.type == "number" && this.format == "float")
                    {
                        return String.IsNullOrEmpty(value) ? (float?)null : float.Parse(value.ToString());
                    }
                    else if (this.type == "number")
                    {
                        // any not formatted number will be considered as a double
                        return String.IsNullOrEmpty(value) ? (double?)null : double.Parse(value.ToString());
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    string message = "attribute " + this.attributename + " can't be parsed as a " + this.type;
                    if (!String.IsNullOrEmpty(this.format)) message += "(" + this.format + ")";
                    throw new HttpException(System.Net.HttpStatusCode.UnprocessableEntity, message, ex);
                }
                return value;
            }
        }

        public string ToStringJson(dynamic value)
        {
            string result = ToQuotedString(value);
            if (null == result) return "\"" + attributename + "\":null";
            return "\"" + attributename + "\":" + ToQuotedString(value);
        }
        public string ToStringXML(dynamic value)
        {
            string result = ToUnquotedString(value);
            if (null == result) return "<" + attributename + " xsi:nil=\"true\"/>";
            else return "<" + attributename + ">" + result + "</" + attributename + ">";
        }

        /// <summary>
        /// This method format correctly the attribute without adding any quote.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string ToUnquotedString(dynamic value)
        {
            if (null == value)
            {
                return null;
            }
            else
            {
                try
                {
                    if (this.type == "boolean")
                    {
                        return ((bool)value) ? "true" : "false";
                    }
                    #region string
                    else if (this.type == "string" && this.format == "0xbyte")
                    {
                        var byteValue = (IEnumerable<byte>)value;
                        return byteValue.Hex();
                    }
                    else if (this.type == "string" && this.format == "byte")
                    {
                        var byteValue = (IEnumerable<byte>)value;
                        return Convert.ToBase64String(byteValue.ToArray());
                    }
                    else if (this.type == "string" && this.format == "date")
                    {
                        var dateValue = (DateTime)value;
                        return dateValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    }
                    else if (this.type == "string" && this.format == "date-time")
                    {
                        var dateValue = (DateTime)value;
                        return dateValue.ISO8601();
                    }
                    else if (this.type == "string")
                    {
                        return value.ToString();
                    }
                    #endregion
                    #region integer && number
                    else if (this.type == "integer")
                    {
                        return value.ToString();
                    }
                    else if (this.type == "integer" || this.type == "number")
                    {
                        // any not formatted integer will be considered as a long
                        return String.IsNullOrEmpty(value) ? (long?)null : long.Parse(value.ToString());
                    }
                    #endregion
                    throw new Exception("unknown type");
                }
                catch (Exception ex)
                {
                    string message = "attribute " + this.attributename + " can't be output as a " + this.type;
                    if (!String.IsNullOrEmpty(this.format)) message += "(" + this.format + ")";
                    throw new HttpException(System.Net.HttpStatusCode.UnprocessableEntity, message, ex);
                }
            }
        }
        public string ToQuotedString(dynamic value)
        {
            string result = this.ToUnquotedString(value);
            if (null == result) return null;
            else if (this.type == "string") return "\"" + result + "\"";
            else return result;
        }
    }
}
