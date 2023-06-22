namespace Ops.Web.Jobs;

public class JobRecordsFilter
{
    public const int Limit = 50;

    public Guid JobId { get; }

    private string? _jobRecordId;
    public string? JobRecordId
    {
        get => _jobRecordId;
        set => _jobRecordId = value.Trim();
    }

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
