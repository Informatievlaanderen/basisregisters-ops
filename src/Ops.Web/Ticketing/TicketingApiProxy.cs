namespace Ops.Web.Ticketing;

using System.Text.Json;
using System.Text.Json.Serialization;
using TicketingService.Abstractions;
using Flurl;

public class TicketingApiProxy : ITicketingApiProxy
{
    private readonly ITicketing _ticketing;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public TicketingApiProxy(
        ITicketing ticketing,
        HttpClient httpClient)
    {
        _ticketing = ticketing;
        _httpClient = httpClient;
        _jsonSerializerOptions = new JsonSerializerOptions();
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        _jsonSerializerOptions.PropertyNameCaseInsensitive = true;
    }

    public async Task<IEnumerable<Ticket>> Get(TicketsFilter filter, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(filter.TicketId))
        {
            var ticket = await _ticketing.Get(Guid.Parse(filter.TicketId), ct);
            return ticket is not null ? new[] { ticket } : Array.Empty<Ticket>();
        }

        var location = "/all";

        if (filter.Since.HasValue)
        {
            location = location.SetQueryParam("fromDate", filter.Since.Value.ToString("s"));
        }

        location = location.SetQueryParam("statuses", filter.Statuses.Where(x => x.Value).Select(x => (int)x.Key));
        location = location.SetQueryParam("offset", (filter.CurrentPage - 1) * TicketsFilter.Limit);
        location = location.SetQueryParam("limit", TicketsFilter.Limit);

        return (await _httpClient.GetFromJsonAsync<IEnumerable<Ticket>>(location, _jsonSerializerOptions, ct))!;
    }
}
