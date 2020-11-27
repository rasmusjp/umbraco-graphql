using GraphQL;
using GraphQL.Types;
using Umbraco.Forms.Core;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    [GraphQLMetadata(nameof(FormDataSourceDefinition))]
    public class FormDataSourceDefinitionGraphType : ObjectGraphType<FormDataSourceDefinition>
    {
        public FormDataSourceDefinitionGraphType()
        {
            Name = nameof(FormDataSourceDefinition);

            this.AddBuiltinFields();
        }
    }
}
