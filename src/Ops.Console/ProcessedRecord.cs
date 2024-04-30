namespace Ops.Console
{
    using CsvHelper.Configuration;

    public sealed class ProcessedRecord
    {
        public string Id { get; init; }
        public string TicketUrl { get; init; }

        protected ProcessedRecord()
        {
        }

        public ProcessedRecord(string id, string ticketUrl)
        {
            Id = id;
            TicketUrl = ticketUrl;
        }
    }

    public sealed class ProcessedRecordMap : ClassMap<ProcessedRecord>
    {
        public ProcessedRecordMap()
        {
            Map(m => m.Id).Name("Id");
            Map(m => m.TicketUrl).Name("Ticket");
        }
    }
}
