using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Exceptions
{
	public class SmartAPIException : Exception
	{
		public static string Host { get; set; }

		public SmartAPIException(System.Net.HttpStatusCode p_HttpStatusCode, string p_Message, Exception p_InnerException) : base(p_Message, p_InnerException)
		{
			HttpStatusCode = p_HttpStatusCode;
		}

		public SmartAPIException(System.Net.HttpStatusCode p_HttpStatusCode, string p_Message) : base(p_Message)
		{
			HttpStatusCode = p_HttpStatusCode;
		}

		public SmartAPIException(System.Net.HttpStatusCode p_HttpStatusCode) : base()
		{
			HttpStatusCode = p_HttpStatusCode;
		}

		public SmartAPIException() : base() { }

		public System.Net.HttpStatusCode HttpStatusCode { get; set; } = System.Net.HttpStatusCode.OK;
	}
}
