using GraphQL;
using GraphQL.Types;
using Umbraco.Forms.Core.Models;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    [GraphQLMetadata(nameof(Page))]
    public class PageGraphType : ObjectGraphType<Page>
    {
        public PageGraphType()
        {
            Name = nameof(Page);

            this.AddBuiltinFields();
        }
    }
}
