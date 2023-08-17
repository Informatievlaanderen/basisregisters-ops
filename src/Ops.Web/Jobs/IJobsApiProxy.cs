namespace Ops.Web.Jobs
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IJobsApiProxy
    {
        Task<IEnumerable<Job>> GetJobs(JobsFilter filter, CancellationToken ct);
        Task<IEnumerable<JobRecord>> GetJobRecords(JobRecordsFilter filter, CancellationToken ct);
        Task CancelJob(Job job, CancellationToken ct);
        Task ResolveJobRecordError(JobRecord jobRecord, CancellationToken ct);
    }
}
