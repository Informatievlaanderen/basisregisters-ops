namespace Ops.Web.Jobs
{
    using System;
    using Grb;

    public class Job
    {
        public Guid Id { get; set; }
        public Uri? TicketUrl { get; set; }
        public JobStatus Status { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset LastChanged { get; set; }

        public Job(Guid id, Uri? ticketUrl, JobStatus status, DateTimeOffset created, DateTimeOffset lastChanged)
        {
            Id = id;
            TicketUrl = ticketUrl;
            Status = status;
            Created = created;
            LastChanged = lastChanged;
        }

        public bool CanCancel()
        {
            return Status is JobStatus.Created or JobStatus.Error;
        }

        public bool CanHaveRecords()
        {
            return Status != JobStatus.Created && Status != JobStatus.Cancelled;
        }

        public bool HasTicket()
        {
            return TicketUrl is not null;
        }
    }
}
