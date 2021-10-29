using MadeOfTech.SmartAPI.Data.Models;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;
using Attribute = MadeOfTech.SmartAPI.Data.Models.Attribute;

namespace MadeOfTech.SmartAPI
{
    public class SmartAPIDocumentGenerator
    {
        private API _api;
        private Collection[] _collections;
        private SmartAPIOptions _options;

        public SmartAPIDocumentGenerator(SmartAPIOptions options, API api)
        {
            _options = options;
            _api = api;
            var allCollections = new List<Collection>();
            foreach (var db in api.dbs)
            {
                allCollections.AddRange(db.collections);
            }
            _collections = allCollections.ToArray();
        }

        public OpenApiDocument GetDocument()
        {
            var collections = _collections;

            var swaggerDoc = new OpenApiDocument();
            swaggerDoc.Info = new OpenApiInfo() { Title = _api.designation, Description = _api.description, Version = _options.Version };

            swaggerDoc.Servers.Add(new OpenApiServer() { Description = _api.designation + " server", Url = _options.BasePath });

            var paths = new OpenApiPaths();

            foreach (var collection in collections)
            {
                swaggerDoc.Tags.Add(new OpenApiTag()
                {
                    Name = collection.collectionname,
                    Description = collection.description,
                });

                //var collection_attributes = new List<Attribute>();
                var collection_keyattributes = new SortedList<int, Data.Models.Attribute>();

                var openApiParameters = new List<OpenApiParameter>();
                foreach (var attribute in collection.attributes)
                {
                    if (attribute.keyindex.HasValue)
                    {
                        collection_keyattributes.Add(attribute.keyindex.Value, attribute);
                        openApiParameters.Add(new OpenApiParameter() { Name = attribute.attributename, In = ParameterLocation.Path });
                    }
                }

                if (collection.attributes.Count() <= 0) continue;

                List<OpenApiParameter> fiqlParameters = null;
                if (collection.attributes.Where(x => x.fiqlkeyindex.HasValue).Count() > 0)
                {
                    fiqlParameters = new List<OpenApiParameter>();
                    string fiqlDescription = @"query is the place where collection can be requested with criterias. ";
                    fiqlDescription += "If used, the query has to be compliant with FIQL specification (https://tools.ietf.org/html/draft-nottingham-atompub-fiql-00).\n";
                    fiqlDescription += "Only the following attributes can be requested via the query : ";
                    fiqlDescription += collection.attributes.Where(x => x.fiqlkeyindex.HasValue).OrderBy(x => x.fiqlkeyindex.Value).Aggregate("", (current, next) => current + ", " + next.attributename).Remove(0, 2);
                    fiqlDescription += ".";
                    fiqlParameters.Add(new OpenApiParameter() { Name = "query", In = ParameterLocation.Query, Description = fiqlDescription, Required = false });
                }

                string idDescription = "";
                for (int index = 0; index < collection_keyattributes.Count; index++)
                {
                    if (idDescription.Length > 0) idDescription += ",";
                    idDescription += "{" + collection_keyattributes.Values[index].attributename + "}";
                }
                if (collection_keyattributes.Count > 1) idDescription = "(" + idDescription + ")";

                var collectionPathItem = new OpenApiPathItem();
                var memberPathItem = new OpenApiPathItem();

                if (collection.publish_getcollection)
                {
                    collectionPathItem.AddOperation(OperationType.Get, new OpenApiOperation()
                    {
                        Description = "Returns a collection of " + collection.collectionname + ".",
                        Parameters = fiqlParameters,
                        OperationId = "get_" + collection.collectionname,
                        Tags = new List<OpenApiTag> { new OpenApiTag() { Name = collection.collectionname } },
                        Responses = new OpenApiResponses()
                            .WithCollectionReturnedSuccessResponse(collection)
                            .WithBadRequestResponse(collection)
                            .WithUnauthorizedErrorResponse()
                            .WithForbiddenErrorResponse()
                            .WithUnprocessableEntityErrorResponse()
                            .WithInternalServerErrorResponse()
                    }); ;
                }

                if (collection.publish_getmember)
                {
                    memberPathItem.AddOperation(OperationType.Get, new OpenApiOperation()
                    {
                        Description = "Returns a member of the collection of " + collection.collectionname + ".",
                        Parameters = openApiParameters,
                        Tags = new List<OpenApiTag> { new OpenApiTag() { Name = collection.collectionname } },
                        OperationId = "get_" + collection.membername,
                        Responses = new OpenApiResponses()
                            .WithMemberReturnedSuccessResponse(collection)
                            .WithNotFoundErrorResponse(collection)
                            .WithUnauthorizedErrorResponse()
                            .WithForbiddenErrorResponse()
                            .WithUnprocessableEntityErrorResponse()
                            .WithInternalServerErrorResponse()
                    });
                }

                if (collection.publish_postmember)
                {
                    collectionPathItem.AddOperation(OperationType.Post, new OpenApiOperation()
                    {
                        Description = "Post a new " + collection.membername + ".",
                        OperationId = "post_" + collection.collectionname,
                        Tags = new List<OpenApiTag> { new OpenApiTag() { Name = collection.collectionname } },
                        RequestBody = new OpenApiRequestBody().WithMemberContent(collection, false),
                        Responses = new OpenApiResponses()
                            .WithCreatedSuccessResponse(_options.Upsert_FillBodyWithMember, collection)
                            .WithConstraintErrorResponse(collection)
                            .WithUnauthorizedErrorResponse()
                            .WithForbiddenErrorResponse()
                            .WithUnprocessableEntityErrorResponse()
                            .WithInternalServerErrorResponse(),
                    });
                }

                if (collection.publish_putmember)
                {
                    memberPathItem.AddOperation(OperationType.Put, new OpenApiOperation()
                    {
                        Description = "Put (upsert operation) a " + collection.membername + " in the collection " + collection.collectionname + ".",
                        Parameters = openApiParameters,
                        OperationId = "put_" + collection.collectionname,
                        Tags = new List<OpenApiTag> { new OpenApiTag() { Name = collection.collectionname } },
                        RequestBody = new OpenApiRequestBody().WithMemberContent(collection, true),
                        Responses = new OpenApiResponses()
                            .WithCreatedSuccessResponse(_options.Upsert_FillBodyWithMember, collection)
                            .WithUpdatedSuccessResponse(_options.Upsert_FillBodyWithMember, collection)
                            .WithUnchangedResponse(collection)
                            .WithUnauthorizedErrorResponse()
                            .WithForbiddenErrorResponse()
                            .WithUnprocessableEntityErrorResponse()
                            .WithInternalServerErrorResponse(),
                    });
                }

                if (collection.publish_patchmember)
                {
                    memberPathItem.AddOperation(OperationType.Patch, new OpenApiOperation()
                    {
                        Description = "Patch (update operation) a " + collection.membername + " in the collection " + collection.collectionname + ".",
                        Parameters = openApiParameters,
                        OperationId = "patch_" + collection.collectionname,
                        Tags = new List<OpenApiTag> { new OpenApiTag() { Name = collection.collectionname } },
                        RequestBody = new OpenApiRequestBody().WithPatchContent(collection),
                        Responses = new OpenApiResponses()
                            .WithUpdatedSuccessResponse(collection)
                            .WithUnchangedResponse(collection)
                            .WithUnauthorizedErrorResponse()
                            .WithForbiddenErrorResponse()
                            .WithUnprocessableEntityErrorResponse()
                            .WithInternalServerErrorResponse(),
                    });
                }

                if (collection.publish_deletemember)
                {
                    memberPathItem.AddOperation(OperationType.Delete, new OpenApiOperation()
                    {
                        Description = "Delete a member of the collection " + collection.collectionname + ".",
                        Parameters = openApiParameters,
                        Tags = new List<OpenApiTag> { new OpenApiTag() { Name = collection.collectionname } },
                        OperationId = "delete_" + collection.membername,
                        Responses = new OpenApiResponses()
                            .WithDeletedSuccessResponse(collection)
                            .WithUnauthorizedErrorResponse()
                            .WithForbiddenErrorResponse()
                            .WithUnprocessableEntityErrorResponse()
                            .WithInternalServerErrorResponse(),
                    });
                }

                paths.Add("/" + collection.collectionname, collectionPathItem);
                paths.Add("/" + collection.collectionname + "/" + idDescription, memberPathItem);
            }

            swaggerDoc.Paths = paths;
            return swaggerDoc;
        }

