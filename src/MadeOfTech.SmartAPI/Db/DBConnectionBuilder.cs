using MySql.Data.MySqlClient;
using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Db
{
    public static class DBConnectionBuilder
    {
		public static IDbConnection Use(string dbType, string connectionString)
		{
			switch (dbType.ToLower())
			{
				case "mysql":
					if (!connectionString.Contains("UseAffectedRows")) connectionString += ";UseAffectedRows=True;";
					return new MySqlConnection(connectionString);

				case "sqlite":
					return new SQLiteConnection(connectionString);
				
				default: return null;
			}
		}

		public static async Task OpenAsync(this IDbConnection connection)
		{
			if (connection.GetType().IsAssignableFrom(typeof(MySqlConnection)))
			{
				MySqlConnection mySqlConnection = (MySqlConnection)connection;
				await mySqlConnection.OpenAsync();
			}
			else if (connection.GetType().IsAssignableFrom(typeof(SQLiteConnection)))
			{
				SQLiteConnection mySqlConnection = (SQLiteConnection)connection;
				await mySqlConnection.OpenAsync();
			}
			else
			{
				connection.Open();
			}
		}

		public static async Task CloseAsync(this IDbConnection connection)
		{
			if (connection.GetType().IsAssignableFrom(typeof(MySql.Data.MySqlClient.MySqlConnection)))
			{
				MySqlConnection mySqlConnection = (MySqlConnection)connection;
				await mySqlConnection.CloseAsync();
			}
			else if (connection.GetType().IsAssignableFrom(typeof(SQLiteConnection)))
			{
				SQLiteConnection mySqlConnection = (SQLiteConnection)connection;
				await mySqlConnection.CloseAsync();
			}
			else
			{
				connection.Close();
			}
		}


		public static string LastInsertedIdClause(this IDbConnection connection)
		{
			if (connection.GetType().IsAssignableFrom(typeof(MySql.Data.MySqlClient.MySqlConnection)))
			{
				return "LAST_INSERT_ID()";
			}
			else if (connection.GetType().IsAssignableFrom(typeof(SQLiteConnection)))
			{
				return "LAST_INSERT_ROWID()";
			}
			else
			{
				return "0";
			}
		}
	}
}
