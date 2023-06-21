namespace Ops.Web.Jobs
{
    public interface IJobsApiProxy
    {
        Task<IEnumerable<Job>> GetJobs(JobsFilter filter, CancellationToken ct);
        // GetJobRecords
    }
}
