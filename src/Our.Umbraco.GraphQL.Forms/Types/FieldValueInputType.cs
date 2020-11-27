using GraphQL.Types;

namespace Our.Umbraco.GraphQL.Forms.Types
{
    public class FieldValue
    {
        public string Field { get; set; }
        public string Value { get; set; }
    }

    public class FieldValueInputType : InputObjectGraphType<FieldValue>
    {
        public FieldValueInputType()
        {
            Name = "FieldDataInput";
            Field(x => x.Field);
            Field(x => x.Value);
        }
    }
}
