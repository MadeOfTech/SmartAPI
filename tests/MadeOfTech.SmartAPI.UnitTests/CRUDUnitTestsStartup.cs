using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MadeOfTech.SmartAPI.UnitTests
{
    public class CRUDUnitTestsStartup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole()).AddLogging(configure => configure.AddDebug());
                /*.AddTransient<CRUDUnitTestsStartup>();*/

            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/crudtests/swagger/swagger.json", "CRUD API");
                c.RoutePrefix = "crudtests/swagger";
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                string API_JsonDescription = "";
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MadeOfTech.SmartAPI.UnitTests.CRUDAPI.json"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    API_JsonDescription = reader.ReadToEnd();
                }
                
                endpoints.MapSmartAPI(options =>
                {
                    options.API_JsonDescription = API_JsonDescription;
                    options.APIDb_APIDesignation = "CRUD API";
                    options.OpenAPIDocument_Path = "swagger/swagger.json";
                    options.Upsert_FillBodyWithMember = true;
                });
            });
        }
    }
}
