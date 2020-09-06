using GraphQL;
using GraphQL.Http;
using Our.Umbraco.GraphQL.Adapters;
using Our.Umbraco.GraphQL.Adapters.Types.Resolution;
using Our.Umbraco.GraphQL.Builders;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Our.Umbraco.GraphQL.Adapters.Visitors;
using Our.Umbraco.GraphQL.Composing;
using System.Linq;
using Our.Umbraco.GraphQL.Web;
using Newtonsoft.Json;
using Our.Umbraco.GraphQL.Adapters.PublishedContent.Visitors;
using GraphQL.DataLoader;
using Our.Umbraco.GraphQL.FieldMiddleware;
using Our.Umbraco.GraphQL.Adapters.Examine.Visitors;

namespace Our.Umbraco.GraphQL.Compose
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class GraphQLComposer : ComponentComposer<GraphQLComponent>, IUserComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.Register<ITypeRegistry, TypeRegistry>(Lifetime.Singleton);
            composition.Register<IGraphTypeAdapter, GraphTypeAdapter>(Lifetime.Scope);
            composition.Register<ISchemaBuilder, SchemaBuilder>(Lifetime.Scope);

            composition.GraphVisitors()
                .Append<PublishedContentVisitor>()
                .Append<ExamineVisitor>();

            composition.FieldMiddlewares()
                .Append<EnsureHttpContextFieldMiddleware>();

            composition.Register(new GraphQLRequestParser(new JsonSerializer()));
            composition.Register<IGraphVisitor>(factory => new CompositeGraphVisitor(factory.GetInstance<GraphVisitorCollection>().ToArray()));

            composition.Register<IDependencyResolver>(factory =>
                new FuncDependencyResolver(type => factory.TryGetInstance(type) ?? factory.CreateInstance(type)), Lifetime.Singleton);
            composition.Register<IDocumentWriter, DocumentWriter>(Lifetime.Singleton);

            composition.Register<IDataLoaderContextAccessor, DataLoaderContextAccessor>(Lifetime.Singleton);
            composition.Register<DataLoaderDocumentListener>(Lifetime.Singleton);
        }
    }
}
