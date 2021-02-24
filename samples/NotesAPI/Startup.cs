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
using ZNetCS.AspNetCore.Authentication.Basic;

namespace NotesAPI
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(configure =>
            {
                configure.AddPolicy("access", policy =>
                {
                    policy.AuthenticationSchemes.Add(BasicAuthenticationDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });
            });

            services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
            .AddBasicAuthentication(options =>
            {
                options.Realm = "NotesAPI v1";
                options.Events = new BasicAuthenticationHandler();
            });
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
                c.SwaggerEndpoint("/notesapi/v1/swagger/swagger.json", "Notes API V1");
                c.RoutePrefix = "notesapi/v1/swagger";
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                string API_JsonDescription = "";
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NotesAPI.NotesAPI.json"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    API_JsonDescription = reader.ReadToEnd();
                }
                
                endpoints.MapSmartAPI(options =>
                {
                    /*options.APIDb_ConnectionType = "sqlite";
                    options.APIDb_ConnectionString = "Data Source=apidb.sqlite";*/
                    options.API_JsonDescription = API_JsonDescription;
                    options.APIDb_APIDesignation = "Notes API v1";
                    options.BasePath = "notesapi/v1/";
                    options.Authentication_RequireAuthentication = true;
                    options.Authentication_GlobalModifyPolicyName = "access";
                    options.OpenAPIDocument_Path = "swagger/swagger.json";
                    options.InjectAttribute_Name = "member_id";
                    options.InjectAttribute_ValueEvaluator = context => {
                        return Int32.Parse(context.User.FindFirst(BasicAuthenticationHandler.ClaimType_MemberId).Value);
                    };
                    options.Upsert_FillBodyWithMember = true;
                });

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
