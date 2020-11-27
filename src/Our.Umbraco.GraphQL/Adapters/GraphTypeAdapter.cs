using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Our.Umbraco.GraphQL.Adapters.Resolvers;
using Our.Umbraco.GraphQL.Adapters.Types;
using Our.Umbraco.GraphQL.Adapters.Types.Relay;
using Our.Umbraco.GraphQL.Adapters.Types.Resolution;
using Our.Umbraco.GraphQL.Adapters.Visitors;
using Our.Umbraco.GraphQL.Attributes;
using Our.Umbraco.GraphQL.Reflection;
using Our.Umbraco.GraphQL.Types;
using Our.Umbraco.GraphQL.Types.Relay;

namespace Our.Umbraco.GraphQL.Adapters
{
    public class GraphTypeAdapter : IGraphTypeAdapter
    {
        private readonly Dictionary<TypeInfo, IGraphType> _cache;
        private readonly ITypeRegistry _typeRegistry;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IGraphVisitor _visitor;

        public GraphTypeAdapter(ITypeRegistry typeRegistry, IDependencyResolver dependencyResolver,
            IGraphVisitor visitor)
        {
            _typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
            _dependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
            _visitor = visitor;
            _cache = new Dictionary<TypeInfo, IGraphType>();
        }

        public IGraphType Adapt<T>()
        {
            return Adapt(typeof(T).GetTypeInfo());
        }

