using GraphQL.Types;
using Newtonsoft.Json;
using Our.Umbraco.GraphQL.Adapters.Builders;
using Our.Umbraco.GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Data.Storage;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Interfaces;
using Umbraco.Forms.Core.Models;
using Umbraco.Forms.Data.Storage;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    public static class UmbracoFormsGraphTypeExtensions
    {
        public static void AddBuiltinFields(this ComplexGraphType<Form> graphType, IDataSourceStorage dataSourceStorage = null, IWorkflowStorage workflowStorage = null)
        {
            graphType.Field<StringGraphType>().Name("name").Metadata(nameof(MemberInfo), GetMember((Form x) => x.Name)).Resolve(ctx => ctx.Source.Name);
            graphType.Field<NonNullGraphType<DateTimeGraphType>>().Name("created").Metadata(nameof(MemberInfo), GetMember((Form x) => x.Created)).Resolve(ctx => ctx.Source.Created);
            graphType.Field<ListGraphType<PageGraphType>>().Name("pages").Metadata(nameof(MemberInfo), GetMember((Form x) => x.Pages)).Resolve(ctx => ctx.Source.Pages);
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("id").Metadata(nameof(MemberInfo), GetMember((Form x) => x.Id)).Resolve(ctx => ctx.Source.Id);
            graphType.Field<NonNullGraphType<EnumerationGraphType<FormFieldIndication>>>().Name("fieldIndicationType").Metadata(nameof(MemberInfo), GetMember((Form x) => x.FieldIndicationType)).Resolve(ctx => ctx.Source.FieldIndicationType);
            graphType.Field<StringGraphType>().Name("indicator").Metadata(nameof(MemberInfo), GetMember((Form x) => x.Indicator)).Resolve(ctx => ctx.Source.Indicator);
            graphType.Field<NonNullGraphType<BooleanGraphType>>().Name("showValidationSummary").Metadata(nameof(MemberInfo), GetMember((Form x) => x.ShowValidationSummary)).Resolve(ctx => ctx.Source.ShowValidationSummary);
            graphType.Field<NonNullGraphType<BooleanGraphType>>().Name("hideFieldValidation").Metadata(nameof(MemberInfo), GetMember((Form x) => x.HideFieldValidation)).Resolve(ctx => ctx.Source.HideFieldValidation);
            graphType.Field<StringGraphType>().Name("requiredErrorMessage").Metadata(nameof(MemberInfo), GetMember((Form x) => x.RequiredErrorMessage)).Resolve(ctx => ctx.Source.RequiredErrorMessage);
            graphType.Field<StringGraphType>().Name("invalidErrorMessage").Metadata(nameof(MemberInfo), GetMember((Form x) => x.InvalidErrorMessage)).Resolve(ctx => ctx.Source.InvalidErrorMessage);
            graphType.Field<StringGraphType>().Name("messageOnSubmit").Metadata(nameof(MemberInfo), GetMember((Form x) => x.MessageOnSubmit)).Resolve(ctx => ctx.Source.MessageOnSubmit);
            graphType.Field<NonNullGraphType<IntGraphType>>().Name("goToPageOnSubmit").Metadata(nameof(MemberInfo), GetMember((Form x) => x.GoToPageOnSubmit)).Resolve(ctx => ctx.Source.GoToPageOnSubmit);
            graphType.Field<StringGraphType>().Name("xPathOnSubmit").Metadata(nameof(MemberInfo), GetMember((Form x) => x.XPathOnSubmit)).Resolve(ctx => ctx.Source.XPathOnSubmit);
            graphType.Field<NonNullGraphType<BooleanGraphType>>().Name("manualApproval").Metadata(nameof(MemberInfo), GetMember((Form x) => x.ManualApproval)).Resolve(ctx => ctx.Source.ManualApproval);
            graphType.Field<NonNullGraphType<BooleanGraphType>>().Name("storeRecordsLocally").Metadata(nameof(MemberInfo), GetMember((Form x) => x.StoreRecordsLocally)).Resolve(ctx => ctx.Source.StoreRecordsLocally);
            graphType.Field<StringGraphType>().Name("cssClass").Metadata(nameof(MemberInfo), GetMember((Form x) => x.CssClass)).Resolve(ctx => ctx.Source.CssClass);
            graphType.Field<NonNullGraphType<BooleanGraphType>>().Name("disableDefaultStylesheet").Metadata(nameof(MemberInfo), GetMember((Form x) => x.DisableDefaultStylesheet)).Resolve(ctx => ctx.Source.DisableDefaultStylesheet);
            graphType.Field<NonNullGraphType<BooleanGraphType>>().Name("useClientDependency").Metadata(nameof(MemberInfo), GetMember((Form x) => x.UseClientDependency)).Resolve(ctx => ctx.Source.UseClientDependency);
            graphType.Field<ListGraphType<GuidGraphType>>().Name("workflowIds").Metadata(nameof(MemberInfo), GetMember((Form x) => x.WorkflowIds)).Resolve(ctx => ctx.Source.WorkflowIds);
            graphType.Field<FormDataSourceDefinitionGraphType>().Name("dataSourceDefinition").Metadata(nameof(MemberInfo), GetMember((Form x) => x.DataSource)).Resolve(ctx => ctx.Source.DataSource);
            graphType.Field<StringGraphType>().Name("submitLabel").Metadata(nameof(MemberInfo), GetMember((Form x) => x.SubmitLabel)).Resolve(ctx => ctx.Source.SubmitLabel);
            graphType.Field<StringGraphType>().Name("nextLabel").Metadata(nameof(MemberInfo), GetMember((Form x) => x.NextLabel)).Resolve(ctx => ctx.Source.NextLabel);
            graphType.Field<StringGraphType>().Name("prevLabel").Metadata(nameof(MemberInfo), GetMember((Form x) => x.PrevLabel)).Resolve(ctx => ctx.Source.PrevLabel);

            graphType.Field<FormDataSourceGraphType>().Name("_dataSource").Resolve(ctx => ctx.Source.DataSource != null && dataSourceStorage != null ? dataSourceStorage.GetDataSource(ctx.Source.DataSource.Id) : null);

            graphType.Connection<WorkflowGraphType>().Name("_workflows")
                .Bidirectional()
                .Resolve(ctx => (workflowStorage == null ? new List<IWorkflow>() : (ctx.Source.WorkflowIds ?? new List<Guid>()).Select(i => workflowStorage.GetWorkflow(i)).ToList())
                    .ToConnection(x => x.Id, ctx.First, ctx.After, ctx.Last, ctx.Before));
        }

        public static void AddBuiltinFields(this ComplexGraphType<Page> graphType)
        {
            graphType.Field<ListGraphType<FieldSetGraphType>>().Name("FieldSets").Metadata(nameof(MemberInfo), GetMember((Page x) => x.FieldSets)).Resolve(ctx => ctx.Source.FieldSets);
            graphType.Field<StringGraphType>().Name("Caption").Metadata(nameof(MemberInfo), GetMember((Page x) => x.Caption)).Resolve(ctx => ctx.Source.Caption);
            graphType.Field<NonNullGraphType<IntGraphType>>().Name("SortOrder").Metadata(nameof(MemberInfo), GetMember((Page x) => x.SortOrder)).Resolve(ctx => ctx.Source.SortOrder);
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("Id").Metadata(nameof(MemberInfo), GetMember((Page x) => x.Id)).Resolve(ctx => ctx.Source.Id);
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("Form").Metadata(nameof(MemberInfo), GetMember((Page x) => x.Form)).Resolve(ctx => ctx.Source.Form);
        }

        public static void AddBuiltinFields(this ComplexGraphType<FormDataSourceDefinition> graphType)
        {
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("Id").Metadata(nameof(MemberInfo), GetMember((FormDataSourceDefinition x) => x.Id)).Resolve(ctx => ctx.Source.Id);
            graphType.Field<ListGraphType<FormDataSourceMappingGraphType>>().Name("Mappings").Metadata(nameof(MemberInfo), GetMember((FormDataSourceDefinition x) => x.Mappings)).Resolve(ctx => ctx.Source.Mappings);
        }

        public static void AddBuiltinFields(this ComplexGraphType<FieldSet> graphType)
        {
            graphType.Field<StringGraphType>().Name("Caption").Metadata(nameof(MemberInfo), GetMember((FieldSet x) => x.Caption)).Resolve(ctx => ctx.Source.Caption);
            graphType.Field<NonNullGraphType<IntGraphType>>().Name("SortOrder").Metadata(nameof(MemberInfo), GetMember((FieldSet x) => x.SortOrder)).Resolve(ctx => ctx.Source.SortOrder);
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("Id").Metadata(nameof(MemberInfo), GetMember((FieldSet x) => x.Id)).Resolve(ctx => ctx.Source.Id);
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("Page").Metadata(nameof(MemberInfo), GetMember((FieldSet x) => x.Page)).Resolve(ctx => ctx.Source.Page);
            graphType.Field<ListGraphType<FieldsetContainerGraphType>>().Name("Containers").Metadata(nameof(MemberInfo), GetMember((FieldSet x) => x.Containers)).Resolve(ctx => ctx.Source.Containers);
            graphType.Field<FieldConditionGraphType>().Name("Condition").Metadata(nameof(MemberInfo), GetMember((FieldSet x) => x.Condition)).Resolve(ctx => ctx.Source.Condition);
        }

        public static void AddBuiltinFields(this ComplexGraphType<FormDataSourceMapping> graphType)
        {
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("formId").Metadata(nameof(MemberInfo), GetMember((FormDataSourceMapping x) => x.FormId)).Resolve(ctx => ctx.Source.FormId);
            graphType.Field<StringGraphType>().Name("dataFieldKey").Metadata(nameof(MemberInfo), GetMember((FormDataSourceMapping x) => x.DataFieldKey)).Resolve(ctx => JsonConvert.SerializeObject(ctx.Source.DataFieldKey));
            graphType.Field<StringGraphType>().Name("prevalueKeyField").Metadata(nameof(MemberInfo), GetMember((FormDataSourceMapping x) => x.PrevalueKeyfield)).Resolve(ctx => ctx.Source.PrevalueKeyfield);
            graphType.Field<StringGraphType>().Name("prevalueValueField").Metadata(nameof(MemberInfo), GetMember((FormDataSourceMapping x) => x.PrevalueValueField)).Resolve(ctx => ctx.Source.PrevalueValueField);
            graphType.Field<StringGraphType>().Name("prevalueTable").Metadata(nameof(MemberInfo), GetMember((FormDataSourceMapping x) => x.PrevalueTable)).Resolve(ctx => ctx.Source.PrevalueTable);
            graphType.Field<NonNullGraphType<EnumerationGraphType<FieldDataType>>>().Name("dataType").Metadata(nameof(MemberInfo), GetMember((FormDataSourceMapping x) => x.DataType)).Resolve(ctx => ctx.Source.DataType);
            graphType.Field<StringGraphType>().Name("defaultValue").Metadata(nameof(MemberInfo), GetMember((FormDataSourceMapping x) => x.DefaultValue)).Resolve(ctx => ctx.Source.DefaultValue);
        }

        public static void AddBuiltinFields(this ComplexGraphType<FieldsetContainer> graphType)
        {
            graphType.Field<StringGraphType>().Name("caption").Metadata(nameof(MemberInfo), GetMember((FieldsetContainer x) => x.Caption)).Resolve(ctx => ctx.Source.Caption);
            graphType.Field<NonNullGraphType<IntGraphType>>().Name("width").Metadata(nameof(MemberInfo), GetMember((FieldsetContainer x) => x.Width)).Resolve(ctx => ctx.Source.Width);
            graphType.Field<ListGraphType<FieldGraphType>>().Name("fields").Metadata(nameof(MemberInfo), GetMember((FieldsetContainer x) => x.Fields)).Resolve(ctx => ctx.Source.Fields);
        }

        public static void AddBuiltinFields(this ComplexGraphType<FieldCondition> graphType)
        {
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("id").Metadata(nameof(MemberInfo), GetMember((FieldCondition x) => x.Id)).Resolve(ctx => ctx.Source.Id);
            graphType.Field<NonNullGraphType<BooleanGraphType>>().Name("enabled").Metadata(nameof(MemberInfo), GetMember((FieldCondition x) => x.Enabled)).Resolve(ctx => ctx.Source.Enabled);
            graphType.Field<NonNullGraphType<EnumerationGraphType<FieldConditionActionType>>>().Name("actionType").Metadata(nameof(MemberInfo), GetMember((FieldCondition x) => x.ActionType)).Resolve(ctx => ctx.Source.ActionType);
            graphType.Field<NonNullGraphType<EnumerationGraphType<FieldConditionLogicType>>>().Name("logicType").Metadata(nameof(MemberInfo), GetMember((FieldCondition x) => x.LogicType)).Resolve(ctx => ctx.Source.LogicType);
            graphType.Field<ListGraphType<FieldConditionRuleGraphType>>().Name("rules").Metadata(nameof(MemberInfo), GetMember((FieldCondition x) => x.Rules)).Resolve(ctx => ctx.Source.Rules);
        }

        public static void AddBuiltinFields(this ComplexGraphType<Field> graphType, IPrevalueSourceStorage prevalueSourceStorage = null)
        {
            graphType.Field<StringGraphType>().Name("caption").Metadata(nameof(MemberInfo), GetMember((Field x) => x.Caption)).Resolve(ctx => ctx.Source.Caption);
            graphType.Field<StringGraphType>().Name("tooltip").Metadata(nameof(MemberInfo), GetMember((Field x) => x.ToolTip)).Resolve(ctx => ctx.Source.ToolTip);
            graphType.Field<StringGraphType>().Name("placeholder").Metadata(nameof(MemberInfo), GetMember((Field x) => x.Placeholder)).Resolve(ctx => ctx.Source.Placeholder);
            graphType.Field<StringGraphType>().Name("cssClass").Metadata(nameof(MemberInfo), GetMember((Field x) => x.CssClass)).Resolve(ctx => ctx.Source.CssClass);
            graphType.Field<StringGraphType>().Name("alias").Metadata(nameof(MemberInfo), GetMember((Field x) => x.Alias)).Resolve(ctx => ctx.Source.Alias);
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("id").Metadata(nameof(MemberInfo), GetMember((Field x) => x.Id)).Resolve(ctx => ctx.Source.Id);
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("fieldTypeId").Metadata(nameof(MemberInfo), GetMember((Field x) => x.FieldTypeId)).Resolve(ctx => ctx.Source.FieldTypeId);
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("prevalueSourceId").Metadata(nameof(MemberInfo), GetMember((Field x) => x.PreValueSourceId)).Resolve(ctx => ctx.Source.PreValueSourceId);
            graphType.Field<StringGraphType>().Name("dataSourceFieldKey").Metadata(nameof(MemberInfo), GetMember((Field x) => x.DataSourceFieldKey)).Resolve(ctx => JsonConvert.SerializeObject(ctx.Source.DataSourceFieldKey));
            graphType.Field<NonNullGraphType<BooleanGraphType>>().Name("containsSensitiveData").Metadata(nameof(MemberInfo), GetMember((Field x) => x.ContainsSensitiveData)).Resolve(ctx => ctx.Source.ContainsSensitiveData);
            graphType.Field<NonNullGraphType<BooleanGraphType>>().Name("mandatory").Metadata(nameof(MemberInfo), GetMember((Field x) => x.Mandatory)).Resolve(ctx => ctx.Source.Mandatory);
            graphType.Field<StringGraphType>().Name("regex").Metadata(nameof(MemberInfo), GetMember((Field x) => x.RegEx)).Resolve(ctx => ctx.Source.RegEx);
            graphType.Field<StringGraphType>().Name("requiredErrorMessage").Metadata(nameof(MemberInfo), GetMember((Field x) => x.RequiredErrorMessage)).Resolve(ctx => ctx.Source.RequiredErrorMessage);
            graphType.Field<StringGraphType>().Name("invalidErrorMessage").Metadata(nameof(MemberInfo), GetMember((Field x) => x.InvalidErrorMessage)).Resolve(ctx => ctx.Source.InvalidErrorMessage);
            graphType.Field<FieldConditionGraphType>().Name("condition").Metadata(nameof(MemberInfo), GetMember((Field x) => x.Condition)).Resolve(ctx => ctx.Source.Condition);
            graphType.Field<ListGraphType<StringKeyValuePairGraphType>>().Name("settings").Metadata(nameof(MemberInfo), GetMember((Field x) => x.Settings)).Resolve(ctx => ctx.Source.Settings);
            graphType.Field<ListGraphType<StringGraphType>>().Name("preValues").Metadata(nameof(MemberInfo), GetMember((Field x) => x.PreValues)).Resolve(ctx => ctx.Source.PreValues);

            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("_prevalueSource").Resolve(ctx => !Guid.Empty.Equals(ctx.Source.PreValueSourceId) && prevalueSourceStorage != null ? prevalueSourceStorage.GetPrevalueSource(ctx.Source.PreValueSourceId) : null);
        }

        public static void AddBuiltinFields(this ComplexGraphType<FieldConditionRule> graphType)
        {
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("id").Metadata(nameof(MemberInfo), GetMember((FieldConditionRule x) => x.Id)).Resolve(ctx => ctx.Source.Id);
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("field").Metadata(nameof(MemberInfo), GetMember((FieldConditionRule x) => x.Field)).Resolve(ctx => ctx.Source.Field);
            graphType.Field<NonNullGraphType<EnumerationGraphType<FieldConditionRuleOperator>>>().Name("operator").Metadata(nameof(MemberInfo), GetMember((FieldConditionRule x) => x.Operator)).Resolve(ctx => ctx.Source.Operator);
            graphType.Field<StringGraphType>().Name("value").Metadata(nameof(MemberInfo), GetMember((FieldConditionRule x) => x.Value)).Resolve(ctx => ctx.Source.Value);
        }

        public static void AddBuiltinFields(this ComplexGraphType<KeyValuePair<string, string>> graphType)
        {
            graphType.Field<StringGraphType>().Name("key").Metadata(nameof(MemberInfo), GetMember((KeyValuePair<string, string> x) => x.Key)).Resolve(ctx => ctx.Source.Key);
            graphType.Field<StringGraphType>().Name("value").Metadata(nameof(MemberInfo), GetMember((KeyValuePair<string, string> x) => x.Value)).Resolve(ctx => ctx.Source.Value);
        }

        public static void AddBuiltinFields(this ComplexGraphType<FormDataSource> graphType)
        {
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("id").Metadata(nameof(MemberInfo), GetMember((FormDataSource x) => x.Id)).Resolve(ctx => ctx.Source.Id);
            graphType.Field<StringGraphType>().Name("name").Metadata(nameof(MemberInfo), GetMember((FormDataSource x) => x.Name)).Resolve(ctx => ctx.Source.Name);
            graphType.Field<ListGraphType<StringKeyValuePairGraphType>>().Name("settings").Metadata(nameof(MemberInfo), GetMember((FormDataSource x) => x.Settings)).Resolve(ctx => ctx.Source.Settings);
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("formDataSourceTypeId").Metadata(nameof(MemberInfo), GetMember((FormDataSource x) => x.FormDataSourceTypeId)).Resolve(ctx => ctx.Source.FormDataSourceTypeId);
            graphType.Field<NonNullGraphType<BooleanGraphType>>().Name("valid").Metadata(nameof(MemberInfo), GetMember((FormDataSource x) => x.Valid)).Resolve(ctx => ctx.Source.Valid);
        }

        public static void AddBuiltinFields(this ComplexGraphType<IWorkflow> graphType)
        {
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("Id").Metadata(nameof(MemberInfo), GetMember((IWorkflow x) => x.Id)).Resolve(ctx => ctx.Source.Id);
            graphType.Field<StringGraphType>().Name("Name").Metadata(nameof(MemberInfo), GetMember((IWorkflow x) => x.Name)).Resolve(ctx => ctx.Source.Name);
            graphType.Field<NonNullGraphType<BooleanGraphType>>().Name("Active").Metadata(nameof(MemberInfo), GetMember((IWorkflow x) => x.Active)).Resolve(ctx => ctx.Source.Active);
            graphType.Field<NonNullGraphType<EnumerationGraphType<IncludeSensitiveData>>>().Name("IncludeSensitiveData").Metadata(nameof(MemberInfo), GetMember((IWorkflow x) => x.IncludeSensitiveData)).Resolve(ctx => ctx.Source.IncludeSensitiveData);
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("WorkflowTypeId").Metadata(nameof(MemberInfo), GetMember((IWorkflow x) => x.WorkflowTypeId)).Resolve(ctx => ctx.Source.WorkflowTypeId);
            graphType.Field<NonNullGraphType<EnumerationGraphType<FormState>>>().Name("ExecutesOn").Metadata(nameof(MemberInfo), GetMember((IWorkflow x) => x.ExecutesOn)).Resolve(ctx => ctx.Source.ExecutesOn);
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("Form").Metadata(nameof(MemberInfo), GetMember((IWorkflow x) => x.Form)).Resolve(ctx => ctx.Source.Form);
            graphType.Field<ListGraphType<StringKeyValuePairGraphType>>().Name("Settings").Metadata(nameof(MemberInfo), GetMember((IWorkflow x) => x.Settings)).Resolve(ctx => ctx.Source.Settings);
            graphType.Field<NonNullGraphType<IntGraphType>>().Name("SortOrder").Metadata(nameof(MemberInfo), GetMember((IWorkflow x) => x.SortOrder)).Resolve(ctx => ctx.Source.SortOrder);
        }

        public static void AddBuiltinFields(this ComplexGraphType<IFieldPreValueSource> graphType)
        {
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("Id").Metadata(nameof(MemberInfo), GetMember((IFieldPreValueSource x) => x.Id)).Resolve(ctx => ctx.Source.Id);
            graphType.Field<StringGraphType>().Name("Name").Metadata(nameof(MemberInfo), GetMember((IFieldPreValueSource x) => x.Name)).Resolve(ctx => ctx.Source.Name);
            graphType.Field<ListGraphType<StringKeyValuePairGraphType>>().Name("Settings").Metadata(nameof(MemberInfo), GetMember((IFieldPreValueSource x) => x.Settings)).Resolve(ctx => ctx.Source.Settings);
            graphType.Field<NonNullGraphType<GuidGraphType>>().Name("FieldPreValueSourceTypeId").Metadata(nameof(MemberInfo), GetMember((IFieldPreValueSource x) => x.FieldPreValueSourceTypeId)).Resolve(ctx => ctx.Source.FieldPreValueSourceTypeId);
        }

        private static MemberInfo GetMember<TSource, TReturn>(Expression<Func<TSource, TReturn>> expression)
        {
            switch (expression.Body)
            {
                case MethodCallExpression methodCallExpression:
                    return methodCallExpression.Method;
                case MemberExpression memberExpression:
                    return memberExpression.Member;
                default:
                    throw new ArgumentOutOfRangeException(nameof(expression));
            }
        }
    }
}
