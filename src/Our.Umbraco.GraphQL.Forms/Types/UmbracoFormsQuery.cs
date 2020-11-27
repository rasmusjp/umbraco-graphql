using Our.Umbraco.GraphQL.Attributes;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    public class UmbracoFormsQuery
    {
        public FormsDataQuery Forms([Inject] FormsDataQuery query) => query;
        public DataSourcesDataQuery DataSource([Inject] DataSourcesDataQuery query) => query;
        public WorkflowsDataQuery Workflows([Inject] WorkflowsDataQuery query) => query;
        public PreValueSourcesDataQuery PreValueSources([Inject] PreValueSourcesDataQuery query) => query;
    }

    public class ExtendQueryWithUmbracoFormsQuery
    {
        [NonNull]
        [Description("Queries varies pieces of data regarding Umbraco Forms")]
        public UmbracoFormsQuery UmbracoForms([Inject] UmbracoFormsQuery query) => query;
    }
}
