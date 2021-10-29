using MadeOfTech.SmartAPI.DataAdapters;
using MadeOfTech.SmartAPI.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Operations
{
    public class PutMemberOperation : CRUDOperationBase
    {
        public PutMemberOperation() : base(ObjectType.Member, ObjectType.Member) { }
        protected override async Task<OperationOutput> InternalHandlerASync(HttpContext context, object inputObject, TableDataAdapter tableDataAdapter)
        {
            var keys = getKeys(context);

            var result = await tableDataAdapter.UpsertAsync(inputObject, keys);
            switch (result)
            {
                case TableDataAdapter.UpsertResult.NothingChanged: context.Response.StatusCode = 304; break;
                case TableDataAdapter.UpsertResult.OneRowInserted: context.Response.StatusCode = 201; break;
                case TableDataAdapter.UpsertResult.OneRowUpdated: context.Response.StatusCode = 200; break;
            }

            var endpointMetadata = context.GetEndpoint().Metadata.GetMetadata<SmartAPIEndpointMetadata>();

            if (endpointMetadata.Options.Trigger_AfterOperation != null)
            {
                await endpointMetadata.Options.Trigger_AfterOperation(context, tableDataAdapter.Collection, inputObject, keys);
            }

            if (endpointMetadata.Options.Upsert_FillBodyWithMember && context.Response.StatusCode != 304)
            {
                var array = new List<dynamic>();
                array = await tableDataAdapter.SelectAsync(keys);

                if (0 == array.Count) throw new NotFoundException();
                else if (array.Count > 1) throw new InternalServerErrorException();

                return new OperationOutput(OperationOutput.RessourceType.Member, tableDataAdapter.Collection.membername, array.First());
            }
            else
            {
                return null;
            }
        }
    }
}
