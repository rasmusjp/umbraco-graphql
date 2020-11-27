using GraphQL;
using GraphQL.Types;
using Umbraco.Forms.Core.Interfaces;
using Umbraco.Forms.Core.Models;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    [GraphQLMetadata(nameof(Workflow))]
    public class WorkflowGraphType : ObjectGraphType<IWorkflow>
    {
        public WorkflowGraphType()
        {
            Name = nameof(Workflow);

            this.AddBuiltinFields();
        }
    }
}
