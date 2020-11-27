using System;
using FluentAssertions;
using GraphQL.Types;
using GraphQL.Types.Relay;
using NSubstitute;
using Our.Umbraco.GraphQL.Adapters.PublishedContent.Types;
using Our.Umbraco.GraphQL.Adapters.Types.Resolution;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Xunit;
using IdGraphType = Our.Umbraco.GraphQL.Adapters.Types.IdGraphType;

namespace Our.Umbraco.GraphQL.Tests.Adapters.PublishedContent.Types
{
    public class PublishedContentCompositionGraphTypeTests
    {
        private PublishedContentCompositionGraphType CreateSUT(IContentTypeComposition contentType = null)
        {
            return new PublishedContentCompositionGraphType(contentType ?? Substitute.For<IContentTypeComposition>(),
                Substitute.For<IPublishedContentType>(), new TypeRegistry(), Substitute.For<IUmbracoContextFactory>(), Substitute.For<IPublishedRouter>());
        }

        [Theory]
        [InlineData(typeof(IContentType), "person", "PersonPublishedContent")]
        [InlineData(typeof(IMediaType), "image", "ImagePublishedMedia")]
        [InlineData(typeof(IContentType), "feature", "FeaturePublishedElement", true)]
        public void Ctor_SetsName(Type type, string alias, string expectedName, bool isElement = false)
        {
            var contentType = (IContentTypeComposition) Substitute.For(new[] {type}, null);
            contentType.Alias.Returns(alias);
            contentType.IsElement.Returns(isElement);

            var graphType = CreateSUT(contentType);

            graphType.Name.Should().Be(expectedName);
        }

        [Theory]
        [InlineData("_ancestors", typeof(ConnectionType<PublishedContentInterfaceGraphType>))]
        [InlineData("_children", typeof(ConnectionType<PublishedContentInterfaceGraphType>))]
        [InlineData("_createDate", typeof(NonNullGraphType<DateTimeGraphType>))]
        [InlineData("_creatorName", typeof(NonNullGraphType<StringGraphType>))]
        [InlineData("_contentType", typeof(NonNullGraphType<PublishedContentTypeGraphType>))]
        [InlineData("_id", typeof(NonNullGraphType<IdGraphType>))]
        [InlineData("_level", typeof(NonNullGraphType<IntGraphType>))]
        [InlineData("_name", typeof(StringGraphType))]
        [InlineData("_parent", typeof(PublishedContentInterfaceGraphType))]
        [InlineData("_sortOrder", typeof(NonNullGraphType<IntGraphType>))]
        [InlineData("_url", typeof(StringGraphType))]
        [InlineData("_updateDate", typeof(DateTimeGraphType))]
        [InlineData("_writerName", typeof(NonNullGraphType<StringGraphType>))]
        public void Fields_Type_ShouldBeOfExpectedType(string field, Type type)
        {
            var graphType = CreateSUT();

            graphType.Fields.Should().Contain(x => x.Name == field)
                .Which.Type.Should().Be(type);
        }

        [Theory]
        [InlineData("_name", "culture", typeof(StringGraphType))]
        [InlineData("_url", "culture", typeof(StringGraphType))]
        [InlineData("_url", "mode", typeof(UrlModeGraphType))]
        [InlineData("_updateDate", "culture", typeof(StringGraphType))]
        public void Fields_Arguments_ShouldBeOfExpectedType(string field, string argument, Type type)
        {
            var graphType = CreateSUT();

            graphType.Fields.Should().Contain(x => x.Name == field)
                .Which.Arguments.Should().Contain(x => x.Name == argument)
                .Which.Type.Should().Be(type);
        }
    }
}

