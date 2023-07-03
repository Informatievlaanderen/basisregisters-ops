namespace Ops.Web.Jobs
{
    using Grb.Building.Api.Abstractions.Responses;

    public interface IJobsApiProxy
    {
        Task<IEnumerable<JobResponse>> GetJobs(JobsFilter filter, CancellationToken ct);
        Task<IEnumerable<JobRecordResponse>> GetJobRecords(JobRecordsFilter filter, CancellationToken ct);
    }
}
