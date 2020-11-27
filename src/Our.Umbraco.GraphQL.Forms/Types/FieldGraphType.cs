using GraphQL;
using GraphQL.Types;
using Umbraco.Forms.Core.Models;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    [GraphQLMetadata(nameof(Field))]
    public class FieldGraphType : ObjectGraphType<Field>
    {
        public FieldGraphType()
        {
            Name = nameof(Field);

            this.AddBuiltinFields();
        }
    }
}
