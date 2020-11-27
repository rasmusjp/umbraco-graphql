using Our.Umbraco.GraphQL.Adapters.Types.Resolution;
using Our.Umbraco.GraphQL.Forms.Types;
using Our.Umbraco.GraphQL.Types;
using System.Collections.Generic;
using Umbraco.Core.Composing;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Interfaces;
using Umbraco.Forms.Core.Models;

namespace Our.Umbraco.GraphQL.Forms.Composing
{
    public class GraphQLFormsComponent : IComponent
    {
        private readonly ITypeRegistry _typeRegistry;

        public GraphQLFormsComponent(ITypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        public void Initialize()
        {
            _typeRegistry.Add<Form, FormGraphType>();
            _typeRegistry.Add<Page, PageGraphType>();
            _typeRegistry.Add<FormDataSourceDefinition, FormDataSourceDefinitionGraphType>();
            _typeRegistry.Add<FieldSet, FieldSetGraphType>();
            _typeRegistry.Add<FormDataSourceMapping, FormDataSourceMappingGraphType>();
            _typeRegistry.Add<FieldsetContainer, FieldsetContainerGraphType>();
            _typeRegistry.Add<FieldCondition, FieldConditionGraphType>();
            _typeRegistry.Add<Field, FieldGraphType>();
            _typeRegistry.Add<FieldConditionRule, FieldConditionRuleGraphType>();
            _typeRegistry.Add<KeyValuePair<string, string>, StringKeyValuePairGraphType>();
            _typeRegistry.Add<FormDataSource, FormDataSourceGraphType>();
            _typeRegistry.Add<IWorkflow, WorkflowGraphType>();
            _typeRegistry.Add<IFieldPreValueSource, FieldPreValueSourceGraphType>();

            _typeRegistry.Extend<Query, ExtendQueryWithUmbracoFormsQuery>();
            _typeRegistry.Extend<Mutation, ExtendMutationWithUmbracoFormsMutation>();
        }

        public void Terminate()
        {
        }
    }
}
