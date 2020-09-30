using MadeOfTech.SmartAPI;
using MadeOfTech.SmartAPI.DataAdapters;
using MadeOfTech.SmartAPI.Data.Models;
using MadeOfTech.SmartAPI.Db;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using Attribute = MadeOfTech.SmartAPI.Data.Models.Attribute;

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
            SmartAPIOptions options = new SmartAPIOptions();
            if (setupAction != null) setupAction.Invoke(options);

            List<Collection> collections = null;
            List<Attribute> attributes = null;

            var basePath = options.BasePath ?? "";
            if (basePath.Length > 0)
            {
                basePath = basePath.Trim('/');
                basePath = "/" + basePath + "/";
            }
            options.BasePath = basePath;

            using (var dbConnection = DBConnectionBuilder.Use(options.APIDb_ConnectionType, options.APIDb_ConnectionString))
            {
                var collectionDataAdapter = new CollectionDataAdapter(dbConnection);
                collections = collectionDataAdapter.getAll();

                var attributeDataAdapter = new AttributeDataAdapter(dbConnection);
                attributes = attributeDataAdapter.getAll();
            }

            var pipeline = endpoints
                .CreateApplicationBuilder()
                .UseSmartAPI()
                .Build();

            if (!String.IsNullOrEmpty(options.OpenAPIDocument_Path))
            {
                var generator = new SmartAPIDocumentGenerator(options, collections.ToArray(), attributes.ToArray());
                var document = generator.GetDocument();
                endpoints.MapGet(options.OpenAPIDocument_Path, pipeline).WithMetadata(document);
            }

            foreach (var collection in collections)
            {
                if (!string.IsNullOrEmpty(options.DataDb_ConnectionHost) && collection.connectionstring.Contains("{host}"))
                {
                    collection.connectionstring = collection.connectionstring.Replace("{host}", options.DataDb_ConnectionHost);
                }
                if (!string.IsNullOrEmpty(options.DataDb_ConnectionUser) && collection.connectionstring.Contains("{user}"))
                {
                    collection.connectionstring = collection.connectionstring.Replace("{user}", options.DataDb_ConnectionUser);
                }
                if (!string.IsNullOrEmpty(options.DataDb_ConnectionPwd) && collection.connectionstring.Contains("{pwd}"))
                {
                    collection.connectionstring = collection.connectionstring.Replace("{pwd}", options.DataDb_ConnectionPwd);
                }

                List<Attribute> collection_attribute_list = new List<Attribute>();
                foreach (var attribute in attributes)
                {
                    if (attribute.collection_id == collection.id)
                    {
                        collection_attribute_list.Add(attribute);
                    }
                }
                var collection_attributes = collection_attribute_list.ToArray();

                if (collection_attributes.Count() > 0)
                {
                    if (collection.publish_getcollection) endpoints.MapGet(basePath + collection.collectionname, pipeline).WithSmartAPIMetadata(options, SmartAPIEndpointMetadata.EndpointOperation.GetCollection, collection, collection_attributes).WithAuthentication(options, "get_" + collection.collectionname).WithCors(options);
                    if (collection.publish_getmember) endpoints.MapGet(basePath + collection.collectionname + "/{id}", pipeline).WithSmartAPIMetadata(options, SmartAPIEndpointMetadata.EndpointOperation.GetMember, collection, collection_attributes).WithAuthentication(options, "get_" + collection.membername).WithCors(options);

                    if (collection.publish_postmember) endpoints.MapPost(basePath + collection.collectionname, pipeline).WithSmartAPIMetadata(options, SmartAPIEndpointMetadata.EndpointOperation.PostMember, collection, collection_attributes).WithAuthentication(options, "post_" + collection.membername).WithCors(options);
                    if (collection.publish_putmember) endpoints.MapPut(basePath + collection.collectionname + "/{id}", pipeline).WithSmartAPIMetadata(options, SmartAPIEndpointMetadata.EndpointOperation.PutMember, collection, collection_attributes).WithAuthentication(options, "put_" + collection.membername).WithCors(options);
                    if (collection.publish_deletemember) endpoints.MapDelete(basePath + collection.collectionname + "/{id}", pipeline).WithSmartAPIMetadata(options, SmartAPIEndpointMetadata.EndpointOperation.DeleteMember, collection, collection_attributes).WithAuthentication(options, "delete_" + collection.membername).WithCors(options);

                    //endpoints.MapGet(collection.url + "/{id}/{attribute_name}", async context => { await getAttributeRouter.HandlerAsync(context, collection, collection_attributes); });
                }
            }

            return endpoints;
        }

        private static TBuilder WithSmartAPIMetadata<TBuilder>(this TBuilder builder, SmartAPIOptions options, SmartAPIEndpointMetadata.EndpointOperation operation, Collection collection, Attribute[] attributes) where TBuilder : IEndpointConventionBuilder
        {
            builder.WithMetadata(new SmartAPIEndpointMetadata(options, operation, collection, attributes));
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
    }
}
