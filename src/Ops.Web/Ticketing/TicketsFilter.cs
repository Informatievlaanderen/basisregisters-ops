namespace Ops.Web.Ticketing;

using System;
using System.Collections.Generic;
using TicketingService.Abstractions;

public class TicketsFilter
{
    public const int Limit = 50;
    public const int MinutesAfterStaleTicket = 5;

    public static TicketsFilter Default => new(1);

    private string? _ticketId;
    public string? TicketId
    {
        get => _ticketId;
        set => _ticketId = value.Trim();
    }

    public IDictionary<TicketStatus, bool> Statuses { get; }
    public IDictionary<string, bool> Registries { get; }
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

        Registries = new Dictionary<string, bool>
        {
            { "StreetNameRegistry", false },
            { "BuildingRegistry", false },
            { "AddressRegistry", false },
            { "MunicipalityRegistry", false },
            { "PostalRegistry", false },
            { "ParcelRegistry", false },
            { "RoadRegistry", false },
        };
        CurrentPage = currentPage;
    }

    public void OpenTicketsOfToday()
    {
        _ticketId = null;

        Statuses[TicketStatus.Created] = true;
        Statuses[TicketStatus.Pending] = true;
        Statuses[TicketStatus.Error] = false;
        Statuses[TicketStatus.Complete] = false;

        Since = DateTime.Now.Date;
        To = DateTime.Now.Subtract(TimeSpan.FromMinutes(MinutesAfterStaleTicket));

        CurrentPage = 1;
    }

    public void OpenTicketsLastThreeDays()
    {
        _ticketId = null;

        Statuses[TicketStatus.Created] = true;
        Statuses[TicketStatus.Pending] = true;
        Statuses[TicketStatus.Error] = false;
        Statuses[TicketStatus.Complete] = false;

        Since = DateTime.Now.Date.Subtract(TimeSpan.FromDays(3));
        To = DateTime.Now.Subtract(TimeSpan.FromMinutes(MinutesAfterStaleTicket));

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
