using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuotesAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            InitializeDbs();

            CreateHostBuilder(args).Build().Run();
        }

        private static void InitializeDbs()
        {
            File.Delete("quotesdb.sqlite");
            File.Delete("apidb.sqlite");

            if (!System.IO.File.Exists("quotesdb.sqlite"))
            {
                // Configure a sample DB for our api.
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("QuotesAPI.Sql.CreateQuotesDb.sql"))
                using (StreamReader reader = new StreamReader(stream))
                using (var sqliteConnection = new SQLiteConnection("Data Source=quotesdb.sqlite"))
                {
                    var init = reader.ReadToEnd();
                    sqliteConnection.Open();
                    sqliteConnection.Execute(init);
                    sqliteConnection.Close();
                }
            }

            if (!System.IO.File.Exists("apidb.sqlite"))
            {
                // Configure a DB for the configuration of our api.
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("QuotesAPI.Sql.CreateAPIDb.sql"))
                using (StreamReader reader = new StreamReader(stream))
                using (var sqliteConnection = new SQLiteConnection("Data Source=apidb.sqlite"))
                {
                    var init = reader.ReadToEnd();
                    sqliteConnection.Open();
                    sqliteConnection.Execute(init);
                    sqliteConnection.Close();
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
