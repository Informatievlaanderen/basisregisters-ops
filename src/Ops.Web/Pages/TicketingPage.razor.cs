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

    private bool HasNextPage { get; set; } = true;
    private List<Ticket> Tickets { get; set; } = new();
    private TicketsFilter TicketsFilter { get; set; }

    private bool DialogIsOpen { get; set; }
    private Guid? SelectedTicketId { get; set; }
    protected override async Task OnInitializedAsync()
    {
        TicketsFilter = TicketsFilter.Empty;
        await LoadTickets();
    }

    private async Task LoadTickets()
    {
        TicketsLoaded = false;
        Tickets.Clear();

        //await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(200, 2500)));

        Tickets.AddRange(await TicketingMonitoringApiProxy.GetAll(TicketsFilter, CancellationToken.None));

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

    private void OpenDialog(Guid ticketId)
    {
        SelectedTicketId = ticketId;
        DialogIsOpen = true;
    }

    private async Task PlaceTicketInError(bool submit)
    {
        if (SelectedTicketId is not null && submit)
        {
            //await FakeTicketingApiProxy.PlaceTicketInError(SelectedTicketId.Value);
        }

        DialogIsOpen = false;
    }
}
