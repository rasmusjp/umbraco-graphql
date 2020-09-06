using Examine;
using GraphQL;
using GraphQL.Types;

namespace Our.Umbraco.GraphQL.Adapters.Examine.Types
{
    [GraphQLMetadata(TypeName)]
    public class SearchResultsInterfaceGraphType : InterfaceGraphType<ISearchResults>
    {
        private const string TypeName = "SearchResults";
        public SearchResultsInterfaceGraphType()
        {
            Name = TypeName;

            this.AddBuiltInFields();
        }
    }
}
