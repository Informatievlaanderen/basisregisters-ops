namespace Ops.Web.Ticketing;

using TicketingService.Abstractions;

public interface ITicketingApiProxy
{
    Task<IEnumerable<Ticket>> GetAll(TicketsFilter filter, CancellationToken ct);
}
