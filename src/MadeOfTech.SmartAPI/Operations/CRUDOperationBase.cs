using MadeOfTech.SmartAPI.DataAdapters;
using MadeOfTech.SmartAPI.Data.Models;
using MadeOfTech.SmartAPI.Db;
using MadeOfTech.SmartAPI.Exceptions;
using MadeOfTech.SmartAPI.JsonSerialization;
using MadeOfTech.SmartAPI.XmlSerialization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using MadeOfTech.SmartAPI.FIQL;

namespace MadeOfTech.SmartAPI.Operations
{
    public abstract class CRUDOperationBase : OperationBase
    {
        public enum ObjectType
        {
            Collection,
            Member,
            Attribute,
            Void
        }

        public ObjectType InputObjectType { get; }
        public ObjectType OutputObjectType { get; }

        protected CRUDOperationBase(ObjectType inputObjectType = ObjectType.Void, ObjectType outputObjectType = ObjectType.Void)
        {
            InputObjectType = inputObjectType;
            OutputObjectType = outputObjectType;
        }

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

                var input = await InputAsync(context);

                var connectionString = collection.db.connectionstring;
                using (var connection = DBConnectionBuilder.Use(collection.db.dbtype, collection.db.connectionstring))
                {
                    await connection.OpenAsync();
                    using (var tableDataAdapter = new TableDataAdapter(connection, collection))
                    {
                        if (!string.IsNullOrEmpty(injectAttributeName))
                        {
                            tableDataAdapter.injectAttribute(injectAttributeName, injectAttributeValue);
                        }
                        var output = await InternalHandlerASync(context, input, tableDataAdapter);
                        await connection.CloseAsync();

                        await OutputAsync(context, output, collection);
                    }
                }
            }
            catch (HttpException smartAPIException)
            {
                string host = (null == HttpException.Host) ? "undefinedhost" : HttpException.Host;

                if (null != smartAPIException.Message)
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
            catch (FIQLException ex)
            {
                var httpStatusCode = System.Net.HttpStatusCode.BadRequest;
                string message = "A problem occured with your FIQL request. " + ex.Message;
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
        private async Task<object> InputAsync(HttpContext context)
        {
            if ((InputObjectType != ObjectType.Void) && (context.Request.ContentLength > 0))
            {
                if (context.Request.ContentType.Contains("application/json") && context.Request.ContentLength > 0)
                {
                    byte[] inputBytes = new byte[context.Request.ContentLength.Value];
                    _ = await context.Request.Body.ReadAsync(inputBytes, 0, (int)context.Request.ContentLength.Value);
                    return JsonConvert.DeserializeObject(System.Text.UTF8Encoding.UTF8.GetString(inputBytes));
                }
                else if (context.Request.ContentType.Contains("application/xml") && context.Request.ContentLength > 0)
                {
                    byte[] inputBytes = new byte[context.Request.ContentLength.Value];
                    _ = await context.Request.Body.ReadAsync(inputBytes, 0, (int)context.Request.ContentLength.Value);
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(System.Text.UTF8Encoding.UTF8.GetString(inputBytes));

                    // This will remove XmlDeclaration
                    if (xmlDocument.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
                        xmlDocument.RemoveChild(xmlDocument.FirstChild);

                    var jsonDocument = JsonConvert.DeserializeObject(JsonConvert.SerializeXmlNode(xmlDocument));
                    jsonDocument = ((Newtonsoft.Json.Linq.JObject)jsonDocument).Children().First().Children().First();
                    return jsonDocument;
                }
            }

            return null;
        }

        private async Task OutputAsync(HttpContext context, OperationOutput smartAPIRouterOutput, Collection collection)
        {
            if (OutputObjectType == ObjectType.Void) return;
            if (null == smartAPIRouterOutput) return;

            var output = "";

            string format = context.Request.Headers.ContainsKey("Accept") ? context.Request.Headers["Accept"].ToString().ToLower() : "application/json";
            if (format == "application/xml")
            {
                /*XmlSerializer xmlSerializer = null;
                object outputObject = null;

                var xmlAttributes = new XmlAttributes();
                xmlAttributes.XmlRoot = new XmlRootAttribute(smartAPIRouterOutput.Name);
                var xmlAttributeOverrides = new XmlAttributeOverrides();

                if (OutputObjectType == ObjectType.Collection)
                {
                    outputObject = new XmlDynamicListSerialize((List<dynamic>)smartAPIRouterOutput.Value, collection.membername);
                    xmlAttributeOverrides.Add(typeof(XmlDynamicListSerialize), xmlAttributes);
                    xmlSerializer = new XmlSerializer(typeof(XmlDynamicListSerialize), xmlAttributeOverrides);
                }
                else if (OutputObjectType == ObjectType.Member)
                {
                    outputObject = new XmlDynamicSerialize((dynamic)smartAPIRouterOutput.Value);
                    xmlAttributeOverrides.Add(typeof(XmlDynamicSerialize), xmlAttributes);
                    xmlSerializer = new XmlSerializer(typeof(XmlDynamicSerialize), xmlAttributeOverrides);
                }*//*
                else if (OutputObjectType == ObjectType.Attribute)
                {
                    outputObject = new XmlAttributeSerialize(smartAPIRouterOutput.Value);
                    xmlAttributeOverrides.Add(typeof(XmlAttributeSerialize), xmlAttributes);
                    xmlSerializer = new XmlSerializer(typeof(XmlAttributeSerialize), xmlAttributeOverrides);
                }*/

                /*using (var stringWriter = new StringWriter())
                {
                    using (XmlWriter writer = XmlWriter.Create(stringWriter))
                    {
                        try
                        {
                            xmlSerializer.Serialize(writer, outputObject);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        output = stringWriter.ToString(); // Your XML
                    }
                }*/
                StringWriter buffer = new StringWriter();
                buffer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");

                if (OutputObjectType == ObjectType.Collection)
                {
                    await GenerateCollectionXML(buffer, collection, (IEnumerable<dynamic>)smartAPIRouterOutput.Value);
                }
                else if (OutputObjectType == ObjectType.Member)
                {
                    await GenerateMemberXML(buffer, collection, (IDictionary<string, object>)smartAPIRouterOutput.Value);
                }

                output = buffer.ToString();
            }
            else /* We'll return json */
            {
                /*
                var setting = new JsonSerializerSettings { Converters = { new ByteArrayHexConverter(), new DateTimeISO8601Converter() } };
                output = JsonConvert.SerializeObject(smartAPIRouterOutput.Value, setting);*/
                StringWriter buffer = new StringWriter();
                if (OutputObjectType == ObjectType.Collection)
                {
                    await GenerateCollectionJson(buffer, collection, (IEnumerable<dynamic>)smartAPIRouterOutput.Value);
                }
                else if (OutputObjectType == ObjectType.Member)
                {
                    await GenerateMemberJson(buffer, collection, (IDictionary<string, object>)smartAPIRouterOutput.Value);
                }

                output = buffer.ToString();
            }

            await context.Response.WriteAsync(output);
        }

        private async Task GenerateCollectionJson(StringWriter buffer, Collection collection, IEnumerable<dynamic> members)
        {
            await buffer.WriteAsync("[");
            bool firstMember = true;
            foreach (IDictionary<string, object> member in members)
            {
                if (firstMember)
                {
                    firstMember = false;
                }
                else
                {
                    await buffer.WriteAsync(",");
                }
                await GenerateMemberJson(buffer, collection, member);
            }
            await buffer.WriteAsync("]");
        }

        private async Task GenerateMemberJson(StringWriter buffer, Collection collection, IDictionary<string, object> member)
        {
            bool firstAttribute = true;
            await buffer.WriteAsync("{");
            foreach (var attribute in collection.attributes)
            {
                if (firstAttribute)
                {
                    firstAttribute = false;
                }
                else
                {
                    await buffer.WriteAsync(",");
                }

                await buffer.WriteAsync(attribute.ToStringJson(member[attribute.attributename]));
            }
            await buffer.WriteAsync("}");
        }

        private async Task GenerateCollectionXML(StringWriter buffer, Collection collection, IEnumerable<dynamic> members)
        {
            await buffer.WriteAsync("<" + collection.collectionname + ">");
            foreach (IDictionary<string, object> member in members)
            {
                await GenerateMemberXML(buffer, collection, member);
            }
            await buffer.WriteAsync("</" + collection.collectionname + ">");
        }

        private async Task GenerateMemberXML(StringWriter buffer, Collection collection, IDictionary<string, object> member)
        {
            await buffer.WriteAsync("<" + collection.membername + ">");
            foreach (var attribute in collection.attributes)
            {
                await buffer.WriteAsync(attribute.ToStringXML(member[attribute.attributename]));
            }
            await buffer.WriteAsync("</" + collection.membername + ">");
        }

        protected abstract Task<OperationOutput> InternalHandlerASync(HttpContext context, object inputObject, TableDataAdapter tableDataAdapter);
    }
}
