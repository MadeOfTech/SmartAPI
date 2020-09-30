using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace System
{
    public static class HexExtension
    {

        /// <summary>
        /// This return a byte array using a string defined like this :
        /// 0x[[0-9a-fA-F]{2}]* as an input.
        /// </summary>
        /// <param name="str">string representing byte array</param>
        /// <returns>byte array</returns>
        public static byte[] UnHex(this string str)
        {
            return Enumerable.Range(2, str.Length - 2)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(str.Substring(x, 2), 16))
                         .ToArray();
        }

        public static string Hex(this byte[] array)
        {
            return "0x" + BitConverter.ToString(array).Replace("-", string.Empty);
        }
}
}
