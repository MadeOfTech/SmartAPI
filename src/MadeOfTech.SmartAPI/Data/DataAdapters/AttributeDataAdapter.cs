using Dapper;
using MadeOfTech.SmartAPI.Data.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MadeOfTech.SmartAPI.DataAdapters
{
    public class AttributeDataAdapter
	{
		private IDbConnection _dbConnection;

		private static string _getAllSqlStatement =
@"
SELECT
	id,
	collection_id,
	attributename,
	columnname,
	description,
	type,
	format,
	autovalue,
	nullable,
	keyindex,
	fiqlkeyindex
FROM
	attribute
ORDER BY
	collection_id,
	IFNULL(keyindex, 2130706431),
	id
";
		private static string _getByCollectionIdSqlStatement = _getAllSqlStatement +
@"
WHERE
	collection_id = @collection_id
";

		public AttributeDataAdapter(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		public List<Attribute> getAll()
		{
			return _dbConnection.Query<Attribute>(_getAllSqlStatement).ToList();
		}

		public List<Attribute> getByCollectionId(int collectionId)
		{
			return _dbConnection.Query<Attribute>(_getByCollectionIdSqlStatement, new { collection_id = collectionId }).ToList();
		}
	}
}
