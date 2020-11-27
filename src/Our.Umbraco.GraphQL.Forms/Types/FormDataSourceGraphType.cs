using GraphQL;
using GraphQL.Types;
using Umbraco.Forms.Core;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    [GraphQLMetadata(nameof(FormDataSource))]
    public class FormDataSourceGraphType : ObjectGraphType<FormDataSource>
    {
        public FormDataSourceGraphType()
        {
            Name = nameof(FormDataSource);

            this.AddBuiltinFields();
        }
    }
}
