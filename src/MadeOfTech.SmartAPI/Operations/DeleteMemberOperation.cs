using MadeOfTech.SmartAPI.DataAdapters;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Operations
{
    public class DeleteMemberOperation : CRUDOperationBase
    {
        protected override async Task<OperationOutput> InternalHandlerASync(HttpContext context, object inputObject, TableDataAdapter tableDataAdapter)
        {
            var keys = getKeys(context);

            await tableDataAdapter.DeleteASync(keys);

            context.Response.StatusCode = 204;

            var endpointMetadata = context.GetEndpoint().Metadata.GetMetadata<SmartAPIEndpointMetadata>();

            if (endpointMetadata.Options.Trigger_AfterOperation != null)
            {
                await endpointMetadata.Options.Trigger_AfterOperation(context, tableDataAdapter.Collection, inputObject, keys);
            }

            return null;
        }
    }
}
