using GraphQL;
using GraphQL.Types;
using Umbraco.Forms.Core.Models;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    [GraphQLMetadata(nameof(Form))]
    public class FormGraphType : ObjectGraphType<Form>
    {
        public FormGraphType()
        {
            Name = nameof(Form);

            this.AddBuiltinFields();
        }
    }
}
