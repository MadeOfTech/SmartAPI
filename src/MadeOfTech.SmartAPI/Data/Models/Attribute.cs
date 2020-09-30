namespace MadeOfTech.SmartAPI.Data.Models
{
    public class Attribute
    {
        public int id { get; set; }
        public int collection_id { get; set; }
        public string attributename { get; set; }
        public string columnname { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string format { get; set; }
        public bool autovalue { get; set; }
        public bool nullable { get; set; }
        public int? keyindex { get; set; }
        public int? fiqlkeyindex { get; set; }
    }
}
