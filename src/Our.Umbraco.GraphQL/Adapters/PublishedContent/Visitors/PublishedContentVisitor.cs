using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Our.Umbraco.GraphQL.Adapters.PublishedContent.Types;
using Our.Umbraco.GraphQL.Adapters.Types;
using Our.Umbraco.GraphQL.Adapters.Types.Relay;
using Our.Umbraco.GraphQL.Adapters.Types.Resolution;
using Our.Umbraco.GraphQL.Adapters.Visitors;
using Our.Umbraco.GraphQL.Types;
using Our.Umbraco.GraphQL.Types.PublishedContent;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

namespace Our.Umbraco.GraphQL.Adapters.PublishedContent.Visitors
{
    public class PublishedContentVisitor : GraphVisitor
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IPublishedContentTypeFactory _publishedContentTypeFactory;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IPublishedRouter _publishedRouter;
        private readonly ITypeRegistry _typeRegistry;
        private readonly Lazy<IGraphTypeAdapter> _graphTypeAdapter;
        private readonly Lazy<IGraphVisitor> _visitor;

        public PublishedContentVisitor(IContentTypeService contentTypeService, IMediaTypeService mediaTypeService,
            IPublishedContentTypeFactory publishedContentTypeFactory, IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IUmbracoContextFactory umbracoContextFactory, IPublishedRouter publishedRouter,
            ITypeRegistry typeRegistry, Lazy<IGraphTypeAdapter> graphTypeAdapter, Lazy<IGraphVisitor> visitor)
        {
            _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
            _mediaTypeService = mediaTypeService ?? throw new ArgumentNullException(nameof(mediaTypeService));
            _publishedContentTypeFactory = publishedContentTypeFactory ??
                                           throw new ArgumentNullException(nameof(publishedContentTypeFactory));
            _publishedSnapshotAccessor = publishedSnapshotAccessor ??
                                         throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
            _umbracoContextFactory = umbracoContextFactory ?? throw new ArgumentNullException(nameof(umbracoContextFactory));
            _publishedRouter = publishedRouter ?? throw new ArgumentNullException(nameof(publishedRouter));
            _typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
            _graphTypeAdapter = graphTypeAdapter ?? throw new ArgumentNullException(nameof(graphTypeAdapter));
            _visitor = visitor ?? throw new ArgumentNullException(nameof(visitor));
        }

        public override void Visit(ISchema schema)
        {
            var contentTypeList = _contentTypeService.GetAll().Cast<IContentTypeComposition>()
                .Concat(_mediaTypeService.GetAll()).ToList();
            var compositions = contentTypeList.SelectMany(x => x.ContentTypeComposition).Distinct().ToList();

            var atRootQuery =
                (ObjectGraphType<PublishedContentAtRootQuery>) _graphTypeAdapter.Value
                    .Adapt<PublishedContentAtRootQuery>();
            var byTypeQuery =
                (ObjectGraphType<PublishedContentByTypeQuery>) _graphTypeAdapter.Value
                    .Adapt<PublishedContentByTypeQuery>();

            var compositionGraphTypes = new Dictionary<string, IInterfaceGraphType>();

            foreach (var contentType in compositions)
            {
                var publishedContentType = _publishedContentTypeFactory.CreateContentType(contentType);

                var compositionGraphType =
                    new PublishedContentCompositionGraphType(contentType, publishedContentType, _typeRegistry, _umbracoContextFactory, _publishedRouter);
                compositionGraphTypes.Add(contentType.Alias, compositionGraphType);
                _visitor.Value.Visit(compositionGraphType);
                schema.RegisterType(compositionGraphType);
            }

            foreach (var contentType in contentTypeList.Except(compositions))
            {
                var publishedContentType = _publishedContentTypeFactory.CreateContentType(contentType);

                IObjectGraphType graphType;
                if (publishedContentType.IsElement)
                {
                    graphType = new PublishedElementGraphType(contentType, publishedContentType, _typeRegistry);
                }
                else
                {
                    graphType = new PublishedContentGraphType(contentType, publishedContentType, _typeRegistry, _umbracoContextFactory, _publishedRouter);
                    foreach (var composition in contentType.ContentTypeComposition)
                    {
                        graphType.AddResolvedInterface(compositionGraphTypes[composition.Alias]);
                    }

                    if (publishedContentType.ItemType == PublishedItemType.Content)
                    {
                        if (contentType.AllowedAsRoot)
                        {
                            CreateContentField(atRootQuery, graphType, publishedContentType, (cache, culture) =>
                                cache.GetAtRoot(culture).Where(x => x.ContentType.Alias == publishedContentType.Alias));
                        }

                        CreateContentField(byTypeQuery, graphType, publishedContentType, (cache, culture) =>
                            cache.GetByContentType(publishedContentType)
                                .Where(x => culture == null || x.IsInvariantOrHasCulture(culture)));
                    }

                }

                _visitor.Value.Visit(graphType);
                schema.RegisterType(graphType);
            }
        }

        private void CreateContentField<T>(ComplexGraphType<T> query, IComplexGraphType graphType,
            IPublishedContentType publishedContentType,
            Func<IPublishedContentCache, string, IEnumerable<IPublishedContent>> fetch)
        {
            query.Connection<PublishedContentInterfaceGraphType>().Name(publishedContentType.Alias)
                .Argument<StringGraphType>("culture", "The culture.")
                .Bidirectional()
                .Resolve(ctx =>
                {
                    var items = fetch(_publishedSnapshotAccessor.PublishedSnapshot.Content,
                        ctx.GetArgument<string>("culture")).ToList();

                    return items.OrderBy(ctx.GetArgument<IEnumerable<OrderBy>>("orderBy"))
                        .ToConnection(x => x.Key, ctx.First, ctx.After, ctx.Last, ctx.Before, items.Count);
                });

            var connectionField = query.GetField(publishedContentType.Alias);
            connectionField.ResolvedType = new ConnectionGraphType(graphType);
            connectionField.Arguments.Add(
                new QueryArgument(new ListGraphType(new NonNullGraphType(new OrderByGraphType(graphType))))
                    {Name = "orderBy"});
        }
    }
}
