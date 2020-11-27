using GraphQL;
using GraphQL.Types;
using Our.Umbraco.GraphQL.Adapters.Types.Resolution;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Our.Umbraco.GraphQL.Adapters.PublishedContent.Types
{
    public class PublishedElementGraphType : ObjectGraphType<IPublishedElement>
    {
        public PublishedElementGraphType(IContentTypeComposition contentType,
            IPublishedContentType publishedContentType, ITypeRegistry typeRegistry)
        {
            Name = $"{contentType.Alias.ToPascalCase()}PublishedElement";
            IsTypeOf = x => ((IPublishedElement) x).ContentType.Alias == contentType.Alias;

            Interface<PublishedElementInterfaceGraphType>();

            this.AddBuiltInFields();

            foreach (var propertyType in contentType.CompositionPropertyTypes)
                base.AddField(new PublishedPropertyFieldType(publishedContentType, propertyType, typeRegistry, null, null));
        }
    }
}
