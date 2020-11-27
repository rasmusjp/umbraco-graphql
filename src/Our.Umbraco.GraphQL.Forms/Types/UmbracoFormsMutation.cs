using GraphQL.Types;
using Newtonsoft.Json.Linq;
using Our.Umbraco.GraphQL.Adapters.Types;
using Our.Umbraco.GraphQL.Attributes;
using Our.Umbraco.GraphQL.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Forms.Core.Data.Storage;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Extensions;
using Umbraco.Forms.Core.Models;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    public class UmbracoFormsMutation : ObjectGraphType
    {
        private readonly ILogger _logger;
        private readonly IFormStorage _formStorage;
        private readonly IFieldTypeStorage _fieldTypeStorage;
        private readonly IRecordService _recordService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IPublishedRouter _publishedRouter;

        public UmbracoFormsMutation(ILogger logger, IFormStorage formStorage, IFieldTypeStorage fieldTypeStorage, IRecordService recordService, IUmbracoContextFactory umbracoContextFactory, IPublishedRouter publishedRouter)
        {
            Name = nameof(UmbracoFormsMutation);

            Field<JsonGraphType>()
                .Name("submit")
                .Argument<NonNullGraphType<Adapters.Types.IdGraphType>>("formId", "The GUID of the form")
                .Argument<NonNullGraphType<Adapters.Types.IdGraphType>>("umbracoPageId", "The integer ID of the Umbraco page you were on")
                .Argument<ListGraphType<FieldValueInputType>>("fields", "An array of objects representing the field data.  Each object has a 'field' property that is either the GUID or alias of the form field, and a 'value' property that is the field value")
                .Resolve(Submit);
            _logger = logger;
            _formStorage = formStorage;
            _fieldTypeStorage = fieldTypeStorage;
            _recordService = recordService;
            _umbracoContextFactory = umbracoContextFactory;
            _publishedRouter = publishedRouter;
        }

        private object Submit(ResolveFieldContext<object> ctx)
        {
            string formIdArg = null;
            string umbracoPageId = null;

            try
            {
                formIdArg = ctx.GetArgument<Id>("formId").Value;
                umbracoPageId = ctx.GetArgument<Id>("umbracoPageId").Value;
                var fieldsList = ctx.GetArgument<List<FieldValue>>("fields");

                if (!Guid.TryParse(formIdArg, out var formId) || !(_formStorage.GetForm(formId) is Form form)) return SubmitResult("The form ID specified could not be found");
                if (fieldsList == null || fieldsList.Count == 0) return SubmitResult("You must specify one or more field values");

                var fields = new Dictionary<string, string>(fieldsList.Count);
                fieldsList.ForEach(f => fields[f.Field] = f.Value);

                var context = HttpContext.Current;
                var contextWrapped = new HttpContextWrapper(context);
                var errors = ValidateFormState(fields, form, contextWrapped)?.ToList();
                if (errors != null && errors.Count > 0) return SubmitResult(errors);

                using (var ucRef = _umbracoContextFactory.EnsureUmbracoContext(contextWrapped))
                {
                    var uc = ucRef.UmbracoContext;
                    var page = Guid.TryParse(umbracoPageId, out var guid) ? uc.Content.GetById(guid)
                        : (int.TryParse(umbracoPageId, out var id) ? uc.Content.GetById(id)
                        : (GuidUdi.TryParse(umbracoPageId, out var udi) ? uc.Content.GetById(udi.Guid) : null));
                    if (page == null) return SubmitResult("Could not find the umbracoPageId specified");

                    var url = page.Url;
                    if (string.IsNullOrEmpty(url)) return SubmitResult("The page specified does not have a routable URL to associate with the form request");

                    var request = _publishedRouter.CreateRequest(uc, new Uri(context.Request.Url, url));
                    uc.PublishedRequest = request;
                    _publishedRouter.PrepareRequest(request);

                    var recordId = SubmitForm(context, contextWrapped, formId, form, page.Id, fields);
                    return SubmitResult(recordId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error<UmbracoFormsMutation>(ex, "Could not submit form {formId} for Umbraco page {umbracoPageId}", formIdArg, umbracoPageId);
                return SubmitResult("An unspecified error occurred.  Check the Umbraco logs for more details.");
            }
        }

        private Guid SubmitForm(HttpContext context, HttpContextBase contextWrapped, Guid formId, Form form, int umbracoPageId, Dictionary<string, string> fields)
        {
            var record = new Record
            {
                Form = formId,
                State = FormState.Submitted,
                UmbracoPageId = umbracoPageId,
                IP = context.Request.UserHostAddress
            };

            if (context.User != null && context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var username = context.User.Identity.Name;
                var user = string.IsNullOrWhiteSpace(username) ? null : Membership.GetUser(username);

                if (user != null) record.MemberKey = user.ProviderUserKey != null ? user.ProviderUserKey.ToString() : null;
            }

            foreach (var allField in form.AllFields)
            {
                var inputValues = new object[0];
                if (fields.TryGetValue(allField.Id.ToString(), out var field) || fields.TryGetValue(allField.Alias, out field)) inputValues = new[] { field };

                var fieldValues = _fieldTypeStorage.GetFieldTypeByField(allField).ConvertToRecord(allField, inputValues, contextWrapped).ToArray();

                if (record.RecordFields.TryGetValue(allField.Id, out var recordField))
                {
                    recordField.Values.Clear();
                    recordField.Values.AddRange(fieldValues);
                }
                else
                {
                    recordField = new RecordField(allField);
                    recordField.Values.AddRange(fieldValues);
                    record.RecordFields.Add(allField.Id, recordField);
                }
            }

            _recordService.Submit(record, form);

            return record.UniqueId;
        }

        private JToken SubmitResult(string error) => new JObject
        {
            ["success"] = false,
            ["errors"] = new JArray(new[] { new JObject
            {
                ["error"] = error
            } })
        };

        private JToken SubmitResult(IEnumerable<FieldValue> errors) => new JObject
            {
                ["success"] = false,
                ["errors"] = new JArray(errors.Select(e => new JObject
                {
                    ["field"] = e.Field,
                    ["error"] = e.Value
                }).ToArray())
            };

        private JToken SubmitResult(Guid recordId) => new JObject
            {
                ["success"] = true,
                ["id"] = recordId.ToString()
            };

        private IEnumerable<FieldValue> ValidateFormState(Dictionary<string, string> fields, Form form, HttpContextBase context)
        {
            var allFields = form.AllFields.ToDictionary(f => f.Id, f => string.Join(", ", f.Values ?? new List<object>()));
            foreach (var formField in form.AllFields)
            {
                var inputValues = new object[0];
                if (fields.TryGetValue(formField.Id.ToString(), out var field) || fields.TryGetValue(formField.Alias, out field)) inputValues = new[] { field };

                var type = _fieldTypeStorage.GetFieldTypeByField(formField);
                var errors = type.ValidateField(form, formField, inputValues, context, _formStorage);

                foreach (string error in errors)
                {
                    var message = error;
                    if (string.IsNullOrWhiteSpace(message)) message = string.Format((form.InvalidErrorMessage ?? "").ParsePlaceHolders(), formField.Caption);
                    yield return new FieldValue { Field = formField.Alias, Value = message };
                }
            }
        }
    }

    public class ExtendMutationWithUmbracoFormsMutation
    {
        [NonNull]
        [Description("Mutation to submit an Umbraco Form")]
        public UmbracoFormsMutation UmbracoForms([Inject] UmbracoFormsMutation mutation) => mutation;
    }
}