        public OpenApiMediaType GenerateMemberType(
            Collection collection,
            List<Attribute> collection_attributes,
            bool excludeKeyAttributes = false,
            bool excludeKeyAutoAttributes = false)
        {
            var type = new OpenApiMediaType();
            type.Schema = new OpenApiSchema();
            type.Schema.Type = "object";
            type.Schema.Title = collection.membername;
            type.Schema.Xml = new OpenApiXml()
            {
                Name = collection.membername,
            };

            foreach (var attribute in collection_attributes)
            {
                if (attribute.keyindex.HasValue && excludeKeyAttributes) continue;
                if (attribute.keyindex.HasValue && attribute.autovalue && excludeKeyAutoAttributes) continue;
                type.Schema.Properties.Add(attribute.attributename, new OpenApiSchema() { Title = attribute.attributename, Type = attribute.type, Format = attribute.format, Nullable = attribute.nullable });
            }
            return type;
        }

        public OpenApiMediaType GenerateCollectionSchema(
            Collection collection,
            List<Attribute> collection_attributes)
        {
            var type = GenerateMemberType(collection, collection_attributes);

            var arrtype = new OpenApiMediaType();
            arrtype.Schema = new OpenApiSchema();
            arrtype.Schema.Items = type.Schema;
            arrtype.Schema.Items.Xml = type.Schema.Xml;
            arrtype.Schema.Type = "array";
            arrtype.Schema.Title = collection.collectionname;
            type.Schema.Xml = new OpenApiXml()
            {
                Name = collection.collectionname,
            };

            return arrtype;
        }

    }
}
