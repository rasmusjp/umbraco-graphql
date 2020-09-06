using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Examine;
using GraphQL;
using GraphQL.Types;
using Our.Umbraco.GraphQL.Adapters.Builders;
using Our.Umbraco.GraphQL.Types;
using Umbraco.Web.PublishedCache;

namespace Our.Umbraco.GraphQL.Adapters.Examine.Types
{
    internal static class ExamineGraphTypeExtensions
    {
        private static readonly Regex InvalidNameCharacters = new Regex("[^A-Za-z0-9_]", RegexOptions.Compiled);
        private static readonly Regex InvalidNameStart = new Regex("^([0-9]|__)", RegexOptions.Compiled);

        public static string SafeName(this string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "unknown";

            var n = InvalidNameCharacters.Replace(name, "").Trim();
            while (InvalidNameStart.IsMatch(n)) n = n.Substring(1);
            if (n.Length == 0) return "invalid";

            return n.ToPascalCase();
        }

        public static void AddBuiltInFields(this ComplexGraphType<ISearchResult> graphType, bool includeFields = false)
        {
            graphType.Field<FloatGraphType>().Name("score")
                .Metadata(nameof(MemberInfo), GetMember((ISearchResult x) => x.Score))
                .Resolve(ctx => ctx.Source.Score);

            graphType.Field<StringGraphType>().Name("id")
                .Metadata(nameof(MemberInfo), GetMember((ISearchResult x) => x.Id))
                .Resolve(ctx => ctx.Source.Id);

            if (includeFields)
            {
                graphType.Connection<SearchResultFieldsGraphType>().Name("fields")
                    .Metadata(nameof(MemberInfo), GetMember((ISearchResult x) => x.AllValues))
                    .Bidirectional()
                    .Orderable()
                    .Resolve(ctx =>
                        ctx.Source.AllValues
                            .OrderBy(ctx.GetArgument<IList<OrderBy>>("orderBy"))
                            .ToConnection(x => x.Key, ctx.First, ctx.After, ctx.Last, ctx.Before));
            }
        }

        public static void AddBuiltInFields(this ComplexGraphType<ISearchResults> graphType, IPublishedSnapshotAccessor snapshotAccessor = null, string searcherSafeName = null, IEnumerable<string> fields = null)
        {
            graphType.Field<FloatGraphType>().Name("total")
                .Metadata(nameof(MemberInfo), GetMember((ISearchResults x) => x.TotalItemCount))
                .Resolve(ctx => ctx.Source.TotalItemCount);

            graphType.Field<ListGraphType<SearchResultInterfaceGraphType>>().Name("results")
                .Metadata(nameof(MemberInfo), GetMember((ISearchResults x) => x.ToList()))
                .Resolve(ctx => ctx.Source);

            if (snapshotAccessor != null && searcherSafeName != null)
                graphType.GetField("results").ResolvedType = new ListGraphType(new SearchResultGraphType(snapshotAccessor, searcherSafeName, fields));
        }

        public static void AddBuiltInFields(this ComplexGraphType<KeyValuePair<string, IReadOnlyList<string>>> graphType)
        {
            graphType.Connection<StringGraphType>().Name("values")
                .Metadata(nameof(MemberInfo), GetMember((KeyValuePair<string, IReadOnlyList<string>> x) => x.Value))
                .Bidirectional()
                .Resolve(ctx =>
                    ctx.Source.Value
                        .ToConnection(x => x, ctx.First, ctx.After, ctx.Last, ctx.Before));
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
