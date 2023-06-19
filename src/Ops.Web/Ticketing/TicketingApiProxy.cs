namespace Ops.Web.Ticketing;

using TicketingService.Abstractions;

public class TicketingApiProxy
{
    private readonly ITicketing _ticketing;

    public TicketingApiProxy(ITicketing ticketing)
    {
        _ticketing = ticketing;
    }
}
