﻿namespace Ops.Web.Ticketing;

using TicketingService.Abstractions;

public class FakeTicketingApiProxy : ITicketingApiProxy
    {
        private List<Ticket> Tickets { get; set; }

        public FakeTicketingApiProxy()
        {
            Tickets = SeedTickets(150);
        }

        public async Task<IEnumerable<Ticket>> GetAll(TicketsFilter ticketsFilter, CancellationToken ct)
        {
            IEnumerable<Ticket> tickets = Tickets;

            if (ticketsFilter.Status.Any(x => x.Value))
            {
                tickets = tickets.Where(x =>
                    ticketsFilter.Status.Where(y => y.Value).Select(z => z.Key).Contains(x.Status));
            }

            if (ticketsFilter.AmountOfDaysSince is not null)
            {
                tickets = tickets.Where(x => x.Created >= ticketsFilter.Since);
            }

            const int limit = 50;
            var offset = (ticketsFilter.CurrentPage - 1) * limit;

            return await Task.FromResult(
                tickets
                    .OrderBy(x => x.Created)
                    .Skip(offset)
                    .Take(limit)
                    .ToList());
        }

        public async Task PlaceTicketInError(Guid ticketId)
        {
            var ticket = Tickets.Single(x => x.TicketId == ticketId);
            ticket.Status = TicketStatus.Error;

            await Task.CompletedTask;
        }

        private static List<Ticket> SeedTickets(int count)
        {
            var statuses = new[] { TicketStatus.Created, TicketStatus.Pending, TicketStatus.Complete, TicketStatus.Error };
            var dateRange = 30; // days

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
