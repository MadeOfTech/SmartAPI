using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Exceptions
{
	public class InvalidParameterException : BadRequestException
	{
		public InvalidParameterException(string p_ParameterName, string p_ParameterDescription) : base("Paramètre invalide " + p_ParameterName + ". " + p_ParameterDescription) { }
		public InvalidParameterException(string p_ParameterName) : base("Paramètre invalide " + p_ParameterName + ".") { }
	}
}
