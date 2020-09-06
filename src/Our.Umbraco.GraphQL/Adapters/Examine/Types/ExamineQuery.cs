using Our.Umbraco.GraphQL.Attributes;

namespace Our.Umbraco.GraphQL.Adapters.Examine.Types
{
    public class ExamineQuery
    {
    }

    public class ExtendUmbracoQueryWithExamineQuery
    {
        [NonNull]
        [Description("Query Examine indexes")]
        public ExamineQuery Examine([Inject] ExamineQuery query) => query;
    }
}