        public IGraphType Adapt(TypeInfo typeInfo)
        {
            if (typeInfo == null) throw new ArgumentNullException(nameof(typeInfo));

            var unwrappedTypeInfo = typeInfo.Unwrap();
            var graphType = TryGetFromCache(unwrappedTypeInfo, typeInfo);
            if (graphType != null) return graphType;

            if (unwrappedTypeInfo.IsEnum)
            {
                graphType = CreateGraphType(unwrappedTypeInfo,
                    typeof(EnumerationGraphType<>).MakeGenericType(unwrappedTypeInfo));
            }
            else if (unwrappedTypeInfo.IsInterface || unwrappedTypeInfo.IsAbstract)
            {
                graphType = CreateGraphType(unwrappedTypeInfo,
                    typeof(InterfaceGraphType<>).MakeGenericType(unwrappedTypeInfo));
                _visitor?.Visit((IInterfaceGraphType) graphType);
            }
            else
            {
                try
                {
                    graphType = CreateGraphType(unwrappedTypeInfo,
                        typeof(ObjectGraphType<>).MakeGenericType(unwrappedTypeInfo));
                    _visitor?.Visit((IObjectGraphType)graphType);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return Wrap(typeInfo, graphType);
        }

        private IGraphType AdaptInput(TypeInfo typeInfo)
        {
            var unwrappedTypeInfo = typeInfo.Unwrap();
            var graphType = TryGetFromCache(unwrappedTypeInfo, typeInfo);
            if (graphType != null) return graphType;
            if (unwrappedTypeInfo.IsEnum)
            {
                graphType = CreateGraphType(unwrappedTypeInfo,
                    typeof(EnumerationGraphType<>).MakeGenericType(unwrappedTypeInfo));
            }
            else
            {
                graphType = CreateGraphType(unwrappedTypeInfo,
                    typeof(InputObjectGraphType<>).MakeGenericType(unwrappedTypeInfo));
                _visitor?.Visit((IInputObjectGraphType) graphType);
            }

            return graphType;
        }

        private void AddFields(TypeInfo typeInfo, IComplexGraphType graphType)
        {
            do
            {
                var fields = typeInfo.DeclaredFields.Where(x =>
                    x.IsPublic && x.IsStatic == false && IsValidReturnType(x.FieldType));
                var methods = typeInfo.DeclaredMethods.Where(x =>
                    x.IsPublic && x.IsStatic == false && x.IsSpecialName == false && IsValidReturnType(x.ReturnType) &&
                    x.GetBaseDefinition()?.DeclaringType != typeof(object));
                var properties = typeInfo.DeclaredProperties.Where(x =>
                    x.CanRead && x.GetMethod.IsPublic && x.GetMethod.IsStatic == false &&
                    IsValidReturnType(x.GetMethod.ReturnType));

                var members = fields.Cast<MemberInfo>().Concat(methods).Concat(properties)
                    .Where(x => x.GetCustomAttribute<IgnoreAttribute>() == null)
                    .OrderBy(x => x.Name);

                foreach (var memberInfo in members)
                {
                    graphType.AddField(CreateField(memberInfo));
                }
            } while (typeInfo.BaseType != null && (typeInfo = typeInfo.BaseType.GetTypeInfo()) != typeof(object));
        }

        private FieldType CreateField(MemberInfo memberInfo)
        {
            var returnType = memberInfo.GetReturnType();

            var unwrappedReturnType = returnType.Unwrap();

            var foundType = _typeRegistry.Get(unwrappedReturnType);

            IGraphType resolvedType = null;
            if (foundType == null)
            {
                if (unwrappedReturnType.IsGenericType &&
                    unwrappedReturnType.GetGenericTypeDefinition() == typeof(Connection<>))
                {
                    foundType = _typeRegistry.Get(unwrappedReturnType.GenericTypeArguments[0].GetTypeInfo());
                    if (foundType != null)
                    {
                        foundType = typeof(ConnectionGraphType<>).MakeGenericType(foundType).GetTypeInfo();
                    }
                    else
                    {
                        resolvedType =
                            new ConnectionGraphType(Adapt(unwrappedReturnType.GenericTypeArguments[0].GetTypeInfo()));
                        _visitor.Visit((IObjectGraphType) resolvedType);
                    }
                }
                else if (unwrappedReturnType.IsGenericType &&
                         unwrappedReturnType.GetGenericTypeDefinition() == typeof(Edge<>))
                {
                    foundType = _typeRegistry.Get(unwrappedReturnType.GenericTypeArguments[0].GetTypeInfo());
                    if (foundType != null)
                    {
                        foundType = typeof(EdgeGraphType<>).MakeGenericType(foundType).GetTypeInfo();
                    }
                    else
                    {
                        resolvedType =
                            new EdgeGraphType(Adapt(unwrappedReturnType.GenericTypeArguments[0].GetTypeInfo()));
                        _visitor.Visit((IObjectGraphType) resolvedType);
                    }
                }
                else
                {
                    resolvedType = Adapt(returnType);
                }
            }

            var isNonNullItem = memberInfo.GetCustomAttribute<NonNullItemAttribute>() != null;
            if (isNonNullItem && resolvedType != null &&
                resolvedType is ListGraphType listGraphType)
            {
                resolvedType = WrapList(WrapNonNull(listGraphType.ResolvedType));
            }

            var isNonNull = memberInfo.GetCustomAttribute<NonNullAttribute>() != null;
            if (isNonNull && resolvedType != null)
            {
                resolvedType = WrapNonNull(resolvedType);
            }

            var fieldType = new FieldType
            {
                Arguments = CreateArguments(memberInfo),
                DefaultValue = memberInfo.GetCustomAttribute<DefaultValueAttribute>()?.DefaultValue,
                Description = memberInfo.GetCustomAttribute<DescriptionAttribute>()?.Description,
                DeprecationReason = memberInfo.GetCustomAttribute<DeprecatedAttribute>()?.DeprecationReason,
                Metadata = {{nameof(MemberInfo), memberInfo}},
                Name = memberInfo.GetCustomAttribute<NameAttribute>()?.Name ?? memberInfo.Name,
                ResolvedType = resolvedType,
                Type = foundType.Wrap(returnType, isNonNull, isNonNullItem)
            };

            fieldType.Resolver = new FieldResolver(fieldType, _dependencyResolver);

            return fieldType;
        }

        private QueryArguments CreateArguments(MemberInfo memberInfo)
        {
            if (memberInfo is MethodInfo methodInfo)
            {
                return new QueryArguments(methodInfo.GetParameters()
                    .Where(x => x.GetCustomAttribute<InjectAttribute>() == null
                                && typeof(CancellationToken).IsAssignableFrom(x.ParameterType) == false)
                    .Select(CreateArgument));
            }

            return null;
        }

        private QueryArgument CreateArgument(ParameterInfo parameterInfo)
        {
            var parameterType = parameterInfo.ParameterType;
            var hasDefaultValue = parameterInfo.HasDefaultValue ||
                                  parameterInfo.GetCustomAttribute<DefaultValueAttribute>() != null ||
                                  parameterType.IsValueType && parameterType.IsNullable();

            var unwrappedType = parameterType.GetTypeInfo().Unwrap();

            IGraphType inputType;
            if (unwrappedType == typeof(OrderBy))
            {
                var returnType = parameterInfo.Member.GetReturnType().Unwrap();

                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Connection<>))
                    returnType = returnType.GenericTypeArguments[0].GetTypeInfo();

                var foundType = _typeRegistry.Get(returnType);

                var graphType = foundType == null
                    ? Adapt(returnType)
                    : Activator.CreateInstance(foundType);

                inputType = new OrderByGraphType((IComplexGraphType) graphType);
                _visitor.Visit((EnumerationGraphType) inputType);

                if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    inputType = WrapList(WrapNonNull(inputType));
                }
            }
            else
            {
                inputType = AdaptInput(parameterType.GetTypeInfo());
            }

            if (hasDefaultValue == false)
            {
                inputType = WrapNonNull(inputType);
            }

            if (hasDefaultValue && inputType is NonNullGraphType nonNullGraphType)
            {
                inputType = nonNullGraphType.ResolvedType;
            }

            return new QueryArgument(inputType)
            {
                DefaultValue = parameterInfo.HasDefaultValue
                    ? parameterInfo.DefaultValue
                    : parameterInfo.GetCustomAttribute<DefaultValueAttribute>()?.DefaultValue,
                Description = parameterInfo.GetCustomAttribute<DescriptionAttribute>()?.Description,
                Name = parameterInfo.GetCustomAttribute<NameAttribute>()?.Name ?? parameterInfo.Name,
            };
        }

