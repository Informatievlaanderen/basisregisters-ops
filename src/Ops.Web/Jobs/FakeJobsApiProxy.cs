namespace Ops.Web.Jobs
{
    using Grb;
    using Grb.Building.Api.Abstractions.Responses;

    public class FakeJobsApiProxy : IJobsApiProxy
    {
        private List<JobResponse> Jobs { get; }
        private Dictionary<Guid, List<JobRecordResponse>> JobRecords { get; }

        public FakeJobsApiProxy()
        {
            var seed = SeedJobs(150);
            Jobs = seed.jobs;
            JobRecords = seed.jobRecords;
        }

        public async Task<IEnumerable<JobResponse>> GetJobs(JobsFilter filter, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(filter.JobId))
            {
                return new[] { Jobs.Single(x => x.Id == Guid.Parse(filter.JobId)) };
            }

            IEnumerable<JobResponse> jobs = Jobs;

            if (filter.Statuses.Any(x => x.Value))
            {
                jobs = jobs
                    .Where(x =>
                        filter.Statuses
                            .Where(y => y.Value)
                            .Select(z => z.Key)
                            .Contains(x.Status));
            }

            var offset = (filter.CurrentPage - 1) * JobsFilter.Limit;

            return await Task.FromResult(
                jobs
                    .OrderBy(x => x.Created)
                    .Skip(offset)
                    .Take(JobsFilter.Limit)
                    .ToList());
        }

        public Task<IEnumerable<JobRecordResponse>> GetJobRecords(JobRecordsFilter filter, CancellationToken ct)
        {
            IEnumerable<JobRecordResponse> jobRecords = JobRecords[filter.JobId];

            if (filter.Statuses.Any(x => x.Value))
            {
                jobRecords = jobRecords
                    .Where(x =>
                        filter.Statuses
                            .Where(y => y.Value)
                            .Select(z => z.Key)
                            .Contains(x.Status));
            }

            var offset = (filter.CurrentPage - 1) * JobRecordsFilter.Limit;

            return Task.FromResult(
                jobRecords
                    .OrderBy(x => x.RecordNumber)
                    .Skip(offset)
                    .Take(JobRecordsFilter.Limit));
        }

        private static (List<JobResponse> jobs, Dictionary<Guid, List<JobRecordResponse>> jobRecords) SeedJobs(int count)
        {
            var jobStatuses = Enum.GetValues(typeof(JobStatus)).OfType<JobStatus>().ToArray();
            var jobRecordStatuses = Enum.GetValues(typeof(JobRecordStatus)).OfType<JobRecordStatus>().ToArray();

            const int dateRange = 30; // days

            var randomizer = new Random();

            JobResponse CreateJob(JobStatus status, int daysToSubtract)
            {
                var created = DateTime.UtcNow.Subtract(TimeSpan.FromDays(daysToSubtract));

                var job = new JobResponse(Guid.NewGuid(), new Uri("https://google.com"), status, created, created, new Uri("https://google.com"));
                return job;
            }

            JobRecordResponse CreateJobRecord(
                int recordNumber,
                JobRecordStatus jobRecordStatus,
                DateTimeOffset versionDate)
            {
                return new JobRecordResponse(
                    recordNumber,
                    recordNumber + 1000,
                    recordNumber * 3 + 1000000,
                    jobRecordStatus != JobRecordStatus.Created ? new Uri("https://google.com") : null,
                    jobRecordStatus,
                    jobRecordStatus is JobRecordStatus.Warning or JobRecordStatus.Error
                        or JobRecordStatus.ErrorResolved
                        ? "Some error occured"
                        : null,
                    versionDate);
            }

            var jobs = Enumerable
                .Range(1, count)
                .Select(_ => CreateJob(
                    jobStatuses[randomizer.Next(0, jobStatuses.Length - 1)],
                    randomizer.Next(1, dateRange)))
                .ToList();

            var jobRecords = jobs
                .Where(job => job.Status != JobStatus.Created && job.Status != JobStatus.Cancelled)
                .ToDictionary(
                    job => job.Id,
                    job => Enumerable
                        .Range(1, randomizer.Next(30, 300))
                        .Select(recordNumber => CreateJobRecord(
                            recordNumber,
                            jobRecordStatuses[randomizer.Next(0, jobRecordStatuses.Length - 1)],
                            job.LastChanged.AddMilliseconds(randomizer.Next(100, 2000))))
                        .ToList());

            return new(jobs, jobRecords);
        }
    }
}
