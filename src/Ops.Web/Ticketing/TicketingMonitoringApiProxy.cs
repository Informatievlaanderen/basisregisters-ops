namespace Ops.Web.Ticketing;

using System.Text.Json;
using System.Text.Json.Serialization;
using TicketingService.Abstractions;
using Flurl;

public class TicketingMonitoringApiProxy : ITicketingApiProxy
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public TicketingMonitoringApiProxy(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = new JsonSerializerOptions();
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        _jsonSerializerOptions.PropertyNameCaseInsensitive = true;
    }

    public async Task<IEnumerable<Ticket>> GetAll(TicketsFilter filter, CancellationToken ct)
    {
        var location = "/all";

        if (filter.Since.HasValue)
        {
            location= location.SetQueryParam("fromDate", filter.Since.Value.ToString("s"));
        }

        location = location.SetQueryParam("statuses", filter.Status.Where(x => x.Value).Select(x => (int)x.Key));
        location = location.SetQueryParam("offset", (filter.CurrentPage-1) * TicketsFilter.Limit);
        location = location.SetQueryParam("limit", TicketsFilter.Limit);

        var result =  await _httpClient.GetFromJsonAsync<IEnumerable<Ticket>>(location, _jsonSerializerOptions, ct) ?? Enumerable.Empty<Ticket>();
        return result;
    }
}
