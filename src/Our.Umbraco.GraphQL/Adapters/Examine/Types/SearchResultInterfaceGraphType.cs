using Examine;
using GraphQL;
using GraphQL.Types;

namespace Our.Umbraco.GraphQL.Adapters.Examine.Types
{
    [GraphQLMetadata(TypeName)]
    public class SearchResultInterfaceGraphType : InterfaceGraphType<ISearchResult>
    {
        private const string TypeName = "SearchResult";
        public SearchResultInterfaceGraphType()
        {
            Name = TypeName;

            this.AddBuiltInFields();
        }
    }
}
