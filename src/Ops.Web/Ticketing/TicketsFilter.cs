namespace Ops.Web.Ticketing;

using System.Text;
using TicketingService.Abstractions;
using Flurl;

public class TicketsFilter
{
    public static TicketsFilter Empty => new TicketsFilter(null, 1);
    public const int Limit = 2;

    public IDictionary<TicketStatus, bool> Status { get; set; }
    public int? AmountOfDaysSince { get; set; }

    public int CurrentPage { get; set; }

    public DateTime? Since => AmountOfDaysSince.HasValue
        ? DateTimeOffset.Now.Date.Subtract(TimeSpan.FromDays(AmountOfDaysSince.Value))
        : null;

    public TicketsFilter(int? amountOfDaysSince, int currentPage)
    {
        Status = new Dictionary<TicketStatus, bool>
        {
            { TicketStatus.Created, false },
            {  TicketStatus.Pending, false },
            {  TicketStatus.Error, false },
            {  TicketStatus.Complete, false }
        };
        AmountOfDaysSince = amountOfDaysSince;
        CurrentPage = currentPage;
    }
}

public static class TicketsFilterExtensions
{
    public static string AsHttpParameters(this TicketsFilter filter)
    {
        var p = "?";

        if (filter.Since.HasValue)
        {
            p.SetQueryParam("dateFrom", filter.Since.Value.ToString("s"));
        }

        return p;
    }
}
