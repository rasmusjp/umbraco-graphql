using GraphQL.Types;
using System.Collections.Generic;

namespace Our.Umbraco.GraphQL.Adapters.Examine.Types
{
    public class SearchResultFieldsGraphType : ObjectGraphType<KeyValuePair<string, IReadOnlyList<string>>>
    {
    }
}
