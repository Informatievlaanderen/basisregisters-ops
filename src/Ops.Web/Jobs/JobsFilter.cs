namespace Ops.Web.Jobs;

using Grb;

public class JobsFilter
{
    public const int Limit = 50;

    public static JobsFilter Default => new(1);

    private string? _jobId;
    public string? JobId
    {
        get => _jobId;
        set => _jobId = value.Trim();
    }

    public IDictionary<JobStatus, bool> Statuses { get; }
    public int CurrentPage { get; set; }

    private JobsFilter(int currentPage)
    {
        Statuses = Enum
            .GetValues(typeof(JobStatus))
            .OfType<JobStatus>()
            .ToDictionary(x => x, _ => false);
        CurrentPage = currentPage;
    }
}
