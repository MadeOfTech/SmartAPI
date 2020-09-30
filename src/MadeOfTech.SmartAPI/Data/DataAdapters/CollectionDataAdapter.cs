using Dapper;
using MadeOfTech.SmartAPI.Data.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MadeOfTech.SmartAPI.DataAdapters
{
    public class CollectionDataAdapter
    {
		private IDbConnection _dbConnection;

		private static string _getAllSqlStatement =
@"
SELECT
	collection.id,
	collection.collectionname,
    collection.membername,
	dbtype.designation AS dbtype_designation,
	db.connectionstring,
	collection.tablename,
	collection.description,
	collection.publish_getcollection,
	collection.publish_getmember,
	collection.publish_postmember,
	collection.publish_putmember,
	collection.publish_deletemember
FROM
	collection INNER JOIN
	db ON db_id=db.id INNER JOIN
	dbtype ON dbtype_id=dbtype.id;
";
		public CollectionDataAdapter(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		public List<Collection> getAll()
		{
			return _dbConnection.Query<Collection>(_getAllSqlStatement).ToList();
		}
	}
}
