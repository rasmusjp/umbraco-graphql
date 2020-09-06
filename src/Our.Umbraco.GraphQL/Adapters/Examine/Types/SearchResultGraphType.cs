using Examine;
using GraphQL.Types;
using Our.Umbraco.GraphQL.Adapters.PublishedContent.Types;
using System.Collections.Generic;
using Umbraco.Web.PublishedCache;

namespace Our.Umbraco.GraphQL.Adapters.Examine.Types
{
    public class SearchResultGraphType : ObjectGraphType<ISearchResult>
    {
        public SearchResultGraphType(IPublishedSnapshotAccessor snapshotAccessor, string searcherSafeName, IEnumerable<string> fields)
        {
            Name = $"{searcherSafeName}Result";

            Interface<SearchResultInterfaceGraphType>();

            this.AddBuiltInFields(fields == null);

            if (fields != null)
            {
                foreach (var field in fields)
                {
                    Field<StringGraphType>().Name(field.SafeName()).Resolve(ctx => ctx.Source.AllValues.TryGetValue(field, out var list) ? string.Join(", ", list) : null);

                    if (field == "__NodeId")
                    {
                        Field<PublishedContentInterfaceGraphType>().Name("_ContentNode").Description("The published content associated with the specified __NodeId value")
                            .Resolve(ctx => int.TryParse(ctx.Source.Id, out var id) ? snapshotAccessor.PublishedSnapshot.Content.GetById(id) : null);
                    }
                }
            }
        }
    }
}
