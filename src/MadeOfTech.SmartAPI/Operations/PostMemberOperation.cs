using MadeOfTech.SmartAPI.DataAdapters;
using MadeOfTech.SmartAPI.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Operations
{
    public class PostMemberOperation : OperationBase
    {
        public PostMemberOperation() : base(ObjectType.Member, ObjectType.Member) { }
        protected override async Task<OperationOutput> InternalHandlerASync(HttpContext context, object inputObject, TableDataAdapter tableDataAdapter)
        {
            var row = await tableDataAdapter.InsertAsync(inputObject);
            var id = (long)row.serial;

            context.Response.StatusCode = 201;

            var httpRequestFeature = context.Features.Get<IHttpRequestFeature>();
            context.Response.Headers["Location"] = httpRequestFeature.RawTarget + "/" + id;

            var endpointMetadata = context.GetEndpoint().Metadata.GetMetadata<SmartAPIEndpointMetadata>();

            if (endpointMetadata.Options.Upsert_FillBodyWithMember)
            {
                var array = new List<dynamic>();
                array = await tableDataAdapter.SelectAsync(new string[] { id.ToString() });

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
