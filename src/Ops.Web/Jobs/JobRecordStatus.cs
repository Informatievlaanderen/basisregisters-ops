namespace Ops.Web.Jobs
{
    public enum JobRecordStatus
    {
        Created = 1,
        Pending,
        Warning,
        Error,
        ErrorResolved,
        Completed
    }
}
