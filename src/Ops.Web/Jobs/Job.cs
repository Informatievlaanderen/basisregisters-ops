namespace Ops.Web.Jobs;

public class Job
{
    public Guid JobId { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset LastChanged { get; set; }
    public JobStatus Status { get; set; }
    public string? TicketUrl { get; set; }
}
