namespace Ops.Web.Jobs
{
    public class JobRecord
    {
        public Guid Id { get; set; }
        public int GrId { get; set; }
        public int RecordNumber { get; set; }
        public JobRecordStatus Status { get; set; }
        public Guid? TicketId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTimeOffset VersionDate { get; set; }
    }
}
