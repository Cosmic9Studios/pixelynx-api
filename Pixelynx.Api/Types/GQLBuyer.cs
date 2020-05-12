using System;
using HotChocolate;

namespace Pixelynx.Api.Types
{
    public class GQLBuyer 
    {
        public Guid? Id { get; set; }

        [GraphQLIgnore]
        public Guid AssetId { get; set; }


        public GQLBuyer(Guid userId, Guid assetId)
        {
            Id = userId;
            AssetId = assetId;
        }
    }
}