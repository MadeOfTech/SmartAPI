namespace MadeOfTech.SmartAPI.Data.Models
{
    public class Collection
    {
		public int id { get; set; }
		public string collectionname { get; set; }
		public string membername { get; set; }
		public string dbtype_designation { get; set; }
		public string connectionstring { get; set; }
		public string tablename { get; set; }
		public string description { get; set; }
		public bool publish_getcollection { get; set; }
		public bool publish_getmember { get; set; }
		public bool publish_postmember { get; set; }
		public bool publish_putmember { get; set; }
		public bool publish_deletemember { get; set; }
	}
}
