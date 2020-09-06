using Examine;
using GraphQL.Types;
using System.Collections.Generic;
using Umbraco.Web.PublishedCache;

namespace Our.Umbraco.GraphQL.Adapters.Examine.Types
{
    public class SearchResultsGraphType : ObjectGraphType<ISearchResults>
    {
        public SearchResultsGraphType(IPublishedSnapshotAccessor snapshotAccessor, string searcherSafeName, IEnumerable<string> fields)
        {
            Name = $"{searcherSafeName}Results";

            Interface<SearchResultsInterfaceGraphType>();

            this.AddBuiltInFields(snapshotAccessor, searcherSafeName, fields);
        }
    }
}
