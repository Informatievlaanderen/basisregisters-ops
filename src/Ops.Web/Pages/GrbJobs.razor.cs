namespace Ops.Web.Pages;

public partial class GrbJobs
{
    private bool JobsLoaded = false;
    private List<Job> Jobs { get; set; } = new();


    protected override async Task OnInitializedAsync()
    {
        Jobs = new List<Job>()
        {
            new Job()
            {
                Id = Guid.NewGuid(),
                TicketId = Guid.NewGuid(),
                Status = JobStatus.Completed,
                Created = DateTimeOffset.Now,
                LastChanged = DateTimeOffset.Now
            },
            new Job()
            {
                Id = Guid.NewGuid(),
                TicketId = Guid.NewGuid(),
                Status = JobStatus.Completed,
                Created = DateTimeOffset.Now,
                LastChanged = DateTimeOffset.Now
            },
            new Job()
            {
                Id = Guid.NewGuid(),
                TicketId = Guid.NewGuid(),
                Status = JobStatus.Completed,
                Created = DateTimeOffset.Now,
                LastChanged = DateTimeOffset.Now
            },
        };
        StateHasChanged();
        JobsLoaded = true;
        await Task.CompletedTask;
    }
}

internal class Job
{
    public Guid Id { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset LastChanged { get; set; }
    public JobStatus Status { get; set; }
    public Guid? TicketId { get; set; }
}

public enum JobStatus
{
    Created = 1,
    Preparing,
    Prepared,
    Processing,
    Completed,
    Cancelled,
    Error
}
