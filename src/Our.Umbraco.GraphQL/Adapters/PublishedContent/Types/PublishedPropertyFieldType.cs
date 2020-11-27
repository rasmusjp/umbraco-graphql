using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using Our.Umbraco.GraphQL.Adapters.Types.Resolution;
using Our.Umbraco.GraphQL.Reflection;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Our.Umbraco.GraphQL.Adapters.PublishedContent.Types
{
    public class PublishedPropertyFieldType : FieldType
    {
        public PublishedPropertyFieldType(IPublishedContentType contentType, PropertyType propertyType,
            ITypeRegistry typeRegistry, IUmbracoContextFactory umbracoContextFactory, IPublishedRouter publishedRouter)
        {
            var publishedPropertyType = contentType.GetPropertyType(propertyType.Alias);

            var type = publishedPropertyType.ClrType.GetTypeInfo();
            var unwrappedTypeInfo = type.Unwrap();

            if (typeof(IPublishedContent).IsAssignableFrom(unwrappedTypeInfo))
                unwrappedTypeInfo = typeof(IPublishedContent).GetTypeInfo();
            else if (typeof(IPublishedElement).IsAssignableFrom(unwrappedTypeInfo))
                unwrappedTypeInfo = typeof(IPublishedElement).GetTypeInfo();

            var propertyGraphType = typeRegistry.Get(unwrappedTypeInfo) ?? typeof(StringGraphType).GetTypeInfo();

            propertyGraphType = propertyGraphType.Wrap(type, propertyType.Mandatory, false);

            if (propertyType.VariesByCulture())
            {
                Arguments = new QueryArguments(new QueryArgument(typeof(StringGraphType))
                {
                    Name = "culture"
                });
            }

            Type = propertyGraphType;
            Name = publishedPropertyType.Alias.ToCamelCase();
            Description = propertyType.Description;
            Resolver = new FuncFieldResolver<IPublishedElement, object>(context =>
            {
                object ResolveProperty() => context.Source.Value(propertyType.Alias, context.GetArgument<string>("culture"), fallback: Fallback.ToLanguage);
                if (umbracoContextFactory == null || publishedRouter == null) return ResolveProperty();

                var pc = context.Source as IPublishedContent;
                var url = pc?.Url;
                if (string.IsNullOrEmpty(url)) return ResolveProperty();

                using (var ucRef = umbracoContextFactory.EnsureUmbracoContext())
                {
                    var uc = ucRef.UmbracoContext;

                    if (uc.PublishedRequest?.PublishedContent?.Id != pc.Id)
                    {
                        var request = publishedRouter.CreateRequest(uc, new Uri(HttpContext.Current.Request.Url, url));
                        uc.PublishedRequest = request;
                        publishedRouter.PrepareRequest(request);
                    }

                    return ResolveProperty();
                }
            });
        }
    }
}
