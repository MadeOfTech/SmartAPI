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
    public abstract class OperationBase
    {
        /// <summary>
        /// This regex allow us to build a unique id for member url, even if it
        /// uses a composite key.
        /// A key is built like followinf this grammar
        /// (https://www.crockford.com/mckeeman.html style) :
        ///
        /// key
        ///		element
        ///		'(' elements ')'
        /// 
        /// elements
        ///		element ',' element
        ///		element ',' elements
        ///		
        /// element
        ///		string
        ///		value
        /// 
        /// string
        ///		'"' characters '"'
        ///
        ///	characters
        ///		""
        ///		character characters
        ///	
        /// character
        ///		- '"'
        ///		
        /// value
        ///		- '"' - ',' - ')'
        /// 
        /// Original regex :
        /// ^\((?&lt;key>"(""|[^"])*"|[^\),"]*)(,(?&lt;key>"(""|[^"])*"|[^\),"]*))+\)$|^(?&lt;key>"(""|[^"])*"|[^,"]*)$
        /// 
        /// tested with : http://regexstorm.net/tester
        /// converted with : http://derekslager.com/blog/posts/2007/09/a-better-dotnet-regular-expression-tester.ashx
        /// </summary>
        private const string _regexPattern = @"^\((?<key>""(""""|[^""])*""|[^\),""]*)(,(?<key>""(""""|[^""])*""|[^\),""]*))+\)$|^(?<key>""(""""|[^""])*""|[^,""]*)$";
        private Regex _regex = new Regex(_regexPattern, RegexOptions.ExplicitCapture);

        public enum ObjectType
        {
            Collection,
            Member,
            Attribute,
            Void
        }

        public ObjectType InputObjectType { get; }
        public ObjectType OutputObjectType { get; }

        protected OperationBase(ObjectType inputObjectType = ObjectType.Void, ObjectType outputObjectType = ObjectType.Void)
        {
            InputObjectType = inputObjectType;
            OutputObjectType = outputObjectType;
        }

        public async Task Invoke(HttpContext context, Collection collection, Data.Models.Attribute[] attributes, string injectAttributeName, object injectAttributeValue)
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

                var connectionString = collection.connectionstring;
                using (var connection = DBConnectionBuilder.Use(collection.dbtype_designation, collection.connectionstring))
                {
                    await connection.OpenAsync();
                    using (var tableDataAdapter = new TableDataAdapter(connection, collection, attributes))
                    {
                        if (!string.IsNullOrEmpty(injectAttributeName))
                        {
                            tableDataAdapter.injectAttribute(injectAttributeName, injectAttributeValue);
                        }
                        var output = await InternalHandlerASync(context, input, tableDataAdapter);
                        await connection.CloseAsync();

                        await OutputAsync(context, output, collection, attributes);
                    }
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
            catch (FIQLException ex)
            {
                var httpStatusCode = System.Net.HttpStatusCode.BadRequest;
                string message = "A problem occured with your FIQL request. " + ex.Message;
                string host = (null == HttpException.Host) ? "undefinedhost" : HttpException.Host;
                string warningValue = ((int)httpStatusCode).ToString() + " " + host + " \"" + message + "\" " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                context.Response.Headers.Add("Warning", warningValue);
                context.Response.StatusCode = (int)httpStatusCode;
            }
            catch (Exception ex)
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
                if (context.Request.ContentType == "application/json" && context.Request.ContentLength > 0)
                {
                    byte[] inputBytes = new byte[context.Request.ContentLength.Value];
                    _ = await context.Request.Body.ReadAsync(inputBytes, 0, (int)context.Request.ContentLength.Value);
                    return JsonConvert.DeserializeObject(System.Text.UTF8Encoding.UTF8.GetString(inputBytes));
                }
                else if (context.Request.ContentType == "application/xml" && context.Request.ContentLength > 0)
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

        private async Task OutputAsync(HttpContext context, OperationOutput smartAPIRouterOutput, Collection collection, Data.Models.Attribute[] attributes)
        {
            if (OutputObjectType == ObjectType.Void) return;
            if (null == smartAPIRouterOutput) return;

            var output = "";

            string format = context.Request.Headers.ContainsKey("Accept") ? context.Request.Headers["Accept"].ToString().ToLower() : "application/json";
            if (format == "application/xml")
            {
                XmlSerializer xmlSerializer = null;
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
                }
                else if (OutputObjectType == ObjectType.Attribute)
                {
                    outputObject = new XmlAttributeSerialize(smartAPIRouterOutput.Value);
                    xmlAttributeOverrides.Add(typeof(XmlAttributeSerialize), xmlAttributes);
                    xmlSerializer = new XmlSerializer(typeof(XmlAttributeSerialize), xmlAttributeOverrides);
                }

                using (var stringWriter = new StringWriter())
                {
                    using (XmlWriter writer = XmlWriter.Create(stringWriter))
                    {
                        try
                        {
                            xmlSerializer.Serialize(writer, outputObject);
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        output = stringWriter.ToString(); // Your XML
                    }
                }
            }
            else /* We'll return json */
            {
                var setting = new JsonSerializerSettings { Converters = { new ByteArrayHexConverter(), new DateTimeISO8601Converter() } };
                output = JsonConvert.SerializeObject(smartAPIRouterOutput.Value, setting);
            }

            await context.Response.WriteAsync(output);
        }

        protected abstract Task<OperationOutput> InternalHandlerASync(HttpContext context, object inputObject, TableDataAdapter tableDataAdapter);

        protected string[] getKeys(HttpContext context)
        {
            var id = context.Request.RouteValues["id"].ToString();
            id = System.Web.HttpUtility.UrlDecode(id);
            if (string.IsNullOrEmpty(id)) return null;

            var match = _regex.Match(id);
            var keys = new List<string>();
            foreach (var capture in match.Groups[1].Captures)
            {
                keys.Add(capture.ToString());
            }

            return keys.ToArray();
        }
    }
}
