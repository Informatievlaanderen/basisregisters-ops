namespace Ops.Web.Jobs
{
    using Grb;

    // We're only mapping job records, because we want to manipulate its state.
    public class JobRecord
    {
        public long JobRecordId { get; }
        public Guid JobId { get; }
        public int RecordNumber { get; }
        public int GrId { get; }
        public Uri? TicketUrl { get; }
        public JobRecordStatus Status { get; set; }
        public string? ErrorMessage { get; }
        public DateTimeOffset VersionDate { get; }

        public JobRecord(
            long jobRecordId,
            Guid jobId,
            int recordNumber,
            int grId,
            Uri? ticketUrl,
            JobRecordStatus status,
            string? errorMessage,
            DateTimeOffset versionDate)
        {
            JobRecordId = jobRecordId;
            JobId = jobId;
            RecordNumber = recordNumber;
            GrId = grId;
            TicketUrl = ticketUrl;
            Status = status;
            ErrorMessage = errorMessage;
            VersionDate = versionDate;
        }
    }
}
