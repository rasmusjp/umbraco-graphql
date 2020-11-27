using GraphQL;
using GraphQL.Types;
using Umbraco.Forms.Core.Models;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    [GraphQLMetadata(nameof(FieldCondition))]
    public class FieldConditionGraphType : ObjectGraphType<FieldCondition>
    {
        public FieldConditionGraphType()
        {
            Name = nameof(FieldCondition);

            this.AddBuiltinFields();
        }
    }
}
