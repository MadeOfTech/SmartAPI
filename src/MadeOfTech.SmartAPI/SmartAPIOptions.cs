using MadeOfTech.SmartAPI.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
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
        /// Name of the API that will be managed by this middleware. This name
        /// will be used as a key against the api table to enumerate related
        /// collections and attributes.
        /// </summary>
        public string APIDb_APIDesignation { get; set; }

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

        /// <summary>
        /// Define the base path to access resources and documentations. This
        /// value, if not null or empty, will override the base path define in
        /// the api table.
        /// </summary>
        /// <remarks>
        /// It must be set like this : /foo/bar/.
        /// </remarks>
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

        /// <summary>
        /// If implemented, this function allow user to do a batch of operation
        /// after an operation has been done successfully.
        /// parameters are the following :
        /// context is an HttpContext, which give you many informations about the Http Call ;
        /// collection is the collection on which operation is applyed ;
        /// input is the object the caller sent into the body ;
        /// keys is a string array containing key values, enumerated in the order decribed into the database.
        /// </summary>
        /// <example>
        /// options.Trigger_AfterOperation = async (context, collection, input, keys) =>
        /// {
        ///     if (context.Request.Method == "POST" && collection.collectionname == "my_wonderfull_collection")
        ///     {
        ///        // A new member has been inserted in my_wonderfull_collection.
        ///        string designation = input["designation"];
        ///        Console.WriteLine("A new mamber has been inserted : " + designation);
        ///     }
        /// };
        /// </example>
        /// <remarks>
        /// Beware of not modify context nor collection. Results could be
        /// unpredictible.
        /// </remarks>
        /// <remarks>
        /// Notice that if this function is used with Upsert_FillBodyWithMember
        /// for a POST or a PUT operation, all actions done in the trigger will
        /// be reflected by the member that will be request after this trigger.
        /// </remarks>
        public Func<HttpContext, Collection, dynamic, string[], Task> Trigger_AfterOperation { get; set; } = null;
    }
}
