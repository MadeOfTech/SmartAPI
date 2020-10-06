using MadeOfTech.SmartAPI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Claims;
using System.Threading.Tasks;
using ZNetCS.AspNetCore.Authentication.Basic;

namespace QuotesAPI
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(configure =>
            {
                configure.AddPolicy("read_policy", policy =>
                {
                    //policy.AuthenticationSchemes.Add(BasicAuthenticationDefaults.AuthenticationScheme);
                    policy.RequireAssertion(context => { context.Succeed(null); return true; });
                });
            });

            services.AddAuthorization(configure =>
            {
                configure.AddPolicy("modify_policy", policy =>
                {
                    policy.AuthenticationSchemes.Add(BasicAuthenticationDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireUserName("admin");
                });
            });

            /* https://sandervandevelde.wordpress.com/2018/01/06/adding-basic-authentication-to-asp-net-core-the-right-way/ */
            /* https://github.com/msmolka/ZNetCS.AspNetCore.Authentication.Basic */
            services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
            .AddBasicAuthentication(options =>
            {
                options.Realm = "QuotesAPI v1";
                options.Events = new ZNetCS.AspNetCore.Authentication.Basic.Events.BasicAuthenticationEvents
                {
                    OnValidatePrincipal = context =>
                    {
                        if ((context.UserName.ToLower() == "admin") && (context.Password == "secret"))
                        {
                             var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, context.UserName, context.Options.ClaimsIssuer),
                            };

                            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                            context.Principal = principal;
                        }

                        return Task.CompletedTask;
                    }
                };
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
                c.SwaggerEndpoint("/quotesapi/v1/swagger/swagger.json", "QuotesAPI V1");
                c.RoutePrefix = "quotesapi/v1/swagger";
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
                    options.APIDb_APIDesignation = "Quotes API v1";
                    options.BasePath = "quotesapi/v1/";
                    options.Authentication_RequireAuthentication = true;
                    options.Authentication_GlobalReadPolicyName = "read_policy";
                    options.Authentication_GlobalModifyPolicyName = "modify_policy";
                    options.OpenAPIDocument_Path = "swagger/swagger.json";
                    options.Trigger_AfterOperation = async (context, collection, input, keys) =>
                    {
                        if (context.Request.Method == "POST" && collection.collectionname == "authors")
                        {
                            // A new author has been inserted.
                            string name = input["name"];
                            Console.WriteLine("A new author has been inserted : " + name);
                        }
                    };
                });
            });
        }
    }
}
