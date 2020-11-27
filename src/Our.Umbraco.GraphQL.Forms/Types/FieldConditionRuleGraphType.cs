using GraphQL;
using GraphQL.Types;
using Umbraco.Forms.Core.Models;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    [GraphQLMetadata(nameof(FieldConditionRule))]
    public class FieldConditionRuleGraphType : ObjectGraphType<FieldConditionRule>
    {
        public FieldConditionRuleGraphType()
        {
            Name = nameof(FieldConditionRule);

            this.AddBuiltinFields();
        }
    }
}
