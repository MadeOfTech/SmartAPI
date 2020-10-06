using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Exceptions
{
	public class SmartAPIException : Exception
	{
		public SmartAPIException(string p_Message, Exception p_InnerException) : base(p_Message, p_InnerException)
		{
		}

		public SmartAPIException(string p_Message) : base(p_Message)
		{
		}

		public SmartAPIException() : base() { }
	}
}
