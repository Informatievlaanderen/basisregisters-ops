namespace Ops.Web.Ticketing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TicketingService.Abstractions;

public class FakeTicketingApiProxy : ITicketingApiProxy
{
    private List<Ticket> Tickets { get; }

    public FakeTicketingApiProxy()
    {
        Tickets = SeedTickets(150);
    }

    public async Task<IEnumerable<Ticket>> Get(TicketsFilter filter, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(filter.TicketId))
        {
            return new[] { Tickets.Single(x => x.TicketId == Guid.Parse(filter.TicketId)) };
        }

        IEnumerable<Ticket> tickets = Tickets;

        if (filter.Statuses.Any(x => x.Value))
        {
            tickets = tickets
                .Where(x =>
                    filter.Statuses
                        .Where(y => y.Value)
                        .Select(z => z.Key)
                        .Contains(x.Status));
        }

        if (filter.Since is not null)
        {
            tickets = tickets.Where(x => x.Created >= filter.Since);
        }

        var offset = (filter.CurrentPage - 1) * TicketsFilter.Limit;

        return await Task.FromResult(
            tickets
                .OrderBy(x => x.Created)
                .Skip(offset)
                .Take(TicketsFilter.Limit)
                .ToList());
    }

    // public async Task PlaceTicketInError(Guid ticketId)
    // {
    //     var ticket = Tickets.Single(x => x.TicketId == ticketId);
    //     ticket.Status = TicketStatus.Error;
    //
    //     await Task.CompletedTask;
    // }

    private static List<Ticket> SeedTickets(int count)
    {
        var statuses = new[] { TicketStatus.Created, TicketStatus.Pending, TicketStatus.Complete, TicketStatus.Error };
        const int dateRange = 30; // days

        var randomizer = new Random();
        Ticket CreateTicket(TicketStatus ticketStatus, int daysToSubtract)
        {
            var created = DateTime.UtcNow.Subtract(TimeSpan.FromDays(daysToSubtract));

            return new Ticket(Guid.NewGuid(), ticketStatus, new Dictionary<string, string>())
            {
                Created = created,
                LastModified = ticketStatus == TicketStatus.Created ? created : created.AddMilliseconds(548)
            };
        }

        return Enumerable
            .Range(1, count)
            .Select(_ => CreateTicket(
                statuses[randomizer.Next(0, statuses.Length - 1)],
                randomizer.Next(1, dateRange)))
            .ToList();
    }
}
