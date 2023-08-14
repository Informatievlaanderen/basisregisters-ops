namespace Ops.Web.Jobs
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Grb.Building.Api.Abstractions.Responses;

    public interface IJobsApiProxy
    {
        Task<IEnumerable<JobResponse>> GetJobs(JobsFilter filter, CancellationToken ct);
        Task<IEnumerable<JobRecord>> GetJobRecords(JobRecordsFilter filter, CancellationToken ct);
        Task ResolveJobRecordError(JobRecord jobRecord, CancellationToken ct);
    }
}
