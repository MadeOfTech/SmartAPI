using MadeOfTech.SmartAPI.Data.Models;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Attribute = MadeOfTech.SmartAPI.Data.Models.Attribute;

namespace MadeOfTech.SmartAPI
{
    public static class OpenApiDocumentExtensions
    {
        public static OpenApiRequestBody WithMemberContent(this OpenApiRequestBody openApiRequestBody, Collection collection, Attribute[] collection_attributes, bool excludeKeyAttributes = false)
        {
            openApiRequestBody.Content = new Dictionary<string, OpenApiMediaType>();
            var type = GenerateMemberType(collection, collection_attributes, excludeKeyAttributes, true);
            openApiRequestBody.Content.Add("application/json", type);
            openApiRequestBody.Content.Add("application/xml", type);
            return openApiRequestBody;
        }

        public static OpenApiResponses WithCollectionReturnedSuccessResponse(this OpenApiResponses responses, Collection collection, Attribute[] collection_attributes)
        {
            var response = new OpenApiResponse();
            response.Description = collection.collectionname + " successfully returned.";
            response.Content = new Dictionary<string, OpenApiMediaType>();
            var arrtype = GenerateCollectionSchema(collection, collection_attributes);

            response.Content.Add("application/json", arrtype);
            response.Content.Add("application/xml", arrtype);
            responses.Add("200", response);

            return responses;
        }

        public static OpenApiResponses WithMemberReturnedSuccessResponse(this OpenApiResponses responses, Collection collection, Attribute[] collection_attributes)
        {
            var response = new OpenApiResponse();
            response.Description = collection.membername + " successfully returned.";
            response.Content = new Dictionary<string, OpenApiMediaType>();
            var type = GenerateMemberType(collection, collection_attributes);

            response.Content.Add("application/json", type);
            response.Content.Add("application/xml", type);
            responses.Add("200", response);

            return responses;
        }

        public static OpenApiResponses WithCreatedSuccessResponse(this OpenApiResponses responses, Collection collection)
        {
            responses.Add("201", new OpenApiResponse() { Description = collection.membername + " created." });
            return responses;
        }

        public static OpenApiResponses WithCreatedSuccessResponse(this OpenApiResponses responses, bool fillBodyWithMember, Collection collection, Attribute[] collection_attributes)
        {
            if (fillBodyWithMember)
            {
                var response = new OpenApiResponse();
                response.Description = collection.membername + " created.";
                response.Content = new Dictionary<string, OpenApiMediaType>();
                var type = GenerateMemberType(collection, collection_attributes);

                response.Content.Add("application/json", type);
                response.Content.Add("application/xml", type);
                responses.Add("201", response);

                return responses;
            }
            else
            {
                return responses.WithCreatedSuccessResponse(collection);
            }
        }

        public static OpenApiResponses WithDeletedSuccessResponse(this OpenApiResponses responses, Collection collection)
        {
            responses.Add("204", new OpenApiResponse() { Description = collection.membername + " successfully deleted." });
            return responses;
        }

        public static OpenApiResponses WithUpdatedSuccessResponse(this OpenApiResponses responses, Collection collection)
        {
            responses.Add("204", new OpenApiResponse() { Description = collection.membername + " successfully updated." });
            return responses;
        }

        public static OpenApiResponses WithUpdatedSuccessResponse(this OpenApiResponses responses, bool fillBodyWithMember, Collection collection, Attribute[] collection_attributes)
        {
            if (fillBodyWithMember)
            {
                var response = new OpenApiResponse();
                response.Description = collection.membername + " updated.";
                response.Content = new Dictionary<string, OpenApiMediaType>();
                var type = GenerateMemberType(collection, collection_attributes);

                response.Content.Add("application/json", type);
                response.Content.Add("application/xml", type);
                responses.Add("204", response);

                return responses;
            }
            else
            {
                return responses.WithUpdatedSuccessResponse(collection);
            }
        }

        public static OpenApiResponses WithUnchangedResponse(this OpenApiResponses responses, Collection collection)
        {
            responses.Add("304", new OpenApiResponse() { Description = collection.membername + " has not changed at all." });
            return responses;
        }

        public static OpenApiResponses WithBadRequestResponse(this OpenApiResponses responses, Collection collection, IEnumerable<Attribute> attributes)
        {
            if (attributes.Select(x => x.fiqlkeyindex.HasValue).Count() > 0)
            {
                responses.Add("400", new OpenApiResponse() { Description = "the request is not correctly formatted." });
            }
            return responses;
        }

        public static OpenApiResponses WithUnauthorizedErrorResponse(this OpenApiResponses responses)
        {
            responses.Add("401", new OpenApiResponse() { Description = "request is not authorized, check your credentials." });
            return responses;
        }

        public static OpenApiResponses WithForbiddenErrorResponse(this OpenApiResponses responses)
        {
            responses.Add("403", new OpenApiResponse() { Description = "request is forbidden, check your grants." });
            return responses;
        }

        public static OpenApiResponses WithNotFoundErrorResponse(this OpenApiResponses responses, Collection collection)
        {
            responses.Add("404", new OpenApiResponse() { Description = collection.membername + " not found." });
            return responses;
        }

        public static OpenApiResponses WithConstraintErrorResponse(this OpenApiResponses responses, Collection collection)
        {
            responses.Add("409", new OpenApiResponse() { Description = collection.membername + " has not been created because of a constraint that couldn't be validated.." });
            return responses;
        }

        public static OpenApiResponses WithInternalServerErrorResponse(this OpenApiResponses responses)
        {
            responses.Add("500", new OpenApiResponse() { Description = "an internal server error has been encountered. contact support." });
            return responses;
        }

        private static OpenApiMediaType GenerateCollectionSchema(
            Collection collection,
            Attribute[] collection_attributes)
        {
            var type = GenerateMemberType(collection, collection_attributes);

            var arrtype = new OpenApiMediaType();

            arrtype.Schema = new OpenApiSchema()
            {
                Items = type.Schema,
                Type = "array",
                Title = collection.collectionname,
            };
            arrtype.Schema.Items.Xml = type.Schema.Xml;

            arrtype.Schema.Xml = new OpenApiXml()
            {
                Name = collection.collectionname,
                Wrapped = true,
            };

            return arrtype;
        }

        private static OpenApiMediaType GenerateMemberType(
            Collection collection,
            Attribute[] collection_attributes,
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
                Wrapped = true
            };
            if (!String.IsNullOrEmpty(collection.description))
            {
                type.Schema.Description = collection.description;
            }

            foreach (var attribute in collection_attributes)
            {
                if (attribute.keyindex.HasValue && excludeKeyAttributes) continue;
                if (attribute.keyindex.HasValue && attribute.autovalue && excludeKeyAutoAttributes) continue;
                var attributeSchema = new OpenApiSchema()
                {
                    Title = attribute.attributename,
                    Type = attribute.type,
                    Format = attribute.format,
                    Nullable = attribute.nullable
                };
                if (!String.IsNullOrEmpty(attribute.description))
                {
                    attributeSchema.Description = attribute.description;
                }
                type.Schema.Properties.Add(attribute.attributename, attributeSchema);
            }
            return type;
        }
    }
}
