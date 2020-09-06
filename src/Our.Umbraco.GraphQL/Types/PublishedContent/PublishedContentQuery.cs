using System;
using Our.Umbraco.GraphQL.Attributes;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache;

namespace Our.Umbraco.GraphQL.Types.PublishedContent
{
    public class PublishedContentQuery
    {
        [NonNull]
        public PublishedContentAtRootQuery AtRoot([Inject] PublishedContentAtRootQuery query) => query;

        public IPublishedContent ById([Inject] IPublishedSnapshotAccessor snapshotAccessor, Id id, string culture = null) =>
            GetInternal(snapshotAccessor, x => x.GetById(id.As<Guid>()), culture);

        [NonNull]
        public PublishedContentByTypeQuery ByType([Inject] PublishedContentByTypeQuery query) => query;

        public IPublishedContent ByUrl([Inject] IPublishedSnapshotAccessor snapshotAccessor, string url, string culture = null) =>
            GetInternal(snapshotAccessor, x => x.GetByRoute(url, culture: culture), culture);

        private static IPublishedContent GetInternal(IPublishedSnapshotAccessor snapshotAccessor, Func<IPublishedContentCache, IPublishedContent> fetch, string culture)
        {
            var content = fetch(snapshotAccessor.PublishedSnapshot.Content);
            if (culture == null || content != null && content.IsInvariantOrHasCulture(culture))
                return content;

            return null;
        }
    }

    public class ExtendUmbracoQueryWithPublishedContentQuery
    {
        [NonNull]
        [Description("Published content in Umbraco")]
        public PublishedContentQuery Content([Inject] PublishedContentQuery query) => query;
    }
}
