﻿namespace Ops.Web.Pages;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grb;
using Jobs;
using Microsoft.AspNetCore.Components;
using JobRecord = Jobs.JobRecord;

public partial class GrbJobRecords
{
    private bool JobRecordsLoaded;

    [Inject] private IJobsApiProxy JobsApiProxy { get; set; }
    [Parameter] public Guid JobId { get; set; }

    private List<JobRecord> JobRecords { get; } = new();
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

    private bool DialogIsOpen { get; set; }
    private string DialogCaption => $"Job record with ID {SelectedJobRecord!.JobRecordId}";
    private string DialogMessage => $"Error: {SelectedJobRecord!.ErrorMessage}";
    private JobRecord? SelectedJobRecord { get; set; }

    private void OpenDialog(JobRecord jobRecord)
    {
        SelectedJobRecord = jobRecord;
        DialogIsOpen = true;
    }

    private async Task ResolveJobRecordError(bool submit)
    {
        if (SelectedJobRecord is not null && submit)
        {
            await JobsApiProxy.ResolveJobRecordError(SelectedJobRecord, CancellationToken.None);
        }

        DialogIsOpen = false;
    }
}
