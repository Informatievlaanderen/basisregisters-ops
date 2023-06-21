namespace Ops.Web.Jobs
{
    public class FakeJobsApiProxy : IJobsApiProxy
    {
        private List<Job> Jobs { get; }

        public FakeJobsApiProxy()
        {
            Jobs = SeedJobs(150);
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

        private static List<Job> SeedJobs(int count)
        {
            var statuses = Enum.GetValues(typeof(JobStatus)).OfType<JobStatus>().ToArray();
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

            return Enumerable
                .Range(1, count)
                .Select(_ => CreateJob(
                    statuses[randomizer.Next(0, statuses.Length - 1)],
                    randomizer.Next(1, dateRange)))
                .ToList();
        }
    }
}
