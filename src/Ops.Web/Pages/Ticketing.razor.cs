namespace Ops.Web.Pages;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Web.Ticketing;
using TicketingService.Abstractions;

public partial class Ticketing
{
    private bool _ticketsLoaded;

    [Inject] private ITicketingApiProxy TicketingApiProxy { get; set; }
    [Inject] private IOptions<TicketingOptions> TicketingOptions { get; set; }

    private string CreateTicketingUrl(Guid id) => $"{TicketingOptions.Value.PublicApiTicketingUrl.TrimEnd('/')}/{id}";

    private List<Ticket> Tickets { get; } = new();
    private TicketsFilter TicketsFilter { get; set; } = TicketsFilter.Default;
    private bool HasNextPage { get; set; } = true;
    private string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        TicketsFilter = TicketsFilter.Default;
        await LoadTickets();
    }

    private async Task Filter()
    {
        TicketsFilter.CurrentPage = 1;
        await LoadTickets();
    }

    private async Task OpenTicketsOfToday()
    {
        TicketsFilter.OpenTicketsOfToday();
        await LoadTickets();
    }

    private async Task OpenTicketsLastThreeDays()
    {
        TicketsFilter.OpenTicketsLastThreeDays();
        await LoadTickets();
    }

    private void UpdateStatusFilter(TicketStatus status, bool isChecked)
    {
        TicketsFilter.Statuses[status] = isChecked;
    }

    private async Task LoadPage(int pageNumber)
    {
        TicketsFilter.CurrentPage = pageNumber;
        await LoadTickets();
    }

    private async Task LoadTickets()
    {
        _ticketsLoaded = false;
        Tickets.Clear();

        if (!string.IsNullOrWhiteSpace(TicketsFilter.TicketId) && !Guid.TryParse(TicketsFilter.TicketId, out _))
        {
            ErrorMessage = "Invalid ticket id specified";
        }
        else
        {
            Tickets.AddRange(await TicketingApiProxy.Get(TicketsFilter, CancellationToken.None));
        }

        HasNextPage = Tickets.Count >= TicketsFilter.Limit;
        _ticketsLoaded = true;
    }

    // private Dialog Dialog { get; set; } = new();
    // private TicketStatus DialogStatus { get; set; }
    // private bool DialogIsOpen { get; set; }
    // private Guid? SelectedTicketId { get; set; }
    // private void OpenDialog(Guid ticketId, TicketStatus ticketStatus)
    // {
    //     DialogStatus = ticketStatus;
    //     Dialog.Message = $"TicketId: {ticketId}";
    //
    //     if (ticketStatus== TicketStatus.Error)
    //     {
    //         Dialog.Caption = "Put ticket in error";
    //     }
    //     else if (DialogStatus == TicketStatus.Complete)
    //     {
    //         Dialog.Caption = "Put ticket in complete";
    //     }
    //
    //     Dialog.OnClose = new EventCallback<bool>(this, (Func<bool,Task>) PlaceTicketInStatus);
    //     SelectedTicketId = ticketId;
    //     DialogIsOpen = true;
    // }
    //
    // private async Task PlaceTicketInStatus(bool submit)
    // {
    //     if (SelectedTicketId is not null && submit && DialogStatus == TicketStatus.Error)
    //     {
    //         if (DialogStatus == TicketStatus.Error)
    //         {
    //             await Ticketing.Error(
    //                 SelectedTicketId.Value,
    //                 new TicketError("Placed in error via Ops user.",
    //                     string.Empty),
    //                 CancellationToken.None);
    //         }
    //         else if (DialogStatus == TicketStatus.Complete)
    //         {
    //             await Ticketing.Complete(
    //                 SelectedTicketId.Value,
    //                 new TicketResult(),
    //                 CancellationToken.None);
    //         }
    //     }
    //
    //     DialogIsOpen = false;
    // }
}
