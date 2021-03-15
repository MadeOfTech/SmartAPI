using MadeOfTech.SmartAPI;
using MadeOfTech.SmartAPI.DataAdapters;
using MadeOfTech.SmartAPI.Data.Models;
using MadeOfTech.SmartAPI.Db;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using Attribute = MadeOfTech.SmartAPI.Data.Models.Attribute;
using MadeOfTech.SmartAPI.Exceptions;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Microsoft.AspNetCore.Builder
{
    public static class SmartAPIBuilderExtensions
    {
        public static IApplicationBuilder UseSmartAPI(
            this IApplicationBuilder app)
        {
            app.UseMiddleware<SmartAPIMiddleware>();
            return app;
        }

        public static IEndpointRouteBuilder MapSmartAPI(
            this IEndpointRouteBuilder endpoints,
            Action<SmartAPIOptions> setupAction = null)
        {
            var logger = endpoints.ServiceProvider.GetService<ILogger<SmartAPIMiddleware>>();

            SmartAPIOptions options = new SmartAPIOptions();
            if (setupAction != null) setupAction.Invoke(options);

            API api = null;

            if (null != options.APIDb_ConnectionType && null != options.APIDb_ConnectionString && null == options.API_JsonDescription)
            {
                using (var dbConnection = DBConnectionBuilder.Use(options.APIDb_ConnectionType, options.APIDb_ConnectionString))
                {
                    var apiDataAdapter = new APIDataAdapter(dbConnection);
                    var apis = apiDataAdapter.getAll(options.APIDb_APIDesignation);
                    if (apis.Count() != 1)
                    {
                        throw new SmartAPIException("No API definition has been found in the database.");
                    }
                    api = apis.First();

                    var dbDataAdapter = new DbDataAdapter(dbConnection);
                    var dbs = dbDataAdapter.getAll(options.APIDb_APIDesignation);
                    if (dbs.Count() < 1)
                    {
                        throw new SmartAPIException("No Db definition has been found in the database.");
                    }
                    api.dbs = dbs.ToArray();

                    var collectionDataAdapter = new CollectionDataAdapter(dbConnection);
                    var collections = collectionDataAdapter.getAll(options.APIDb_APIDesignation);
                    if (collections.Count() <= 1)
                    {
                        throw new SmartAPIException("No Collection definition has been found in the database.");
                    }
                    foreach (var db in api.dbs)
                    {
                        db.api = api;

                        if (!string.IsNullOrEmpty(options.DataDb_ConnectionHost) && db.connectionstring.Contains("{host}"))
                        {
                            db.connectionstring = db.connectionstring.Replace("{host}", options.DataDb_ConnectionHost);
                        }
                        if (!string.IsNullOrEmpty(options.DataDb_ConnectionUser) && db.connectionstring.Contains("{user}"))
                        {
                            db.connectionstring = db.connectionstring.Replace("{user}", options.DataDb_ConnectionUser);
                        }
                        if (!string.IsNullOrEmpty(options.DataDb_ConnectionPwd) && db.connectionstring.Contains("{pwd}"))
                        {
                            db.connectionstring = db.connectionstring.Replace("{pwd}", options.DataDb_ConnectionPwd);
                        }

                        var db_collection_list = new List<Collection>();
                        foreach (var collection in collections)
                        {
                            if (collection.db_id.Value == db.id.Value)
                            {
                                db_collection_list.Add(collection);
                                collection.db = db;
                            }
                        }
                        db.collections = db_collection_list.ToArray();
                    }

                    var attributeDataAdapter = new AttributeDataAdapter(dbConnection);
                    var attributes = attributeDataAdapter.getAll(options.APIDb_APIDesignation);

                    foreach (var collection in collections)
                    {
                        var collection_attribute_list = new List<Attribute>();
                        foreach (var attribute in attributes)
                        {
                            if (attribute.collection_id == collection.id.Value)
                            {
                                collection_attribute_list.Add(attribute);
                            }
                        }
                        collection.attributes = collection_attribute_list.ToArray();
                    }
                }
            }
            else if (null != options.API_JsonDescription)
            {
                api = JsonConvert.DeserializeObject<API>(options.API_JsonDescription);
                if (null == api) throw new SmartAPIException("No API definition has been found in the json document.");
                if (null == api.dbs || api.dbs.Count() == 0) throw new SmartAPIException("No DB definition has been found in the api.");
                foreach (var db in api.dbs)
                {
                    db.api = api;
                    if (null == db.collections) throw new SmartAPIException("No Collection definition has been found in the db " + db.designation + ".");

                    if (!string.IsNullOrEmpty(options.DataDb_ConnectionHost) && db.connectionstring.Contains("{host}"))
                    {
                        db.connectionstring = db.connectionstring.Replace("{host}", options.DataDb_ConnectionHost);
                    }
                    if (!string.IsNullOrEmpty(options.DataDb_ConnectionUser) && db.connectionstring.Contains("{user}"))
                    {
                        db.connectionstring = db.connectionstring.Replace("{user}", options.DataDb_ConnectionUser);
                    }
                    if (!string.IsNullOrEmpty(options.DataDb_ConnectionPwd) && db.connectionstring.Contains("{pwd}"))
                    {
                        db.connectionstring = db.connectionstring.Replace("{pwd}", options.DataDb_ConnectionPwd);
                    }

                    foreach (var collection in db.collections)
                    {
                        collection.db = db;
                    }
                }
            }

            if (string.IsNullOrEmpty(options.BasePath) && !string.IsNullOrEmpty(api.basepath))
            {
                options.BasePath = api.basepath;
            }

            var basePath = options.BasePath ?? "";
            if (basePath.Length > 0)
            {
                basePath = basePath.Trim('/');
                basePath = "/" + basePath + "/";
            }
            else
            {
                basePath = "/";
            }
            options.BasePath = basePath;

            if (string.IsNullOrEmpty(options.Version) && !string.IsNullOrEmpty(api.version))
            {
                options.Version = api.version;
            }

            var pipeline = endpoints
                .CreateApplicationBuilder()
                .UseSmartAPI()
                .Build();

            if (!String.IsNullOrEmpty(options.OpenAPIDocument_Path))
            {
                var generator = new SmartAPIDocumentGenerator(options, api);
                var document = generator.GetDocument();
                endpoints.MapGet(options.BasePath + options.OpenAPIDocument_Path, pipeline).WithSmartAPIDocumentationMetadata(options, document);
                logger.LogInformation("Swagger document available at : " + options.BasePath + options.OpenAPIDocument_Path);
            }

            foreach (var collection in api.collections)
            {
                VerifyCollectionDefinition(collection);

                if (collection.attributes.Count() > 0)
                {
                    if (collection.publish_getcollection)
                    {
                        endpoints.MapGet(basePath + collection.collectionname, pipeline).WithSmartAPIMetadata(options, SmartAPIEndpointMetadata.EndpointOperation.GetCollection, collection).WithAuthentication(options, "get_" + collection.collectionname).WithCors(options);
                        logger.LogInformation("Collection selection available at GET : " + basePath + collection.collectionname);
                    }

                    if (collection.publish_getmember)
                    {
                        endpoints.MapGet(basePath + collection.collectionname + "/{id}", pipeline).WithSmartAPIMetadata(options, SmartAPIEndpointMetadata.EndpointOperation.GetMember, collection).WithAuthentication(options, "get_" + collection.membername).WithCors(options);
                        logger.LogInformation("Member selection available at GET : " + basePath + collection.collectionname + "/{id}");
                    }

                    if (collection.publish_postmember)
                    {
                        endpoints.MapPost(basePath + collection.collectionname, pipeline).WithSmartAPIMetadata(options, SmartAPIEndpointMetadata.EndpointOperation.PostMember, collection).WithAuthentication(options, "post_" + collection.membername).WithCors(options);
                        logger.LogInformation("Member creation available at POST : " + basePath + collection.collectionname);
                    }

                    if (collection.publish_putmember)
                    {
                        endpoints.MapPut(basePath + collection.collectionname + "/{id}", pipeline).WithSmartAPIMetadata(options, SmartAPIEndpointMetadata.EndpointOperation.PutMember, collection).WithAuthentication(options, "put_" + collection.membername).WithCors(options);
                        logger.LogInformation("Member upsert available at PUT : " + basePath + collection.collectionname + "/{id}");
                    }

                    if (collection.publish_deletemember)
                    {
                        endpoints.MapDelete(basePath + collection.collectionname + "/{id}", pipeline).WithSmartAPIMetadata(options, SmartAPIEndpointMetadata.EndpointOperation.DeleteMember, collection).WithAuthentication(options, "delete_" + collection.membername).WithCors(options);
                        logger.LogInformation("Member deletion available at DELETE : " + basePath + collection.collectionname + "/{id}");
                    }

                    //endpoints.MapGet(collection.url + "/{id}/{attribute_name}", async context => { await getAttributeRouter.HandlerAsync(context, collection, collection_attributes); });
                }
                else
                {
                    logger.LogWarning("No endpoint available for collection " + collection.collectionname + " : no attribute has been found.");
                }
            }

            return endpoints;
        }

        private static TBuilder WithSmartAPIMetadata<TBuilder>(this TBuilder builder, SmartAPIOptions options, SmartAPIEndpointMetadata.EndpointOperation operation, Collection collection) where TBuilder : IEndpointConventionBuilder
        {
            builder.WithMetadata(new SmartAPIEndpointMetadata(options, operation, collection));
            return builder;
        }

        private static TBuilder WithSmartAPIDocumentationMetadata<TBuilder>(this TBuilder builder, SmartAPIOptions options, OpenApiDocument document) where TBuilder : IEndpointConventionBuilder
        {
            builder.WithMetadata(new SmartAPIDocumentationMetadata(options, document));
            return builder;
        }

        private static TBuilder WithAuthentication<TBuilder>(this TBuilder builder, SmartAPIOptions options, string operationName) where TBuilder : IEndpointConventionBuilder
        {
            if (options.Authentication_RequireAuthentication)
            {
                List<string> policyNames = new List<string>();
                if (options.Authentication_UsePerOperationPolicy)
                {
                    policyNames.Add(operationName);
                }

                if (!string.IsNullOrEmpty(options.Authentication_GlobalReadPolicyName) && operationName.StartsWith("get"))
                {
                    policyNames.Add(options.Authentication_GlobalReadPolicyName);
                }

                if (!string.IsNullOrEmpty(options.Authentication_GlobalModifyPolicyName) && !operationName.StartsWith("get"))
                {
                    policyNames.Add(options.Authentication_GlobalModifyPolicyName);
                }

                builder.RequireAuthorization(policyNames.ToArray());
            }

            return builder;
        }

        private static TBuilder WithCors<TBuilder>(this TBuilder builder, SmartAPIOptions options) where TBuilder : IEndpointConventionBuilder
        {
            if (options.Cors_RequireCors && !string.IsNullOrEmpty(options.Cors_PolicyName))
            {
                builder.RequireCors(options.Cors_PolicyName);
            }

            return builder;
        }

        private static void VerifyCollectionDefinition(Collection collection)
        {
            var keyAttributes = collection.attributes.Where(a => { return a.keyindex.HasValue; }).OrderBy(a => { return a.keyindex.Value; });

            int keyIndex = 1;
            foreach (var attribute in keyAttributes)
            {
                if (attribute.keyindex.Value != keyIndex) throw new SmartAPIException("The key index of attribute " + attribute.attributename + " is not correctly defined for collection " + collection.collectionname + ". Indexes are 1-based and must follow one by one.");
                keyIndex++;
            }

            var fiqlAttributes = collection.attributes.Where(a => { return a.fiqlkeyindex.HasValue; }).OrderBy(a => { return a.fiqlkeyindex.Value; });

            int fiqlIndex = 1;
            foreach (var attribute in fiqlAttributes)
            {
                if (attribute.fiqlkeyindex.Value != fiqlIndex) throw new SmartAPIException("The fiql index of attribute " + attribute.attributename + " is not correctly defined for collection " + collection.collectionname + ". Indexes are 1-based and must follow one by one.");
                fiqlIndex++;
            }
        }
    }
}
