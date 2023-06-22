namespace Ops.Web.Jobs
{
    public interface IJobsApiProxy
    {
        Task<IEnumerable<Job>> GetJobs(JobsFilter filter, CancellationToken ct);
        Task<IEnumerable<JobRecord>> GetJobRecords(JobRecordsFilter filter, CancellationToken ct);
    }
}
