namespace Ops.Web.Jobs
{
    using Flurl;
    using Ticketing;

    public class JobsApiProxy : IJobsApiProxy
    {
        private readonly HttpClient _httpClient;

        public JobsApiProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Job>> GetJobs(JobsFilter filter, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(filter.JobId))
            {
                var job = await _httpClient.GetFromJsonAsync<Job>("/v2/uploads/jobs/{jobId}", ct);
                return job is not null ? new[] { job } : Array.Empty<Job>();
            }

            var location = "/v2/uploads/jobs";

            if (filter.Since.HasValue)
            {
                location = location.SetQueryParam("fromDate", filter.Since.Value.ToString("s"));
            }

            location = location.SetQueryParam("statuses", filter.Statuses.Where(x => x.Value).Select(x => (int)x.Key));
            location = location.SetQueryParam("offset", (filter.CurrentPage - 1) * TicketsFilter.Limit);
            location = location.SetQueryParam("limit", TicketsFilter.Limit);

            return (await _httpClient.GetFromJsonAsync<IEnumerable<Job>>(location, ct))!;
        }

        public Task<IEnumerable<JobRecord>> GetJobRecords(JobRecordsFilter filter, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
