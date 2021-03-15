using Dapper;
using MadeOfTech.SmartAPI.Data.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MadeOfTech.SmartAPI.DataAdapters
{
    public class DbDataAdapter
    {
		private IDbConnection _dbConnection;

		private static string _getAllSqlStatement =
@"
SELECT
	db.id,
	db.designation,
	db.dbtype,
	db.connectionstring
FROM
	db INNER JOIN
	api ON api_id=api.id
WHERE
	api.designation=@api_designation
ORDER BY
	db.id
";
		public DbDataAdapter(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		public List<Data.Models.Db> getAll(string apiDesignation)
		{
			var parameters = new Dictionary<string, object>();
			parameters.Add("api_designation", apiDesignation);
			return _dbConnection.Query<Data.Models.Db>(_getAllSqlStatement, parameters).ToList();
		}
	}
}
