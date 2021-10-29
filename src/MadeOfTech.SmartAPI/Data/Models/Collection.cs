using System.Collections.Generic;

namespace MadeOfTech.SmartAPI.Data.Models
{
    public class Collection
    {
		public int? id { get; set; }
		public Db db { get; set; }
		public int? db_id { get; set; }
		public string collectionname { get; set; }
		public string membername { get; set; }
		public string tablename { get; set; }
		public string description { get; set; }
		public bool publish_getcollection { get; set; }
		public bool publish_getmember { get; set; }
		public bool publish_postmember { get; set; }
		public bool publish_putmember { get; set; }
		public bool publish_deletemember { get; set; }
		public bool publish_patchmember { get; set; }
		public IEnumerable<Attribute> attributes { get; set; }
	}
}
