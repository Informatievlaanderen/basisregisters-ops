namespace Ops.Web.Pages
{
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Mvc.Filters;

    public partial class Ticketing
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

        [Inject] private TicketingApiProxy TicketingApiProxy { get; set; }

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

            await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(200, 2500)));

            Tickets.AddRange(await TicketingApiProxy.GetTickets(TicketsFilter));
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

        private void StatusFilterOnInput(string status, bool isChecked)
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
                await TicketingApiProxy.PlaceTicketInError(SelectedTicketId.Value);
            }

            DialogIsOpen = false;
        }
    }

    public class TicketingApiProxy
    {
        private List<Ticket> Tickets { get; set; }

        public TicketingApiProxy()
        {
            Tickets = SeedTickets(150);
        }

        public async Task<List<Ticket>> GetTickets(TicketsFilter ticketsFilter)
        {
            if (ticketsFilter.Id.HasValue && ticketsFilter.Id != Guid.Empty)
            {
                return await Task.FromResult(Tickets.Where(x => x.Id == ticketsFilter.Id).ToList());
            }

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
            var ticket = Tickets.Single(x => x.Id == ticketId);
            ticket.Status = "Error";

            await Task.CompletedTask;
        }

        private static List<Ticket> SeedTickets(int count)
        {
            var statuses = new[] { "Created", "Pending", "Completed", "Error" };
            var dateRange = 30; // days

            var randomizer = new Random();

            Ticket CreateTicket(string status, int daysToSubtract)
            {
                var created = DateTime.UtcNow.Subtract(TimeSpan.FromDays(daysToSubtract));

                return new()
                {
                    Id = Guid.NewGuid(),
                    Status = status,
                    Created = created,
                    LastModified = status == "Created" ? created : created.AddMilliseconds(548)
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

    public class TicketsFilter
    {
        public static TicketsFilter Empty => new TicketsFilter(null, null, 1);
        public const int Limit = 50;

        public Guid? Id { get; set; }
        public IDictionary<string, bool> Status { get; set; }
        public int? AmountOfDaysSince { get; set; }

        public int CurrentPage { get; set; }

        public DateTime? Since => AmountOfDaysSince.HasValue
            ? DateTimeOffset.Now.Date.Subtract(TimeSpan.FromDays(AmountOfDaysSince.Value))
            : null;

        public TicketsFilter(Guid? id, int? amountOfDaysSince, int currentPage)
        {
            Id = id;
            Status = new Dictionary<string, bool>
            {
                { "Created", false },
                { "Pending", false },
                { "Error", false },
                { "Completed", false }
            };
            AmountOfDaysSince = amountOfDaysSince;
            CurrentPage = currentPage;
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
