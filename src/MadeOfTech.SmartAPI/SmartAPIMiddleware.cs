using MadeOfTech.SmartAPI.DataAdapters;
using MadeOfTech.SmartAPI.Operations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI
{
    public class SmartAPIMiddleware
    {
        private readonly RequestDelegate _next;

        public SmartAPIMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var endpointMetadata = httpContext.GetEndpoint().Metadata.GetMetadata<SmartAPIEndpointMetadata>();
            if (endpointMetadata != null)
            {
                OperationBase operation = null;
                switch (endpointMetadata.Operation)
                {
                    case SmartAPIEndpointMetadata.EndpointOperation.GetCollection:
                        operation = new GetCollectionOperation();
                        break;

                    case SmartAPIEndpointMetadata.EndpointOperation.GetMember:
                        operation = new GetMemberOperation();
                        break;

                    case SmartAPIEndpointMetadata.EndpointOperation.PostMember:
                        operation = new PostMemberOperation();
                        break;

                    case SmartAPIEndpointMetadata.EndpointOperation.PutMember:
                        operation = new PutMemberOperation();
                        break;

                    case SmartAPIEndpointMetadata.EndpointOperation.DeleteMember:
                        operation = new DeleteMemberOperation();
                        break;
                }

                if (null != operation)
                {
                    if (endpointMetadata.Options.FillHeaderXPoweredBy) httpContext.Response.Headers.Add("X-Powered-By", "SmartAPI - Visit https://smartapi.madeoftech.com");
                    string InjectAttribute_Name = null;
                    object InjectAttribute_Value = null;
                    if (!string.IsNullOrEmpty(endpointMetadata.Options.InjectAttribute_Name) && (endpointMetadata.Options.InjectAttribute_ValueEvaluator != null))
                    {
                        InjectAttribute_Name = endpointMetadata.Options.InjectAttribute_Name;
                        InjectAttribute_Value = endpointMetadata.Options.InjectAttribute_ValueEvaluator(httpContext);
                    }
                    await operation.Invoke(httpContext, endpointMetadata.Collection, InjectAttribute_Name, InjectAttribute_Value);
                }
                else
                {
                    await _next(httpContext);
                }
            }
            else
            {
                var documentationMetadata = httpContext.GetEndpoint().Metadata.GetMetadata<SmartAPIDocumentationMetadata>();
                if (documentationMetadata != null)
                {
                    var outputString = documentationMetadata.Document.Serialize(documentationMetadata.Options.OpenAPIDocument_SpecVersion, OpenApiFormat.Json);
                    httpContext.Response.ContentType = "application/json";
                    await httpContext.Response.WriteAsync(outputString);
                }
                else
                {
                    await _next(httpContext);
                }
            }
        }
    }
}
