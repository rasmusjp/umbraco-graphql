using GraphQL;
using GraphQL.Types;
using Umbraco.Forms.Core.Models;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    [GraphQLMetadata(nameof(FieldSet))]
    public class FieldSetGraphType : ObjectGraphType<FieldSet>
    {
        public FieldSetGraphType()
        {
            Name = nameof(FieldSet);

            this.AddBuiltinFields();
        }
    }
}
