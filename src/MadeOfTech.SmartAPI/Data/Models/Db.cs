using System.Collections.Generic;

namespace MadeOfTech.SmartAPI.Data.Models
{
    public class Db
    {
        public int? id { get; set; }
        public API api { get; set; }
        public string designation { get; set; }
        public string dbtype { get; set; }
        public string connectionstring { get; set; }
        public IEnumerable<Collection> collections { get; set; }
    }
}
