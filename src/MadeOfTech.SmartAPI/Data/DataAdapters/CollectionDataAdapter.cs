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
	api ON api_id=api.id INNER JOIN
	db ON db_id=db.id INNER JOIN
	dbtype ON dbtype_id=dbtype.id
WHERE
	api.designation=@api_designation
ORDER BY
	collection.collectionname,
	collection.id
";
		public CollectionDataAdapter(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		public List<Collection> getAll(string apiDesignation)
		{
			var parameters = new Dictionary<string, object>();
			parameters.Add("api_designation", apiDesignation);
			return _dbConnection.Query<Collection>(_getAllSqlStatement, parameters).ToList();
		}
	}
}
