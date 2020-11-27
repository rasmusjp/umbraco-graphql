using GraphQL;
using GraphQL.Types;
using Umbraco.Forms.Core.Models;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    [GraphQLMetadata(nameof(FieldsetContainer))]
    public class FieldsetContainerGraphType : ObjectGraphType<FieldsetContainer>
    {
        public FieldsetContainerGraphType()
        {
            Name = nameof(FieldsetContainer);

            this.AddBuiltinFields();
        }
    }
}
