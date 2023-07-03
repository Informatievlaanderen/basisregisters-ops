namespace Ops.Web.Pages;

using Components;
using Grb;
using Grb.Building.Api.Abstractions.Responses;
using Jobs;
using Microsoft.AspNetCore.Components;

public partial class GrbJobRecords
{
    private bool JobRecordsLoaded;

    [Inject] private IJobsApiProxy JobsApiProxy { get; set; }
    [Parameter] public Guid JobId { get; set; }

    private List<JobRecordResponse> JobRecords { get; } = new();
    private JobRecordsFilter JobRecordsFilter { get; set; }
    private bool HasNextPage { get; set; } = true;
    private string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        JobRecordsFilter = new JobRecordsFilter(JobId);
        await LoadJobs();
    }

    private async Task Filter()
    {
        JobRecordsFilter.CurrentPage = 1;
        await LoadJobs();
    }

    private void UpdateStatusFilter(JobRecordStatus status, bool isChecked)
    {
        JobRecordsFilter.Statuses[status] = isChecked;
    }

    private async Task LoadPage(int pageNumber)
    {
        JobRecordsFilter.CurrentPage = pageNumber;
        await LoadJobs();
    }

    private async Task LoadJobs()
    {
        JobRecordsLoaded = false;
        JobRecords.Clear();

        JobRecords.AddRange(await JobsApiProxy.GetJobRecords(JobRecordsFilter, CancellationToken.None));

        HasNextPage = JobRecords.Count >= JobRecordsFilter.Limit;
        JobRecordsLoaded = true;
    }

    private Dialog Dialog { get; set; } = new();
    private bool DialogIsOpen { get; set; }
    private JobRecordResponse? SelectedJobRecord { get; set; }
    private void OpenDialog(JobRecordResponse jobRecord)
    {
        Dialog.Caption = $"Job record with ID {jobRecord.Id}";
        Dialog.Message = $"Error: {jobRecord.ErrorMessage}";

        Dialog.OnClose = new EventCallback<bool>(this, (Func<bool,Task>) ResolveJobRecordError);
        SelectedJobRecord = jobRecord;
        DialogIsOpen = true;
    }

    private async Task ResolveJobRecordError(bool submit)
    {
        if (SelectedJobRecord is not null && submit)
        {
            // await JobsApiProxy.ResolveJobRecordError(SelectedJobRecord.Id);
        }

        DialogIsOpen = false;
    }
}
