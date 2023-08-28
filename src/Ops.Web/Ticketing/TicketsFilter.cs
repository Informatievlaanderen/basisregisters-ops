namespace Ops.Web.Ticketing;

using System;
using System.Collections.Generic;
using TicketingService.Abstractions;

public class TicketsFilter
{
    public const int Limit = 50;

    public static TicketsFilter Default => new(1);

    private string? _ticketId;
    public string? TicketId
    {
        get => _ticketId;
        set => _ticketId = value.Trim();
    }

    public IDictionary<TicketStatus, bool> Statuses { get; }
    public DateTime? Since { get; set; }
    public DateTime? To { get; set; }
    public int CurrentPage { get; set; }


    public TicketsFilter(int currentPage)
    {
        Statuses = new Dictionary<TicketStatus, bool>
        {
            { TicketStatus.Created, false },
            { TicketStatus.Pending, false },
            { TicketStatus.Error, false },
            { TicketStatus.Complete, false }
        };
        CurrentPage = currentPage;
    }

    public void OpenTicketsOffToday()
    {
        _ticketId = null;

        Statuses[TicketStatus.Created] = true;
        Statuses[TicketStatus.Pending] = true;
        Statuses[TicketStatus.Error] = false;
        Statuses[TicketStatus.Complete] = false;

        Since = DateTimeOffset.Now.Date;
        To = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(15)).DateTime;

        CurrentPage = 1;
    }

    public void Clear()
    {
        _ticketId = null;

        Statuses[TicketStatus.Created] = false;
        Statuses[TicketStatus.Pending] = false;
        Statuses[TicketStatus.Error] = false;
        Statuses[TicketStatus.Complete] = false;

        Since = null;
        To = null;

        CurrentPage = 1;
    }
}
