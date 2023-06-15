namespace Ops.Web.Pages
{
    using Microsoft.AspNetCore.Components;

    public partial class Ticketing
    {
        [Inject]
        private TicketingApiProxy TicketingApiProxy { get; set; }
        private List<Ticket> Tickets { get; set; } = new List<Ticket>();
        private string? FilterId { get; set; }


        protected override async Task OnInitializedAsync()
        {
            Tickets = await TicketingApiProxy.GetTickets(TicketsFilter.Empty);
        }

        private async Task Filter()
        {
            Tickets.Clear();
            await Task.Delay(5000);
            var ticketId = !string.IsNullOrWhiteSpace(FilterId) ? Guid.Parse(FilterId.Trim()) : null as Guid?;
            var filter = new TicketsFilter(ticketId, null, null);
            Tickets = await TicketingApiProxy.GetTickets(filter);
        }
    }

    public class TicketingApiProxy
    {
        private List<Ticket> Tickets { get; set; }

        public TicketingApiProxy()
        {
            Tickets = SeedTickets();
        }

        public async Task<List<Ticket>> GetTickets(TicketsFilter ticketsFilter)
        {
            if (ticketsFilter.Id.HasValue && ticketsFilter.Id != Guid.Empty)
            {
                return await Task.FromResult(Tickets.Where(x => x.Id == ticketsFilter.Id).ToList());
            }

            IEnumerable<Ticket> tickets = Tickets;

            if (ticketsFilter.Status is not null)
            {
                tickets = tickets.Where(x => x.Status == ticketsFilter.Status);
            }

            if (ticketsFilter.Since is not null)
            {
                tickets = tickets.Where(x => x.Created >= ticketsFilter.Since);
            }

            return await Task.FromResult(tickets.ToList());
        }

        private static List<Ticket> SeedTickets()
        {
            return new List<Ticket>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Status = "Created",
                    LastModified = DateTime.UtcNow,
                    Created = DateTime.UtcNow.AddMinutes(-5)
                },
                new Ticket()
                {
                    Id = Guid.NewGuid(),
                    Status = "Pending",
                    LastModified = DateTime.UtcNow.AddMinutes(10),
                    Created = DateTime.UtcNow.AddMinutes(5)
                },
                new Ticket()
                {
                    Id = Guid.NewGuid(),
                    Status = "Cancelled",
                    LastModified = DateTime.UtcNow.AddHours(1),
                    Created = DateTime.UtcNow.AddMinutes(20)
                },
                new Ticket()
                {
                    Id = Guid.NewGuid(),
                    Status = "Created",
                    LastModified = DateTime.UtcNow,
                    Created = DateTime.UtcNow.AddMinutes(-5)
                },
                new Ticket()
                {
                    Id = Guid.NewGuid(),
                    Status = "Completed",
                    LastModified = DateTime.UtcNow.AddDays(1),
                    Created = DateTime.UtcNow.AddMinutes(300)
                },
                new Ticket()
                {
                    Id = Guid.NewGuid(),
                    Status = "Created",
                    LastModified = DateTime.UtcNow,
                    Created = DateTime.UtcNow.AddMinutes(-5)
                }
            };
        }
    }

    public class TicketsFilter
    {
        public static readonly TicketsFilter Empty = new TicketsFilter(null, null, null);

        public Guid? Id { get; set; }
        public string? Status { get; set; }
        public DateTime? Since { get; set; }

        public TicketsFilter(Guid? id, string? status, DateTime? since)
        {
            Id = id;
            Status = status;
            Since = since;
        }
    }

    public class Ticket
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
    }
}
