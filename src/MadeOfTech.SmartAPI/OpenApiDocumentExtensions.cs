﻿using MadeOfTech.SmartAPI.Data.Models;
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
        public static OpenApiRequestBody WithMemberContent(this OpenApiRequestBody openApiRequestBody, Collection collection, bool excludeKeyAttributes = false)
        {
            openApiRequestBody.Content = new Dictionary<string, OpenApiMediaType>();
            var type = GenerateMemberType(collection, excludeKeyAttributes, true);
            openApiRequestBody.Content.Add("application/json", type);
            openApiRequestBody.Content.Add("application/xml", type);
            return openApiRequestBody;
        }

        public static OpenApiRequestBody WithPatchContent(this OpenApiRequestBody openApiRequestBody, Collection collection)
        {
            openApiRequestBody.Content = new Dictionary<string, OpenApiMediaType>();
            var type = GeneratePatchOperationsType(collection);
            openApiRequestBody.Content.Add("application/json-patch+json", type);
            openApiRequestBody.Content.Add("application/json-patch+xml", type);
            return openApiRequestBody;
        }

        public static OpenApiResponses WithCollectionReturnedSuccessResponse(this OpenApiResponses responses, Collection collection)
        {
            var response = new OpenApiResponse();
            response.Description = collection.collectionname + " successfully returned.";
            response.Content = new Dictionary<string, OpenApiMediaType>();
            var arrtype = GenerateCollectionSchema(collection);

            response.Content.Add("application/json", arrtype);
            response.Content.Add("application/xml", arrtype);
            responses.Add("200", response);

            return responses;
        }

        public static OpenApiResponses WithMemberReturnedSuccessResponse(this OpenApiResponses responses, Collection collection)
        {
            var response = new OpenApiResponse();
            response.Description = collection.membername + " successfully returned.";
            response.Content = new Dictionary<string, OpenApiMediaType>();
            var type = GenerateMemberType(collection);

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

        public static OpenApiResponses WithCreatedSuccessResponse(this OpenApiResponses responses, bool fillBodyWithMember, Collection collection)
        {
            if (fillBodyWithMember)
            {
                var response = new OpenApiResponse();
                response.Description = collection.membername + " created.";
                response.Content = new Dictionary<string, OpenApiMediaType>();
                var type = GenerateMemberType(collection);

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

        public static OpenApiResponses WithUpdatedSuccessResponse(this OpenApiResponses responses, bool fillBodyWithMember, Collection collection)
        {
            if (fillBodyWithMember)
            {
                var response = new OpenApiResponse();
                response.Description = collection.membername + " updated.";
                response.Content = new Dictionary<string, OpenApiMediaType>();
                var type = GenerateMemberType(collection);

                response.Content.Add("application/json", type);
                response.Content.Add("application/xml", type);
                responses.Add("200", response);

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

        public static OpenApiResponses WithBadRequestResponse(this OpenApiResponses responses, Collection collection)
        {
            if (collection.attributes.Select(x => x.fiqlkeyindex.HasValue).Count() > 0)
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
            responses.Add("409", new OpenApiResponse() { Description = collection.membername + " has not been created because of a constraint that couldn't be validated." });
            return responses;
        }

        public static OpenApiResponses WithUnprocessableEntityErrorResponse(this OpenApiResponses responses)
        {
            responses.Add("422", new OpenApiResponse() { Description = "request is well-formed but given data can't be processed correctly." });
            return responses;
        }

        public static OpenApiResponses WithInternalServerErrorResponse(this OpenApiResponses responses)
        {
            responses.Add("500", new OpenApiResponse() { Description = "an internal server error has been encountered. contact support." });
            return responses;
        }

        private static OpenApiMediaType GenerateCollectionSchema(
            Collection collection)
        {
            var type = GenerateMemberType(collection);

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

            foreach (var attribute in collection.attributes)
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

        private static OpenApiMediaType GeneratePatchOperationsType(
            Collection collection)
        {
            #region operation
            var type = new OpenApiMediaType();
            type.Schema = new OpenApiSchema();
            type.Schema.Type = "object";
            type.Schema.Title = "operation";
            type.Schema.Xml = new OpenApiXml()
            {
                Name = "operation",
                Wrapped = true
            };
            type.Schema.Description = "operation is a json-patch operation, compliant to RFC6902 (https://datatracker.ietf.org/doc/html/rfc6902)";
            type.Schema.Properties.Add("op", new OpenApiSchema()
            {
                Title = "op",
                Type = "string",
                Format = null,
                Nullable = false
            });
            type.Schema.Properties.Add("path", new OpenApiSchema()
            {
                Title = "path",
                Type = "string",
                Format = null,
                Nullable = false
            });
            type.Schema.Properties.Add("value", new OpenApiSchema()
            {
                Title = "op",
                Type = "string",
                Format = null,
                Nullable = true
            });
            #endregion

            #region operations
            var arrtype = new OpenApiMediaType();

            arrtype.Schema = new OpenApiSchema()
            {
                Items = type.Schema,
                Type = "array",
                Title = "operations",
            };
            arrtype.Schema.Items.Xml = type.Schema.Xml;

            arrtype.Schema.Xml = new OpenApiXml()
            {
                Name = "operations",
                Wrapped = true,
            };
            #endregion

            return arrtype;
        }
    }
}
