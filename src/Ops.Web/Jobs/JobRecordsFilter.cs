namespace Ops.Web.Jobs;

using Grb;

public class JobRecordsFilter
{
    public const int Limit = 50;

    public Guid JobId { get; }

    public IDictionary<JobRecordStatus, bool> Statuses { get; }
    public int CurrentPage { get; set; }

    public JobRecordsFilter(Guid jobId)
    {
        JobId = jobId;
        CurrentPage = 1;
        Statuses = Enum
            .GetValues(typeof(JobRecordStatus))
            .OfType<JobRecordStatus>()
            .ToDictionary(x => x, _ => false);
    }
}
