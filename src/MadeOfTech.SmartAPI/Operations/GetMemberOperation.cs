using MadeOfTech.SmartAPI.DataAdapters;
using MadeOfTech.SmartAPI.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MadeOfTech.SmartAPI.Operations
{
    public class GetMemberOperation : OperationBase
    {
        public GetMemberOperation() : base(ObjectType.Void, ObjectType.Member) { }
        protected override async Task<OperationOutput> InternalHandlerASync(HttpContext context, object inputObject, TableDataAdapter tableDataAdapter)
        {
            var keys = getKeys(context);

            var array = new List<dynamic>();
            array = await tableDataAdapter.SelectAsync(keys);

            if (0 == array.Count) throw new NotFoundException();
            else if (array.Count > 1) throw new InternalServerErrorException();

            context.Response.StatusCode = 200;
            return new OperationOutput(OperationOutput.RessourceType.Member, tableDataAdapter.Collection.membername, array.First());
        }
    }
}
