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
            DeleteMember
        }
        internal EndpointOperation Operation { get; }
        internal Collection Collection { get; }
        internal Attribute[] Attributes { get; }
        internal SmartAPIOptions Options { get; }
        internal SmartAPIEndpointMetadata()
        {
        }
        internal SmartAPIEndpointMetadata(
            SmartAPIOptions options,
            EndpointOperation operation,
            Collection collection,
            Attribute[] attributes
            )
        {
            Options = options;
            Operation = operation;
            Collection = collection;
            Attributes = attributes;
        }
    }
}
