namespace Ops.Console
{
    using CsvHelper.Configuration;

    public sealed class InputRecord
    {
        public string Id { get; init; }

        protected InputRecord()
        { }
    }

    public sealed class InputRecordMap : ClassMap<InputRecord>
    {
        public InputRecordMap()
        {
            Map(m => m.Id).Name("Id");
        }
    }
}
