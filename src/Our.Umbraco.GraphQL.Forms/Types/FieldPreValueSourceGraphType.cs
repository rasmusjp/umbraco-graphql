using GraphQL;
using GraphQL.Types;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Interfaces;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    [GraphQLMetadata(nameof(FieldPreValueSource))]
    public class FieldPreValueSourceGraphType : ObjectGraphType<IFieldPreValueSource>
    {
        public FieldPreValueSourceGraphType()
        {
            Name = nameof(FieldPreValueSource);

            this.AddBuiltinFields();
        }
    }
}
