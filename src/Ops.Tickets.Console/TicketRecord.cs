namespace Ops.Tickets.Console
{
    using CsvHelper.Configuration;

    public sealed class TicketRecord
    {
        public string Id { get; init; }
        public string TicketUrl { get; init; }

        protected TicketRecord()
        {
        }

        public TicketRecord(string id, string ticketUrl)
        {
            Id = id;
            TicketUrl = ticketUrl;
        }
    }

    public sealed class TicketRecordMap : ClassMap<TicketRecord>
    {
        public TicketRecordMap()
        {
            Map(m => m.Id).Name("Id");
            Map(m => m.TicketUrl).Name("Ticket");
        }
    }

    public sealed class TicketErrorRecord
    {
        public string Id { get; init; }
        public string TicketUrl { get; init; }
        public string Error { get; init; }

        protected TicketErrorRecord()
        {
        }

        public TicketErrorRecord(string id, string ticketUrl, string error)
        {
            Id = id;
            TicketUrl = ticketUrl;
            Error = error;
        }
    }

    public sealed class TicketErrorRecordMap : ClassMap<TicketErrorRecord>
    {
        public TicketErrorRecordMap()
        {
            Map(m => m.Id).Name("Id");
            Map(m => m.TicketUrl).Name("Ticket");
            Map(m => m.Error).Name("Error");
        }
    }

    public sealed class TicketResponse
    {
        public string Status { get; set; }
        public TicketResult Result { get; set; }
    }

    public sealed class TicketResult
    {
        public string Json { get; set; }
    }
}
