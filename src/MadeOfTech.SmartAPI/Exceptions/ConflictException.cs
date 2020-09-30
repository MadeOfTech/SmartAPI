using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Exceptions
{
	public class ConflictException : SmartAPIException
	{
		public ConflictException(string p_Message) : base(System.Net.HttpStatusCode.Conflict, p_Message) { }
		public ConflictException() : base(System.Net.HttpStatusCode.Conflict) { }
	}
}
