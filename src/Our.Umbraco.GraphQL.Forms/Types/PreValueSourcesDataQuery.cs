using Our.Umbraco.GraphQL.Attributes;
using Our.Umbraco.GraphQL.Types;
using System;
using System.Collections.Generic;
using Umbraco.Forms.Core.Interfaces;
using Umbraco.Forms.Data.Storage;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    public class PreValueSourcesDataQuery
    {
        public IEnumerable<IFieldPreValueSource> All([Inject] IPrevalueSourceStorage prevalueSourceStorage) => prevalueSourceStorage.GetAllPrevalueSources();

        public IFieldPreValueSource ById([Inject] IPrevalueSourceStorage prevalueSourceStorage, Id id) => Guid.TryParse(id.Value, out var guid) ? prevalueSourceStorage.GetPrevalueSource(guid) : null;
    }
}
