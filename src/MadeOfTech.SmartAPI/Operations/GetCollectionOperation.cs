using MadeOfTech.SmartAPI.DataAdapters;
using MadeOfTech.SmartAPI.FIQL;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Operations
{
    public class GetCollectionOperation : CRUDOperationBase
    {
        public GetCollectionOperation() : base(ObjectType.Void, ObjectType.Collection) { }
        protected override async Task<OperationOutput> InternalHandlerASync(HttpContext context, object inputObject, TableDataAdapter tableDataAdapter)
        {
            var array = new List<dynamic>();

            if (context.Request.Query.ContainsKey("query") && tableDataAdapter.Attributes.Select(x => x.fiqlkeyindex.HasValue).Count() > 0)
            {
                var fiqlParser = new FIQLParser();
                var rootExpression = fiqlParser.Parse(context.Request.Query["query"]);
                var whereClause = rootExpression.ComputeWhereClause(tableDataAdapter.Attributes);
                Console.WriteLine(whereClause.whereSqlStatement);
                array = await tableDataAdapter.SelectAsync(whereClause);
            }
            else
            {
                array = await tableDataAdapter.SelectAsync();
            }

            context.Response.StatusCode = 200;
            return new OperationOutput(OperationOutput.RessourceType.Collection, tableDataAdapter.Collection.collectionname, array);
        }
    }
}
