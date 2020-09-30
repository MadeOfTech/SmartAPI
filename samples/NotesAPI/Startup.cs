using System;
using System.Collections.Generic;
using System.Linq;
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
                c.SwaggerEndpoint("/notesapi/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = "notesapi/swagger";
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSmartAPI(options =>
                {
                    options.APIDb_ConnectionType = "sqlite";
                    options.APIDb_ConnectionString = "Data Source=apidb.sqlite";
                    options.BasePath = "notesapi/";
                    options.Authentication_RequireAuthentication = true;
                    options.Authentication_GlobalModifyPolicyName = "access";
                    options.OpenAPIDocument_Path = "/notesapi/swagger/v1/swagger.json";
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
