namespace Ops.Web.Ticketing;

using TicketingService.Abstractions;

public interface ITicketingApiProxy
{
    Task<IEnumerable<Ticket>> Get(TicketsFilter filter, CancellationToken ct);
}
