using GraphQL;
using GraphQL.Types;
using Umbraco.Forms.Core;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    [GraphQLMetadata(nameof(FormDataSourceMapping))]
    public class FormDataSourceMappingGraphType : ObjectGraphType<FormDataSourceMapping>
    {
        public FormDataSourceMappingGraphType()
        {
            Name = nameof(FormDataSourceMapping);

            this.AddBuiltinFields();
        }
    }
}
