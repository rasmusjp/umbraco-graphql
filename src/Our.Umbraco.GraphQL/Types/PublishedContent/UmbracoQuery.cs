using Our.Umbraco.GraphQL.Attributes;

namespace Our.Umbraco.GraphQL.Types.PublishedContent
{
    public class UmbracoQuery
    {
    }

    public class ExtendQueryWithUmbracoQuery
    {
        [NonNull]
        [Description("Query various types of data from the Umbraco CMS")]
        public UmbracoQuery Umbraco([Inject] UmbracoQuery query) => query;
    }
}
