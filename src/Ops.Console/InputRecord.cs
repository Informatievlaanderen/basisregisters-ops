namespace Ops.Console
{
    using CsvHelper.Configuration;

    public sealed class InputRecord
    {
        public string Id { get; init; }
        public string Url {get; init; }
        public string? Body { get; init; }

        protected InputRecord()
        { }
    }

    public sealed class InputRecordMap : ClassMap<InputRecord>
    {
        public InputRecordMap()
        {
            Map(m => m.Id).Name("Id");
            Map(m => m.Url).Name("Url");
            Map(m => m.Body).Name("Body");
        }
    }
}
