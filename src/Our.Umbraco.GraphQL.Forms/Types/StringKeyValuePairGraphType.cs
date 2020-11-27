using GraphQL;
using GraphQL.Types;
using System.Collections.Generic;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    [GraphQLMetadata("StringKeyValuePair")]
    public class StringKeyValuePairGraphType : ObjectGraphType<KeyValuePair<string, string>>
    {
        public StringKeyValuePairGraphType()
        {
            Name = "StringKeyValuePair";

            this.AddBuiltinFields();
        }
    }
}
