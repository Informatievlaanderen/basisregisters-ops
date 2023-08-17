namespace Ops.Web.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Grb;

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
                return new[] { Jobs.Single(x => x.Id == Guid.Parse(filter.JobId)) };
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

        public Task CancelJob(Job job, CancellationToken ct)
        {
            job.Status = JobStatus.Cancelled;

            return Task.CompletedTask;
        }

        public Task ResolveJobRecordError(JobRecord jobRecord, CancellationToken ct)
        {
            if (jobRecord.Status == JobRecordStatus.Error)
            {
                jobRecord.Status = JobRecordStatus.ErrorResolved;
            }

            return Task.CompletedTask;
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

                var job = new Job(Guid.NewGuid(), new Uri("https://google.com"), status, created, created);
                return job;
            }

            JobRecord CreateJobRecord(
                Guid jobId,
                int recordNumber,
                JobRecordStatus jobRecordStatus,
                DateTimeOffset versionDate)
            {
                return new JobRecord(
                    recordNumber + 1000,
                    jobId,
                    recordNumber,
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
                .Range(1, count - 10)
                .Select(_ => CreateJob(
                    jobStatuses[randomizer.Next(0, jobStatuses.Length - 1)],
                    randomizer.Next(1, dateRange)))
                .Concat(Enumerable.Range(0, 10).Select(_ => CreateJob(
                        JobStatus.Error,
                        randomizer.Next(1, dateRange))))
                .ToList();

            var jobRecords = jobs
                .Where(job => job.Status != JobStatus.Created && job.Status != JobStatus.Cancelled)
                .ToDictionary(
                    job => job.Id,
                    job => Enumerable
                        .Range(1, randomizer.Next(30, 300))
                        .Select(recordNumber => CreateJobRecord(
                            job.Id,
                            recordNumber,
                            jobRecordStatuses[randomizer.Next(0, jobRecordStatuses.Length - 1)],
                            job.LastChanged.AddMilliseconds(randomizer.Next(100, 2000))))
                        .ToList());

            return new(jobs, jobRecords);
        }
    }
}
