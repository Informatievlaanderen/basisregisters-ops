namespace Ops.Web.Jobs;

public enum JobStatus
{
    Created = 1,
    Preparing,
    Prepared,
    Processing,
    Completed,
    Cancelled,
    Error
}
