using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NotesAPI
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
            File.Delete("notesdb.sqlite");

            if (!System.IO.File.Exists("notesdb.sqlite"))
            {
                // Configure a sample DB for our api.
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NotesAPI.Sql.CreateNotesDb.sql"))
                using (StreamReader reader = new StreamReader(stream))
                using (var sqliteConnection = new SQLiteConnection("Data Source=notesdb.sqlite"))
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
