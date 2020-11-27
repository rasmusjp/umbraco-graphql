using Our.Umbraco.GraphQL.Attributes;
using Our.Umbraco.GraphQL.Types;
using System;
using System.Collections.Generic;
using Umbraco.Forms.Core;
using Umbraco.Forms.Data.Storage;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    public class DataSourcesDataQuery
    {
        public IEnumerable<FormDataSource> All([Inject] IDataSourceStorage dataSourceStorage) => dataSourceStorage.GetAllDataSources();

        public FormDataSource ById([Inject] IDataSourceStorage dataSourceStorage, Id id) => Guid.TryParse(id.Value, out var guid) ? dataSourceStorage.GetDataSource(guid) : null;
    }
}
