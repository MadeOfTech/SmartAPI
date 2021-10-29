using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Operations
{
    public class PatchOperation
    {
        public string op { get; set; }
        public string path { get; set; }
        public dynamic value { get; set; }
    }
}
