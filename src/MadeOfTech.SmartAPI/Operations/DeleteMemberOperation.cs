using MadeOfTech.SmartAPI.DataAdapters;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Operations
{
    public class DeleteMemberOperation : OperationBase
    {
        protected override async Task<OperationOutput> InternalHandlerASync(HttpContext context, object inputObject, TableDataAdapter tableDataAdapter)
        {
            var keys = getKeys(context);

            await tableDataAdapter.DeleteASync(keys);

            context.Response.StatusCode = 204;
            return null;
        }
    }
}
