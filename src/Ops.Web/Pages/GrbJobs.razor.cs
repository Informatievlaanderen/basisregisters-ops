namespace Ops.Web.Pages
{
    using Grb;
    using Grb.Building.Api.Abstractions.Responses;
    using Jobs;
    using Microsoft.AspNetCore.Components;

    public partial class GrbJobs
    {
        private bool JobsLoaded;

        [Inject] private IJobsApiProxy JobsApiProxy { get; set; }

        private List<JobResponse> Jobs { get; } = new();
        private JobsFilter JobsFilter { get; set; }
        private bool HasNextPage { get; set; } = true;
        private string? ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            JobsFilter = JobsFilter.Default;
            await LoadJobs();
        }

        private async Task Filter()
        {
            JobsFilter.CurrentPage = 1;
            await LoadJobs();
        }

        private void UpdateStatusFilter(JobStatus status, bool isChecked)
        {
            JobsFilter.Statuses[status] = isChecked;
        }

        private async Task LoadPage(int pageNumber)
        {
            JobsFilter.CurrentPage = pageNumber;
            await LoadJobs();
        }

        private async Task LoadJobs()
        {
            JobsLoaded = false;
            Jobs.Clear();

            if (!string.IsNullOrWhiteSpace(JobsFilter.JobId) && !Guid.TryParse(JobsFilter.JobId, out _))
            {
                ErrorMessage = "Invalid ticket id specified";
            }
            else
            {
                Jobs.AddRange(await JobsApiProxy.GetJobs(JobsFilter, CancellationToken.None));
            }

            HasNextPage = Jobs.Count >= JobsFilter.Limit;
            JobsLoaded = true;
        }

    }
}
