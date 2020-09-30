using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Operations
{
	public class OperationOutput
	{
		public enum RessourceType
		{
			Collection,
			Member,
			Attribute
		}

		public OperationOutput(RessourceType type, string name, object value)
		{
			Name = name;
			Value = value;
			Type = type;
		}

		public string Name { get; }
		public object Value { get; }
		public RessourceType Type { get; }
	}
}
