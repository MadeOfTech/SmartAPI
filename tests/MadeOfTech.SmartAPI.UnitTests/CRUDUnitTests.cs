using Dapper;
using MadeOfTech.SmartAPI.UnitTests.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace MadeOfTech.SmartAPI.UnitTests
{
    /// <summary>
    /// For this test to run correctly, we will use a SQLite database declared 
    /// thru parameter a Json file. We will then be able to request each
    /// endpoint and verify that everything works fine.
    /// </summary>
    public class CRUDUnitTests
    {
        private TestServer _server;
        public HttpClient Client { get; private set; }


        public CRUDUnitTests()
        {
            Console.WriteLine("CRUDUnitTests()");
            File.Delete("cruddb.sqlite");

            if (!System.IO.File.Exists("cruddb.sqlite"))
            {
                // Configure a sample DB for our api.
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MadeOfTech.SmartAPI.UnitTests.Sql.CreateCRUDDb.sql"))
                using (StreamReader reader = new StreamReader(stream))
                using (var sqliteConnection = new SQLiteConnection("Data Source=cruddb.sqlite"))
                {
                    var init = reader.ReadToEnd();
                    sqliteConnection.Open();
                    sqliteConnection.Execute(init);
                    sqliteConnection.Close();
                }
            }

            _server = new TestServer(new WebHostBuilder()
.UseStartup<CRUDUnitTestsStartup>());

            Client = _server.CreateClient();
        }
        public void Dispose()
        {
            Console.WriteLine("~CRUDUnitTests()");
            _server.Dispose();
            File.Delete("cruddb.sqlite");
        }
        /*
        [Fact]
        public async void ReadAll()
        {
            var httpResponseMessage = await Client.GetAsync("/crudtests/testentities");
            Assert.Equal(System.Net.HttpStatusCode.OK, httpResponseMessage.StatusCode);
            var content = await httpResponseMessage.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<IEnumerable<testentityModel>>(content);
            Assert.Equal(2, data.Count());
        }
        */
        [Fact]
        public async void Insert()
        {
            var newentity = new testentityModel()
            {
                designation = "new entity",
                ts = DateTime.Now.ToString("yyyyMMddTHHmmssfffZ")
            };
            var postData = JsonConvert.SerializeObject(newentity, Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
            var httpContent = new StringContent(postData, Encoding.UTF8, "application/json");

#pragma warning disable IDE0059 // Unnecessary assignment of a value
            // required due to https://github.com/dotnet/aspnetcore/issues/18463
            var contentLenth = httpContent.Headers.ContentLength;
#pragma warning restore IDE0059 // Unnecessary assignment of a value

            var httpResponseMessage = await Client.PostAsync("/crudtests/testentities", httpContent);
            Assert.Equal(System.Net.HttpStatusCode.Created, httpResponseMessage.StatusCode);
            Assert.Equal("/crudtests/testentities/3", httpResponseMessage.Headers.Location.ToString());
            var content = await httpResponseMessage.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<testentityModel>(content);
            Assert.Equal(3, data.id);
            Assert.Equal(newentity.designation, data.designation);
            Assert.Equal(newentity.ts, data.ts);
        }
    }
}
