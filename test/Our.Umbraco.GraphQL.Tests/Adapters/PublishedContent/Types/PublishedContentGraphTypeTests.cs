using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using GraphQL.Types;
using GraphQL.Types.Relay;
using NSubstitute;
using Our.Umbraco.GraphQL.Adapters.PublishedContent.Types;
using Our.Umbraco.GraphQL.Adapters.Types.Resolution;
using Our.Umbraco.GraphQL.Types;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Xunit;
using IdGraphType = Our.Umbraco.GraphQL.Adapters.Types.IdGraphType;

namespace Our.Umbraco.GraphQL.Tests.Adapters.PublishedContent.Types
{
    public class PublishedContentGraphTypeTests
    {
        private PublishedContentGraphType CreateSUT(IContentTypeComposition contentType = null,
            IPublishedContentType publishedContentType = null)
        {
            return new PublishedContentGraphType(contentType ?? Substitute.For<IContentTypeComposition>(),
                publishedContentType ?? Substitute.For<IPublishedContentType>(), new TypeRegistry(), Substitute.For<IUmbracoContextFactory>(), Substitute.For<IPublishedRouter>());
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

        [Fact]
        public void IsOfType_WhenTypeMatches_ReturnsTrue()
        {
            var contentType = Substitute.For<IContentType>();
            contentType.Alias.Returns("person");
            var content = Substitute.For<IPublishedContent>();
            content.ContentType.Alias.Returns("person");

            var graphType = CreateSUT(contentType);

            graphType.IsTypeOf(content).Should().BeTrue();
        }

        [Fact]
        public void IsOfType_WhenTypeDoesNotMatch_ReturnsFalse()
        {
            var content = Substitute.For<IPublishedContent>();
            content.ContentType.Alias.Returns("person");

            var graphType = CreateSUT();

            graphType.IsTypeOf(content).Should().BeFalse();
        }

        [Fact]
        public void Interfaces_ContainsPublishedContentInterfaceGraphType()
        {
            var graphType = CreateSUT();

            graphType.Interfaces.Should().Contain(typeof(PublishedContentInterfaceGraphType));
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

        [Fact]
        public void ContentTypeFieldResolver_WhenCalled_ReturnsAlias()
        {
            var graphType = CreateSUT();

            var contentType = Substitute.For<IPublishedContentType>();
            var content = Substitute.For<IPublishedContent>();
            content.ContentType.Returns(contentType);

            graphType.Fields.Should().Contain(x => x.Name == "_contentType")
                .Which.Resolver.Resolve(new ResolveFieldContext{ Source = content })
                .Should().Be(contentType);
        }

        [Fact]
        public void IdFieldResolver_WhenCalled_ReturnsId()
        {
            var graphType = CreateSUT();

            var content = Substitute.For<IPublishedContent>();
            content.Key.Returns(new Guid("F14EA3D9-E40A-4A14-B860-492125D6877B"));

            graphType.Fields.Should().Contain(x => x.Name == "_id")
                .Which.Resolver.Resolve(new ResolveFieldContext{ Source = content })
                .Should().Be(new Id(content?.Key.ToString()));
        }
    }

    internal static class PropertyTypeExtensions
    {
        /// <summary>
        /// Set the property type alias without requiring `Current`.
        /// </summary>
        /// <param name="propertyType"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static PropertyType SetAlias(this PropertyType propertyType, string alias)
        {
            typeof(PropertyType).GetField("_alias", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(propertyType, alias);
            return propertyType;
        }
    }
}
