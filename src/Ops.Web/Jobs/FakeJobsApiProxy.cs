namespace Ops.Web.Jobs
{
    public class FakeJobsApiProxy : IJobsApiProxy
    {
        private List<Job> Jobs { get; }
        private Dictionary<Guid, List<JobRecord>> JobRecords { get; }

        public FakeJobsApiProxy()
        {
            var seed = SeedJobs(150);
            Jobs = seed.jobs;
            JobRecords = seed.jobRecords;
        }

        public async Task<IEnumerable<Job>> GetJobs(JobsFilter filter, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(filter.JobId))
            {
                return new[] { Jobs.Single(x => x.JobId == Guid.Parse(filter.JobId)) };
            }

            IEnumerable<Job> jobs = Jobs;

            if (filter.Statuses.Any(x => x.Value))
            {
                jobs = jobs
                    .Where(x =>
                        filter.Statuses
                            .Where(y => y.Value)
                            .Select(z => z.Key)
                            .Contains(x.Status));
            }

            if (filter.Since is not null)
            {
                jobs = jobs.Where(x => x.Created >= filter.Since);
            }

            var offset = (filter.CurrentPage - 1) * JobsFilter.Limit;

            return await Task.FromResult(
                jobs
                    .OrderBy(x => x.Created)
                    .Skip(offset)
                    .Take(JobsFilter.Limit)
                    .ToList());
        }

        public Task<IEnumerable<JobRecord>> GetJobRecords(JobRecordsFilter filter, CancellationToken ct)
        {
            IEnumerable<JobRecord> jobRecords = JobRecords[filter.JobId];

            if (!string.IsNullOrWhiteSpace(filter.JobRecordId))
            {
                return Task.FromResult(
                    new[] { jobRecords.Single(x => x.Id == Guid.Parse(filter.JobRecordId)) }.AsEnumerable()
                );
            }

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

        private static (List<Job> jobs, Dictionary<Guid, List<JobRecord>> jobRecords) SeedJobs(int count)
        {
            var jobStatuses = Enum.GetValues(typeof(JobStatus)).OfType<JobStatus>().ToArray();
            var jobRecordStatuses = Enum.GetValues(typeof(JobRecordStatus)).OfType<JobRecordStatus>().ToArray();

            const int dateRange = 30; // days

            var randomizer = new Random();

            Job CreateJob(JobStatus status, int daysToSubtract)
            {
                var created = DateTime.UtcNow.Subtract(TimeSpan.FromDays(daysToSubtract));

                return new Job
                {
                    JobId = Guid.NewGuid(),
                    Status = status,
                    Created = created,
                    LastChanged = status == JobStatus.Created ? created : created.AddMilliseconds(548),
                    TicketUrl = status == JobStatus.Created ? null : "http://google.com"
                };
            }

            JobRecord CreateJobRecord(
                int recordNumber,
                JobRecordStatus jobRecordStatus,
                DateTimeOffset versionDate)
            {
                return new JobRecord
                {
                    Id = Guid.NewGuid(),
                    RecordNumber = recordNumber,
                    Status = jobRecordStatus,
                    ErrorMessage = jobRecordStatus is JobRecordStatus.Warning or JobRecordStatus.Error
                        or JobRecordStatus.ErrorResolved
                        ? "Some error happend"
                        : null,
                    GrId = recordNumber * 3 + 1000000,
                    TicketId = jobRecordStatus != JobRecordStatus.Created ? Guid.NewGuid() : null,
                    VersionDate = versionDate
                };
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
                    job => job.JobId,
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
