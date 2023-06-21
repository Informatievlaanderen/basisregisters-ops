namespace Ops.Web.Ticketing;

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
    public int CurrentPage { get; set; }


    private TicketsFilter(int currentPage)
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
}
