using Dapper;
using MadeOfTech.SmartAPI.Data.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MadeOfTech.SmartAPI.DataAdapters
{
	public class APIDataAdapter
	{
		private IDbConnection _dbConnection;

		private static string _getAllSqlStatement =
@"
SELECT
	id,
	designation,
	description,
	basepath
FROM
	api
WHERE
	api.designation = @api_designation
";

		public APIDataAdapter(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		public List<API> getAll(string apiDesignation)
		{
			var parameters = new Dictionary<string, object>();
			parameters.Add("api_designation", apiDesignation);
			return _dbConnection.Query<API>(_getAllSqlStatement, parameters).ToList();
		}
	}
}
