using GraphQL;
using GraphQL.Http;
using GraphQL.Instrumentation;
using Microsoft.Owin;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GraphQL.DataLoader;
using GraphQL.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Our.Umbraco.GraphQL.Builders;
using Our.Umbraco.GraphQL.Instrumentation;
using Our.Umbraco.GraphQL.Types;
using Our.Umbraco.GraphQL.Composing;

namespace Our.Umbraco.GraphQL.Web.Middleware
{
    internal class GraphQLMiddleware
    {
        private readonly IDocumentWriter _documentWriter;
        private readonly DataLoaderDocumentListener _dataLoaderDocumentListener;
        private readonly GraphQLRequestParser _requestParser;
        private readonly ISchemaBuilder _schemaBuilder;
        private readonly GraphQLServerOptions _options;
        private readonly FieldMiddlewareCollection _fieldMiddlewareCollection;

        public GraphQLMiddleware(ISchemaBuilder schemaBuilder, IDocumentWriter documentWriter,
            DataLoaderDocumentListener dataLoaderDocumentListener, FieldMiddlewareCollection fieldMiddlewareCollection,
            GraphQLRequestParser requestParser, GraphQLServerOptions options)
        {
            _schemaBuilder = schemaBuilder ?? throw new ArgumentNullException(nameof(schemaBuilder));
            _documentWriter = documentWriter ?? throw new ArgumentNullException(nameof(documentWriter));
            _dataLoaderDocumentListener = dataLoaderDocumentListener ??
                                          throw new ArgumentNullException(nameof(dataLoaderDocumentListener));
            _fieldMiddlewareCollection = fieldMiddlewareCollection ??
                                         throw new ArgumentNullException(nameof(fieldMiddlewareCollection));
            _requestParser = requestParser ?? throw new ArgumentNullException(nameof(requestParser));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task Invoke(IOwinContext context, Func<Task> next)
        {
            try
            {
                // TODO: Add ISchemaCacher
                using (var schema = _schemaBuilder.Build(typeof(Schema<Query, Mutation>).GetTypeInfo()))
                {
                    var request = await _requestParser.Parse(context.Request);
                    switch (context.Request.Method)
                    {
                        case "POST":
                            if (request == null)
                            {
                                context.Response.StatusCode = 400;
                                await context.Response.WriteAsync("POST body missing.");
                                return;
                            }
                            break;
                        default:
                            context.Response.StatusCode = 405;
                            await context.Response.WriteAsync("Server supports only POST requests.");
                            return;
                    }

                    var requests = request.Select(async requestParams =>
                    {
                        var query = requestParams.Query;
                        var operationName = requestParams.OperationName;
                        var variables = requestParams.Variables;

                        var start = DateTime.Now;
                        var miniProfiler = MiniProfiler.StartNew();
                        var result = await new DocumentExecuter()
                            .ExecuteAsync(opts =>
                            {
                                opts.CancellationToken = context.Request.CallCancelled;
                                opts.ComplexityConfiguration = _options.ComplexityConfiguration;
                                opts.EnableMetrics = _options.EnableMetrics;

                                foreach(var fieldMiddleware in _fieldMiddlewareCollection)
                                {
                                    opts.FieldMiddleware.Use(nextMiddleware => fieldContext => fieldMiddleware.Resolve(fieldContext, nextMiddleware));
                                }

                                opts.FieldMiddleware.Use<InstrumentFieldsMiddleware>();
                                if (_options.EnableMiniProfiler)
                                    opts.FieldMiddleware.Use<MiniProfilerFieldMiddleware>();
                                opts.ExposeExceptions = _options.Debug;
                                opts.Inputs = variables;
                                opts.Listeners.Add(_dataLoaderDocumentListener);
                                opts.OperationName = operationName;
                                opts.Query = query;
                                opts.Schema = schema;
                                opts.ValidationRules = DocumentValidator.CoreRules();
                            });

                        if (result.Errors == null)
                        {
                            result.EnrichWithApolloTracing(start);
                            if (_options.EnableMiniProfiler)
                            {
                                if (result.Extensions == null)
                                    result.Extensions = new Dictionary<string, object>();
                                result.Extensions["miniProfiler"] = JObject.FromObject(MiniProfiler.Current, new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                            }
                        }
                        miniProfiler.Stop();

                        return result;
                    });

                    var responses = await Task.WhenAll(requests);

                    context.Response.ContentType = "application/json";

                    if (false == request.IsBatched)
                    {
                        await _documentWriter.WriteAsync(context.Response.Body, responses[0]);
                    }
                    else
                    {
                        await _documentWriter.WriteAsync(context.Response.Body, responses);
                    }
                }
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = 500;

                if (_options.Debug)
                {
                    await context.Response.WriteAsync(ex.ToString());
                }
                else
                {
                    await context.Response.WriteAsync("Internal server error");
                }
            }
        }
    }
}
