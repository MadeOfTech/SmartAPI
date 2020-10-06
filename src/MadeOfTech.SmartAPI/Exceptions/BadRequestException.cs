using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Exceptions
{
	public class BadRequestException : HttpException
	{
		public BadRequestException(string p_Message) : base(System.Net.HttpStatusCode.BadRequest, p_Message) { }
		public BadRequestException() : base(System.Net.HttpStatusCode.NotFound) { }
	}
}
