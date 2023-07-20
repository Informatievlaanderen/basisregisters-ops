namespace Ops.Web.Jobs
{
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Flurl;
    using Grb;
    using Grb.Building.Api.Abstractions.Responses;
    using IdentityModel;
    using IdentityModel.Client;
    using Microsoft.Extensions.Options;
    using Ticketing;

    public class JobsApiProxy : IJobsApiProxy
    {
        private const string DvGrIngemetengebouwBeheerScope = "dv_gr_ingemetengebouw_beheer";
        private const string DvGrIngemetengebouwUitzonderingen = "dv_gr_ingemetengebouw_uitzonderingen";

        private readonly HttpClient _httpClient;
        private readonly JobsOptions _options;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        private IDictionary<string, AccessToken> _accessTokens;

        public JobsApiProxy(HttpClient httpClient, IOptions<JobsOptions> jobsOptions)
        {
            _httpClient = httpClient;
            _options = jobsOptions.Value;
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            _jsonSerializerOptions.PropertyNameCaseInsensitive = true;
            _accessTokens = new Dictionary<string, AccessToken>();
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

            return response?.Jobs ?? Array.Empty<JobResponse>();
        }

        public async Task<IEnumerable<JobRecord>> GetJobRecords(JobRecordsFilter filter, CancellationToken ct)
        {
            var location = $"/v2/uploads/jobs/{filter.JobId}/jobrecords";

            location = location.SetQueryParam("statuses",
                filter.Statuses.Where(x => x.Value).Select(x => (int)x.Key));
            location = location.SetQueryParam("offset", (filter.CurrentPage - 1) * TicketsFilter.Limit);
            location = location.SetQueryParam("limit", TicketsFilter.Limit);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await GetAccessToken(DvGrIngemetengebouwBeheerScope));

            var response = await _httpClient.GetFromJsonAsync<GetJobRecordsResponse>(location, _jsonSerializerOptions, ct);

            return response?.JobRecords
                       .Select(x => new JobRecord(x.Id, filter.JobId, x.RecordNumber, x.GrId, x.TicketUrl, x.Status, x.ErrorMessage, x.VersionDate))
                ?? Array.Empty<JobRecord>();
        }

        public async Task ResolveJobRecordError(JobRecord jobRecord, CancellationToken ct)
        {
            if (jobRecord.Status != JobRecordStatus.Error)
            {
                return;
            }

            var location = $"/v2/uploads/jobs/{jobRecord.JobId}/jobrecords/{jobRecord.JobRecordId}";

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await GetAccessToken($"{DvGrIngemetengebouwBeheerScope} {DvGrIngemetengebouwUitzonderingen}"));

            var response = await _httpClient.DeleteAsync(location, ct);
            response.EnsureSuccessStatusCode();

            // Update the job record manually so a reload is not necessary.
            jobRecord.Status = JobRecordStatus.ErrorResolved;
        }

        private async Task<string> GetAccessToken(string requiredScopes)
        {
            if (_accessTokens.ContainsKey(requiredScopes) && !_accessTokens[requiredScopes].IsExpired)
            {
                return _accessTokens[requiredScopes].Token;
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

            var accessToken = new AccessToken(response.AccessToken!, response.ExpiresIn);
            _accessTokens[requiredScopes] = accessToken;

            return accessToken.Token;
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
