namespace Ops.Web.Ticketing;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TicketingService.Abstractions;

public interface ITicketingApiProxy
{
    Task<IEnumerable<Ticket>> Get(TicketsFilter filter, CancellationToken ct);
}
