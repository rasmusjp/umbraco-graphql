using Examine;
using GraphQL.Types;
using Our.Umbraco.GraphQL.Adapters.Examine.Types;
using Our.Umbraco.GraphQL.Adapters.Visitors;
using System;
using System.Linq;
using Umbraco.Web.PublishedCache;

namespace Our.Umbraco.GraphQL.Adapters.Examine.Visitors
{
    public class ExamineVisitor : GraphVisitor
    {
        private readonly IPublishedSnapshotAccessor _snapshotAccessor;
        private readonly IExamineManager _examineManager;
        private readonly Lazy<IGraphTypeAdapter> _graphTypeAdapter;
        private readonly Lazy<IGraphVisitor> _visitor;

        public ExamineVisitor(IPublishedSnapshotAccessor snapshotAccessor, IExamineManager examineManager, Lazy<IGraphTypeAdapter> graphTypeAdapter, Lazy<IGraphVisitor> visitor)
        {
            _snapshotAccessor = snapshotAccessor;
            _examineManager = examineManager;
            _graphTypeAdapter = graphTypeAdapter;
            _visitor = visitor;
        }

        public override void Visit(ISchema schema)
        {
            var searchers = _examineManager.Indexes.Select(i => i.GetSearcher()).Concat(_examineManager.RegisteredSearchers).ToList();
            var examineQuery = (ObjectGraphType<ExamineQuery>)_graphTypeAdapter.Value.Adapt<ExamineQuery>();

            foreach (var searcher in searchers)
            {
                var loopCaptured = searcher;
                var searcherSafeName = searcher.Name.SafeName();
                if (searcherSafeName.EndsWith("Searcher")) searcherSafeName = searcherSafeName.Substring(0, searcherSafeName.Length - 8);

                var graphType = new ExamineSearcherGraphType(_snapshotAccessor, loopCaptured, searcherSafeName);
                examineQuery.Field<ExamineSearcherGraphType>().Name(searcherSafeName).Resolve(ctx => new ExamineSearcherQuery());
                examineQuery.GetField(searcherSafeName).ResolvedType = graphType;

                _visitor.Value.Visit(graphType);
                schema.RegisterType(graphType);
            }
        }
    }
}
