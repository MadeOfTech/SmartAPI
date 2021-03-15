using MadeOfTech.SmartAPI.Data.Models;
using Attribute = MadeOfTech.SmartAPI.Data.Models.Attribute;
using Microsoft.OpenApi.Models;

namespace MadeOfTech.SmartAPI
{
    internal class SmartAPIDocumentationMetadata
    {
        internal OpenApiDocument Document { get; }
        internal SmartAPIOptions Options { get; }
        internal SmartAPIDocumentationMetadata()
        {
        }
        internal SmartAPIDocumentationMetadata(
            SmartAPIOptions options,
            OpenApiDocument document
            )
        {
            Options = options;
            Document = document;
        }
    }
}
