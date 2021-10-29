using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace System
{
    public static class ISO8601Extension
    {
        static readonly string[] iso8601Formats = { 
    // Basic formats
    "yyyyMMddTHHmmssfffzzz",
    "yyyyMMddTHHmmssfffzz",
    "yyyyMMddTHHmmssfffZ",
    "yyyyMMddTHHmmssfffffffZ",
    // Extended formats
    "yyyy-MM-ddTHH:mm:ss.fffzzz",
    "yyyy-MM-ddTHH:mm:ss.fffzz",
    "yyyy-MM-ddTHH:mm:ss.fffZ",
    "yyyy-MM-ddTHH:mm:ss.fffffffZ",
    // Basic formats
    "yyyyMMddTHHmmsszzz",
    "yyyyMMddTHHmmsszz",
    "yyyyMMddTHHmmssZ",
    // Extended formats
    "yyyy-MM-ddTHH:mm:sszzz",
    "yyyy-MM-ddTHH:mm:sszz",
    "yyyy-MM-ddTHH:mm:ssZ",
    };

        /// <summary>
        /// This 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime UnISO8601(this string str)
        {
            return DateTime.ParseExact(str, iso8601Formats,
                CultureInfo.InvariantCulture, DateTimeStyles.None | DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        }

        public static string ISO8601(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture);
        }
}
}
