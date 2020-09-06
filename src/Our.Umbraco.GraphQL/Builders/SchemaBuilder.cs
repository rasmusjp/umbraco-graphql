using System;
using System.Reflection;
using GraphQL;
using GraphQL.Types;
using Our.Umbraco.GraphQL.Adapters;
using Our.Umbraco.GraphQL.Adapters.Visitors;

namespace Our.Umbraco.GraphQL.Builders
{
    public class SchemaBuilder : ISchemaBuilder
    {
        private readonly IGraphTypeAdapter _graphTypeAdapter;
        private readonly IGraphVisitor _visitor;
        private readonly IDependencyResolver _dependencyResolver;

        public SchemaBuilder(IGraphTypeAdapter graphTypeAdapter, IDependencyResolver dependencyResolver, IGraphVisitor visitor)
        {
            _graphTypeAdapter = graphTypeAdapter ?? throw new ArgumentNullException(nameof(graphTypeAdapter));
            _dependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
            _visitor = visitor;
        }

        public ISchema Build<TSchema>(params TypeInfo[] additionalTypes) => Build(typeof(TSchema).GetTypeInfo(), additionalTypes);

        public ISchema Build(TypeInfo schemaType, params TypeInfo[] additionalTypes)
        {
            if (schemaType == null) throw new ArgumentNullException(nameof(schemaType));

            var schema = new Schema(_dependencyResolver);

            schema.Query = GenerateFromProperty(schemaType, "Query", true);

            foreach (var type in additionalTypes)
            {
                schema.RegisterType(_graphTypeAdapter.Adapt(type));
            }

            _visitor?.Visit(schema);

            return schema;
        }

        private IObjectGraphType GenerateFromProperty(TypeInfo schemaType, string propertyName, bool throwError)
        {
            var queryPropertyInfo = schemaType.GetProperty(propertyName);
            if (queryPropertyInfo == null)
            {
                if (throwError) throw new ArgumentException($"Could not find property 'Query' on {schemaType}.", nameof(schemaType));
                return null;
            }

            if (queryPropertyInfo.CanRead == false)
            {
                if (throwError) throw new ArgumentException("'Query' does not have a getter.", nameof(schemaType));
                return null;
            }

            return (IObjectGraphType)_graphTypeAdapter.Adapt(queryPropertyInfo.GetMethod.ReturnType.GetTypeInfo());
        }
    }
}
