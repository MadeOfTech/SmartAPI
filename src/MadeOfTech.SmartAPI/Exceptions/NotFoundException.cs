using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Exceptions
{
	public class NotFoundException : SmartAPIException
	{
		public NotFoundException(string p_Message) : base(System.Net.HttpStatusCode.NotFound, p_Message) { }
		public NotFoundException() : base(System.Net.HttpStatusCode.NotFound) { }
	}
}
