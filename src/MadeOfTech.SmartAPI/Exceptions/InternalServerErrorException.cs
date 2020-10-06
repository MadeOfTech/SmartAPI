using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Exceptions
{
	public class InternalServerErrorException : HttpException
	{
		public InternalServerErrorException(string p_Message, Exception p_InnerException) : base(System.Net.HttpStatusCode.InternalServerError, p_Message, p_InnerException) { }
		public InternalServerErrorException(string p_Message) : base(System.Net.HttpStatusCode.InternalServerError, p_Message) { }
		public InternalServerErrorException() : base(System.Net.HttpStatusCode.InternalServerError) { }
	}
}