        private IGraphType CreateGraphType(TypeInfo typeInfo, Type graphTypeType)
        {
            if (typeof(IGraphType).IsAssignableFrom(typeInfo)) return CreateGraphType(typeInfo);

            var graphType = (IGraphType) Activator.CreateInstance(graphTypeType);
            graphType.Description = typeInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
            graphType.DeprecationReason = typeInfo.GetCustomAttribute<DeprecatedAttribute>()?.DeprecationReason;
            graphType.Metadata[nameof(TypeInfo)] = typeInfo;

            var nameAttribute = typeInfo.GetCustomAttribute<NameAttribute>();
            if (nameAttribute != null)
            {
                graphType.Name = nameAttribute.Name;
            }
            else if (typeInfo.IsGenericType)
            {
                var genericTypeNames =
                    typeInfo.GenericTypeArguments.Select(x => x.GetCustomAttribute<NameAttribute>()?.Name ?? x.Name);

                graphType.Name = string.Join(string.Empty, genericTypeNames) +
                                 (typeInfo.GetCustomAttribute<NameAttribute>()?.Name ??
                                  typeInfo.Name.Substring(0, typeInfo.Name.IndexOf('`')));
            }
            else
            {
                graphType.Name = typeInfo.Name;
            }

            _cache.Add(typeInfo, graphType);

            if (graphType is IObjectGraphType objectGraphType)
            {
                foreach (var @interface in typeInfo.ImplementedInterfaces.Where(x =>
                    x.Namespace?.StartsWith("System") == false))
                {
                    objectGraphType.AddResolvedInterface((IInterfaceGraphType) Adapt(@interface.GetTypeInfo()));
                }
            }

            switch (graphType)
            {
                case IComplexGraphType complexGraphType:
                    AddFields(typeInfo, complexGraphType);
                    foreach (var extendingTypes in _typeRegistry.GetExtending(typeInfo))
                    {
                        AddFields(extendingTypes, complexGraphType);
                    }

                    break;
                case EnumerationGraphType enumerationGraphType:
                {
                    foreach (var value in enumerationGraphType.Values)
                    {
                        var enumMember = typeInfo.GetDeclaredField(value.Value.ToString());
                        value.Description = enumMember.GetCustomAttribute<DescriptionAttribute>()?.Description;
                        value.DeprecationReason =
                            enumMember.GetCustomAttribute<DeprecatedAttribute>()?.DeprecationReason;
                        value.Name = enumMember.GetCustomAttribute<NameAttribute>()?.Name ?? value.Name;
                    }

                    break;
                }
            }

            return graphType;
        }

        private IGraphType CreateGraphType(TypeInfo typeInfo)
        {
            var constructor = typeInfo.GetConstructors().OrderBy(c => c.GetParameters()?.Length ?? 0).First();
            var parms = constructor.GetParameters();
            var args = new object[parms?.Length ?? 0];

            for (var i = 0; i < args.Length; i++)
            {
                args[i] = _dependencyResolver.Resolve(parms[i].ParameterType);
            }

            var graphType = constructor.Invoke(args) as IGraphType;

            _cache.Add(typeInfo, graphType);

            return graphType;
        }

        private IGraphType TryGetFromCache(TypeInfo typeInfo, TypeInfo unwrappedTypeInfo)
        {
            if (_cache.TryGetValue(typeInfo, out var foundGraphType))
            {
                return Wrap(unwrappedTypeInfo, foundGraphType);
            }

            var foundType = _typeRegistry.Get(typeInfo);
            if (foundType == null) return null;
            return Wrap(unwrappedTypeInfo, (IGraphType) Activator.CreateInstance(foundType));
        }

        private IGraphType Wrap(TypeInfo typeInfo, IGraphType graphType)
        {
            var enumerableArgument = typeInfo.GetEnumerableArgument();

            if (typeInfo.IsValueType && typeInfo.IsNullable() == false || enumerableArgument != null &&
                enumerableArgument.IsValueType && enumerableArgument.IsNullable() == false)
                graphType = WrapNonNull(graphType);

            if (enumerableArgument != null) graphType = WrapList(graphType);

            return graphType;
        }

        private static ListGraphType WrapList(IGraphType graphType)
        {
            var listGraphType =
                (ListGraphType) Activator.CreateInstance(
                    typeof(ListGraphType<>).MakeGenericType(graphType.GetType()));
            listGraphType.ResolvedType = graphType;
            return listGraphType;
        }

        private static IGraphType WrapNonNull(IGraphType graphType)
        {
            if (graphType is NonNullGraphType)
                return graphType;

            var nonNullGraphType =
                (NonNullGraphType) Activator.CreateInstance(
                    typeof(NonNullGraphType<>).MakeGenericType(graphType.GetType()));
            nonNullGraphType.ResolvedType = graphType;
            return nonNullGraphType;
        }

        private bool IsValidReturnType(Type type) =>
            type != typeof(void) && type != typeof(object) && type != typeof(Task);
    }
}
