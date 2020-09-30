using System;
using System.Collections.Generic;
using System.Text;

namespace MadeOfTech.SmartAPI.FIQL
{
    public class FIQLException : Exception
    {
        public FIQLException(string message) : base(message) { }
    }
}
