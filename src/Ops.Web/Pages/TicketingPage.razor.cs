namespace Ops.Web.Pages;

using Microsoft.AspNetCore.Components;
using Ticketing;
using TicketingService.Abstractions;

public partial class TicketingPage
{
    private bool TicketsLoaded = false;

    public static readonly IDictionary<string, int> SinceMapping = new Dictionary<string, int>
    {
        { "Today", 0 },
        { "3 days", 3 },
        { "1 week", 7 },
        { "2 weeks", 14 },
        { "1 month", 30 }
    };

    [Inject] private ITicketing Ticketing { get; set; }
    [Inject] private ITicketingApiProxy TicketingMonitoringApiProxy { get; set; }

    private Dialog Dialog { get; set; } = new Dialog();
    private TicketStatus DialogStatus { get; set; }

    private bool HasNextPage { get; set; } = true;
    private List<Ticket> Tickets { get; set; } = new();
    private TicketsFilter TicketsFilter { get; set; }

    private bool DialogIsOpen { get; set; }
    private Guid? SelectedTicketId { get; set; }
    private string? errorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        TicketsFilter = TicketsFilter.Empty;
        await LoadTickets();
    }

    private async Task LoadTickets()
    {
        TicketsLoaded = false;
        Tickets.Clear();

        if (TicketsFilter.TicketId is not null)
        {
            if (Guid.TryParse(TicketsFilter.TicketId, out Guid ticketId))
            {
                var ticket = await Ticketing.Get(ticketId, CancellationToken.None);
                if (ticket is not null)
                {
                    Tickets.Add(ticket);
                    TicketsFilter.TicketId = null;
                }
            }
            else
            {
                errorMessage = "Is not a valid ticketId";
            }
        }
        else
        {
            Tickets.AddRange(await TicketingMonitoringApiProxy.GetAll(TicketsFilter, CancellationToken.None));
        }

        HasNextPage = Tickets.Count >= TicketsFilter.Limit;
        TicketsLoaded = true;
    }

    private async Task Filter()
    {
        TicketsFilter.CurrentPage = 1;
        await LoadTickets();
    }

    private async Task LoadPage(int pageNumber)
    {
        TicketsFilter.CurrentPage = pageNumber;
        await LoadTickets();
    }

    private void StatusFilterOnInput(TicketStatus status, bool isChecked)
    {
        TicketsFilter.Status[status] = isChecked;
    }

    private void OpenDialog(Guid ticketId, TicketStatus ticketStatus)
    {
        DialogStatus = ticketStatus;
        Dialog.Message = $"TicketId: {ticketId}";

        if (ticketStatus== TicketStatus.Error)
        {
            Dialog.Caption = "Put ticket in error";
        }
        else if (DialogStatus == TicketStatus.Complete)
        {
            Dialog.Caption = "Put ticket in complete";
        }

        Dialog.OnClose = new EventCallback<bool>(this, (Func<bool,Task>) PlaceTicketInStatus);
        SelectedTicketId = ticketId;
        DialogIsOpen = true;
    }

    private async Task PlaceTicketInStatus(bool submit)
    {
        if (SelectedTicketId is not null && submit && DialogStatus == TicketStatus.Error)
        {
            if (DialogStatus == TicketStatus.Error)
            {
                await Ticketing.Error(
                    SelectedTicketId.Value,
                    new TicketError("Placed in error via Ops user.",
                        string.Empty),
                    CancellationToken.None);
            }
            else if (DialogStatus == TicketStatus.Complete)
            {
                await Ticketing.Complete(
                    SelectedTicketId.Value,
                    new TicketResult(),
                    CancellationToken.None);
            }
        }

        DialogIsOpen = false;
    }
}
