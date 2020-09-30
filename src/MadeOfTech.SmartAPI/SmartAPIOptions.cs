using MadeOfTech.SmartAPI.Db;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI
{
    public class SmartAPIOptions
    {
        /// <summary>
        /// Connection string to connect to the API database.
        /// </summary>
        public string APIDb_ConnectionString { get; set; }
        /// <summary>
        /// Type of the API database, mysql or sqlite.
        /// </summary>
        public string APIDb_ConnectionType { get; set; }
        /// <summary>
        /// This will use to replace user in connection strings stored in the
        /// APIDb. More precisely, it will replace the {host} field in the
        /// connection string
        /// </summary>
        public string DataDb_ConnectionHost { get; set; }
        /// <summary>
        /// This will use to replace user in connection strings stored in the
        /// APIDb. More precisely, it will replace the {user} field in the
        /// connection string
        /// </summary>
        public string DataDb_ConnectionUser { get; set; }
        /// <summary>
        /// This will use to replace password in connection strings stored in the
        /// APIDb. More precisely, it will replace the {pwd} field in the
        /// connection string
        /// </summary>
        public string DataDb_ConnectionPwd { get; set; }
        public string BasePath { get; set; }
        /// <summary>
        /// Add authentication to each endpoint that has been mapped for the
        /// middleware. Authentication is based uppon database data.
        /// </summary>
        public bool Authentication_RequireAuthentication { get; set; }

        /// <summary>
        /// Use this option if authentication is required (no effect else). It
        /// define one policy per operation (get{collectionname}, put{membername})
        /// and add it to the list of autorized policies.
        /// </summary>
        public bool Authentication_UsePerOperationPolicy { get; set; } = false;

        /// <summary>
        /// Use this option if authentication is required (no effect else). It
        /// defines whether authentication could be based upon data stored in
        /// database or not.
        /// </summary>
        public bool Authentication_UseDatabasePolicy { get; set; } = false;

        /// <summary>
        /// Use this option if authentication is required (no effect else). It
        /// defines the global policy name to access to get operations.
        /// </summary>
        public string Authentication_GlobalReadPolicyName { get; set; } = null;

        /// <summary>
        /// Use this option if authentication is required (no effect else). It
        /// defines the global policy name to access to all operations.
        /// </summary>
        public string Authentication_GlobalModifyPolicyName { get; set; } = null;

        /// <summary>
        /// Add Cors to each endpoint that has been mapped for the
        /// middleware.
        /// </summary>
        public bool Cors_RequireCors { get; set; } = false;

        /// <summary>
        /// Cors policy name that will be added to each endpoint that has been 
        /// mapped for the middleware.
        /// </summary>
        public string Cors_PolicyName { get; set; } = null;

        /// <summary>
        /// If true, X-Powered-By will contain SmartAPI : smartapi.madeoftech.com.
        /// </summary>
        public bool FillHeaderXPoweredBy { get; set; } = true;

        /// <summary>
        /// If set, will generate the OpenAPI document describing this API and
        /// publish it to the given path.
        /// </summary>
        public string OpenAPIDocument_Path { get; set; } = null;

        /// <summary>
        /// If set, an attribute will be inserted inside every requests made
        /// against the database. This attribute will be invisible from the API
        /// client. Clearly, this aims at allowing multiple clients to use the
        /// same API with an enforce by design approach.
        /// </summary>
        public string InjectAttribute_Name { get; set; } = null;

        /// <summary>
        /// This property works with InjectAttribute_Name. It allows evaluation
        /// of the value to be injected for the attribute InjectAttribute_Name.
        /// </summary>
        /// <seealso cref="InjectAttribute_Name"/>
        public Func<HttpContext, object> InjectAttribute_ValueEvaluator { get; set; } = null;

        /// <summary>
        /// If set to true, the API will behave a little differently from the
        /// standard for upsert operations ie. POST and PUT. Indeed, in addition
        /// to returning the recommended headers it will fill the body with member
        /// representation.
        /// 
        /// This option can be very important for POST only collections, for 
        /// exemple.
        /// </summary>
        public bool Upsert_FillBodyWithMember { get; set; } = false;
    }
}
