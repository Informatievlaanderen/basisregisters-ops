namespace Ops.Web.Jobs
{
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Flurl;
    using Grb.Building.Api.Abstractions.Responses;
    using IdentityModel;
    using IdentityModel.Client;
    using Microsoft.Extensions.Options;
    using Ticketing;

    public class JobsApiProxy : IJobsApiProxy
    {
        private const string DvGrIngemetengebouwBeheerScope = "dv_gr_ingemetengebouw_beheer";

        private readonly HttpClient _httpClient;
        private readonly JobsOptions _options;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        private AccessToken? _accessToken;

        public JobsApiProxy(HttpClient httpClient, IOptions<JobsOptions> jobsOptions)
        {
            _httpClient = httpClient;
            _options = jobsOptions.Value;
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            _jsonSerializerOptions.PropertyNameCaseInsensitive = true;
        }

        public async Task<IEnumerable<JobResponse>> GetJobs(JobsFilter filter, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(filter.JobId))
            {
                var job = await _httpClient.GetFromJsonAsync<JobResponse>($"/v2/uploads/jobs/{filter.JobId}", ct);
                return job is not null ? new[] { job } : Array.Empty<JobResponse>();
            }

            var location = "/v2/uploads/jobs";

            location = location.SetQueryParam("statuses", filter.Statuses.Where(x => x.Value).Select(x => (int)x.Key));
            location = location.SetQueryParam("offset", (filter.CurrentPage - 1) * TicketsFilter.Limit);
            location = location.SetQueryParam("limit", TicketsFilter.Limit);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await GetAccessToken(DvGrIngemetengebouwBeheerScope));

            var response = await _httpClient.GetFromJsonAsync<GetJobsResponse>(location, _jsonSerializerOptions, ct);

            return response.Jobs;
        }

        public async Task<IEnumerable<JobRecordResponse>> GetJobRecords(JobRecordsFilter filter, CancellationToken ct)
        {
            var location = $"/v2/uploads/jobs/{filter.JobId}/jobrecords";

            location = location.SetQueryParam("statuses", filter.Statuses.Where(x => x.Value).Select(x => (int)x.Key));
            location = location.SetQueryParam("offset", (filter.CurrentPage - 1) * TicketsFilter.Limit);
            location = location.SetQueryParam("limit", TicketsFilter.Limit);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await GetAccessToken(DvGrIngemetengebouwBeheerScope));

            var response = await _httpClient.GetFromJsonAsync<GetJobRecordsResponse>(location, _jsonSerializerOptions, ct);

            return response.JobRecords;
        }

        private async Task<string> GetAccessToken(string requiredScopes)
        {
            if (_accessToken is not null && !_accessToken.IsExpired)
            {
                return _accessToken.Token;
            }

            var tokenClient = new TokenClient(
                () => new HttpClient(),
                new TokenClientOptions
                {
                    Address = "https://authenticatie-ti.vlaanderen.be/op/v1/token",
                    ClientId = _options.ClientId,
                    ClientSecret = _options.ClientSecret,
                    Parameters = new Parameters(new[] { new KeyValuePair<string, string>("scope", requiredScopes) })
                });

            var response = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);

            _accessToken = new AccessToken(response.AccessToken, response.ExpiresIn);

            return _accessToken.Token;
        }
    }

    internal class AccessToken
    {
        private readonly DateTime _expiresAt;

        internal string Token { get; }

        // Let's regard it as expired 10 seconds before it actually expires.
        internal bool IsExpired => _expiresAt < DateTime.Now.Add(TimeSpan.FromSeconds(10));

        internal AccessToken(string token, int expiresIn)
        {
            _expiresAt = DateTime.Now.AddSeconds(expiresIn);
            Token = token;
        }
    }
}
