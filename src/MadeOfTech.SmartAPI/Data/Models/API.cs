using System.Collections.Generic;
using System.Linq;

namespace MadeOfTech.SmartAPI.Data.Models
{
    public class API
    {
        public string designation { get; set; }
        public string description { get; set; }
        public string basepath { get; set; }
        public IEnumerable<Db> dbs { get; set; }
        public IEnumerable<Collection> collections
        {
            get
            {
                return this.dbs.SelectMany(db => db.collections);
            }
        }
    }
}
