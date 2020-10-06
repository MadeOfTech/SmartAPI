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
	attribute.id,
	attribute.collection_id,
	attribute.attributename,
	attribute.columnname,
	attribute.description,
	attribute.type,
	attribute.format,
	attribute.autovalue,
	attribute.nullable,
	attribute.keyindex,
	attribute.fiqlkeyindex
FROM
	attribute INNER JOIN
	collection ON attribute.collection_id=collection.id INNER JOIN
	api ON collection.api_id=api.id
WHERE
	api.designation = @api_designation
ORDER BY
	attribute.collection_id,
	IFNULL(attribute.keyindex, 2130706431),
	attribute.id
";

		public AttributeDataAdapter(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		public List<Attribute> getAll(string apiDesignation)
		{
			var parameters = new Dictionary<string, object>();
			parameters.Add("api_designation", apiDesignation);
			return _dbConnection.Query<Attribute>(_getAllSqlStatement, parameters).ToList();
		}
	}
}
