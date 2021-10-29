using MadeOfTech.SmartAPI.Data.Models;
using MadeOfTech.SmartAPI.DataAdapters;
using MadeOfTech.SmartAPI.Db;
using MadeOfTech.SmartAPI.Exceptions;
using MadeOfTech.SmartAPI.FIQL;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace MadeOfTech.SmartAPI.Operations
{
    public class PatchMemberOperation : OperationBase
    {
        public override async Task Invoke(HttpContext context, Collection collection, string injectAttributeName, object injectAttributeValue)
        {
            try
            {
                string format = context.Request.Headers.ContainsKey("Accept") ? context.Request.Headers["Accept"].ToString().ToLower() : "application/json";
                if ("application/json" == format)
                {
                    context.Response.ContentType = "application/json";
                }
                else if ("application/xml" == format)
                {
                    context.Response.ContentType = "application/xml";
                }

                var operations = await InputAsync(context);

                context.Response.StatusCode = 304;
                var connectionString = collection.db.connectionstring;
                using (var connection = DBConnectionBuilder.Use(collection.db.dbtype, collection.db.connectionstring))
                {
                    await connection.OpenAsync();
                    var transaction = await Task<IDbTransaction>.Run(() => { return connection.BeginTransaction(); });
                    foreach (var operation in operations)
                    {
                        if (String.IsNullOrEmpty(operation.op) || operation.op != "replace")
                        {
                            throw new HttpException(System.Net.HttpStatusCode.UnprocessableEntity, "operation is not supported");
                        }

                        if (String.IsNullOrEmpty(operation.path))
                        {
                            throw new HttpException(System.Net.HttpStatusCode.UnprocessableEntity, "path must be set");
                        }

                        if (!operation.path.StartsWith("/"))
                        {
                            throw new HttpException(System.Net.HttpStatusCode.UnprocessableEntity, "path must begin with /");
                        }

                        if (operation.path.Length <= 1)
                        {
                            throw new HttpException(System.Net.HttpStatusCode.UnprocessableEntity, "path must be set");
                        }

                        var attribute = collection.attributes.Where(a => a.attributename == operation.path.Substring(1)).FirstOrDefault();
                        if (null == attribute)
                        {
                            throw new HttpException(System.Net.HttpStatusCode.UnprocessableEntity, "path doesn't exists");
                        }

                        using (var tableDataAdapter = new TableDataAdapter(connection, collection))
                        {
                            if (!string.IsNullOrEmpty(injectAttributeName))
                            {
                                tableDataAdapter.injectAttribute(injectAttributeName, injectAttributeValue);
                            }
                            var input = new Dictionary<string, object>();
                            /*var value = new Dictionary<string, object>();
                            value.Add("Value", operation.value);*/
                            var value = new { Value = operation.value };
                            input.Add(attribute.attributename, value);
                            await InternalHandlerASync(context, input, tableDataAdapter);
                        }
                    }
                    await Task.Run(() => { transaction.Commit(); });
                    await connection.CloseAsync();
                }
            }
            catch (HttpException smartAPIException)
            {
                string host = (null == HttpException.Host) ? "undefinedhost" : HttpException.Host;

                if (null == smartAPIException.Message)
                {
                    string warningValue = ((int)smartAPIException.HttpStatusCode).ToString() + " " + host + " \"" + smartAPIException.Message + "\" " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    context.Response.Headers.Add("Warning", warningValue);
                }

                context.Response.StatusCode = (int)smartAPIException.HttpStatusCode;
            }
            catch (SQLiteException sqliteException) when (sqliteException.ErrorCode == 19)
            {
                var httpStatusCode = System.Net.HttpStatusCode.Conflict;
                string message = "A conflict occured. Check correctness of your operation regarding with data constraints.";
                if (sqliteException.Message.Contains("UNIQUE"))
                {
                    message = "A conflict occured. Check correctness of your operation regarding with unicity constraints.";
                }
                string host = (null == HttpException.Host) ? "undefinedhost" : HttpException.Host;
                string warningValue = ((int)httpStatusCode).ToString() + " " + host + " \"" + message + "\" " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                context.Response.Headers.Add("Warning", warningValue);
                context.Response.StatusCode = (int)httpStatusCode;
            }
            catch (Exception /*ex*/)
            {
                var httpStatusCode = System.Net.HttpStatusCode.InternalServerError;
                string message = "Internal server error";
                string host = (null == HttpException.Host) ? "undefinedhost" : HttpException.Host;
                string warningValue = ((int)httpStatusCode).ToString() + " " + host + " \"" + message + "\" " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                context.Response.Headers.Add("Warning", warningValue);
                context.Response.StatusCode = (int)httpStatusCode;
            }
        }

        private async Task<IEnumerable<PatchOperation>> InputAsync(HttpContext context)
        {
                if (context.Request.ContentType.Contains("application/json-patch+json") && context.Request.ContentLength > 0)
                {
                    byte[] inputBytes = new byte[context.Request.ContentLength.Value];
                    _ = await context.Request.Body.ReadAsync(inputBytes, 0, (int)context.Request.ContentLength.Value);
                    return JsonConvert.DeserializeObject<IEnumerable<PatchOperation>>(System.Text.UTF8Encoding.UTF8.GetString(inputBytes));
                }
                else if (context.Request.ContentType.Contains("application/json-patch+xml") && context.Request.ContentLength > 0)
                {
                    /*byte[] inputBytes = new byte[context.Request.ContentLength.Value];
                    _ = await context.Request.Body.ReadAsync(inputBytes, 0, (int)context.Request.ContentLength.Value);
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(System.Text.UTF8Encoding.UTF8.GetString(inputBytes));

                    // This will remove XmlDeclaration
                if (xmlDocument.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
                        xmlDocument.RemoveChild(xmlDocument.FirstChild);

                    var jsonDocument = JsonConvert.DeserializeObject(JsonConvert.SerializeXmlNode(xmlDocument));
                    jsonDocument = ((Newtonsoft.Json.Linq.JObject)jsonDocument).Children().First().Children().First();
                    return jsonDocument;*/
                }

            return null;
        }

        protected async Task InternalHandlerASync(HttpContext context, object inputObject, TableDataAdapter tableDataAdapter)
        {
            var keys = getKeys(context);

            var result = await tableDataAdapter.UpsertAsync(inputObject, keys);
            if (result == TableDataAdapter.UpsertResult.OneRowUpdated) context.Response.StatusCode = 204;

            var endpointMetadata = context.GetEndpoint().Metadata.GetMetadata<SmartAPIEndpointMetadata>();

            if (endpointMetadata.Options.Trigger_AfterOperation != null)
            {
                await endpointMetadata.Options.Trigger_AfterOperation(context, tableDataAdapter.Collection, inputObject, keys);
            }
        }
    }
}
