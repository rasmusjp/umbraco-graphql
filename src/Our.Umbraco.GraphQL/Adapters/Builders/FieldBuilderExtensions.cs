using GraphQL.Builders;
using GraphQL.Types;
using Our.Umbraco.GraphQL.Adapters.Types;

namespace Our.Umbraco.GraphQL.Adapters.Builders
{
    public static class FieldBuilderExtensions
    {
        public static FieldBuilder<TSourceType, object> Metadata<TSourceType>(
            this FieldBuilder<TSourceType, object> builder, string key, object value)
        {
            builder.FieldType.Metadata.Add(key, value);
            return builder;
        }

        public static ConnectionBuilder<TNodeType, TSourceType> Metadata<TNodeType, TSourceType>(
            this ConnectionBuilder<TNodeType, TSourceType> builder, string key, object value)
            where TNodeType : IGraphType
        {
            builder.FieldType.Metadata.Add(key, value);
            return builder;
        }

        public static ConnectionBuilder<TNodeType, TSourceType> Orderable<TNodeType, TSourceType>(
            this ConnectionBuilder<TNodeType, TSourceType> builder) where TNodeType : IComplexGraphType
        {
            builder.Argument<ListGraphType<NonNullGraphType<OrderByGraphType<TNodeType>>>>("orderBy", "");
            return builder;
        }
    }
}
