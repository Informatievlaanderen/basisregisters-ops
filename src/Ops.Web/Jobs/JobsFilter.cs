namespace Ops.Web.Jobs;

using System;
using System.Collections.Generic;
using System.Linq;
using Grb;

public class JobsFilter
{
    public const int Limit = 50;

    public static JobsFilter Default => new(1);

    public DateTime? Since { get; set; }
    public DateTime? To { get; set; }

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
