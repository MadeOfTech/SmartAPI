using MadeOfTech.SmartAPI.Data.Models;
using Attribute = MadeOfTech.SmartAPI.Data.Models.Attribute;

namespace MadeOfTech.SmartAPI
{
    internal class SmartAPIEndpointMetadata
    {
        internal enum EndpointOperation
        {
            GetCollection,
            GetMember,
            PostMember,
            PutMember,
            PatchMember,
            DeleteMember
        }
        internal EndpointOperation Operation { get; }
        internal Collection Collection { get; }
        internal SmartAPIOptions Options { get; }
        internal SmartAPIEndpointMetadata()
        {
        }
        internal SmartAPIEndpointMetadata(
            SmartAPIOptions options,
            EndpointOperation operation,
            Collection collection
            )
        {
            Options = options;
            Operation = operation;
            Collection = collection;
        }
    }
}
