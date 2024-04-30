namespace Ops.Console
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using CsvHelper;
    using CsvHelper.Configuration;
    using IdentityModel;
    using IdentityModel.Client;
    using Microsoft.Extensions.Configuration;

    public sealed class Program
    {
        private const string TokendEndpoint = "https://authenticatie.vlaanderen.be/op/v1/token";

        private const string RequiredScopes =
            "dv_ar_adres_beheer dv_ar_adres_uitzonderingen dv_gr_geschetstgebouw_beheer dv_gr_geschetstgebouw_uitzonderingen dv_gr_ingemetengebouw_uitzonderingen";

        private static string? _clientId;
        private static string? _clientSecret;
        private static AccessToken? _accessToken;

        private static readonly CsvConfiguration CsvConfiguration = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";",
        };

        protected Program()
        { }

        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true,
                    reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            _clientId = configuration.GetValue<string>("ClientId");
            _clientSecret = configuration.GetValue<string>("ClientSecret");

            var csvConfiguration = CsvConfiguration;

            const string actionUrl =
                "https://api.basisregisters.vlaanderen.be/v2/gebouweenheden/{0}/acties/corrigeren/realisatie";

            var inputCsvPath = configuration.GetValue<string>("InputCsvPath");
            var outputCsvPath = configuration.GetValue<string>("OutputCsvPath");

            var buildingUnitPersistentLocalIdsToProcess =
                GetBuildingUnitPersistentLocalIds(inputCsvPath, csvConfiguration, outputCsvPath);

            //
            // using var streamReader = new StreamReader(inputCsvPath!);
            // using var csvReader = new CsvReader(streamReader, csvConfiguration);

            var fileStream = new FileStream(outputCsvPath!, FileMode.Append);
            await using var streamWriter = new StreamWriter(fileStream);
            await using var csvWriter = new CsvWriter(streamWriter, csvConfiguration);

            var client = new HttpClient();
            foreach (var buildingUnitPersistentLocalId in buildingUnitPersistentLocalIdsToProcess)
            {
                var requestUrl = string.Format(actionUrl, buildingUnitPersistentLocalId);

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await GetAccessToken());
                var response = await client.PostAsync(requestUrl, null);

                // Ensure the post was successful
                if (response.IsSuccessStatusCode)
                {
                    var ticketUri = response.Headers.Location;
                }
                else
                {
                }
            }
        }

        private static List<string> GetBuildingUnitPersistentLocalIds(string? inputCsvPath,
            string? outputCsvPath)
        {
            CsvConfiguration csvConfiguration;

            using var inputStreamReader = new StreamReader(inputCsvPath!);
            using var inputCsvReader = new CsvReader(inputStreamReader, csvConfiguration);

            using var outputStreamReader = new StreamReader(outputCsvPath!);
            using var outputCsvReader = new CsvReader(inputStreamReader, csvConfiguration);

            var buildingUnitPersistentLocalIds = inputCsvReader.GetRecords<string>();
            var processedBuildingUnitPersistentLocalIds = outputCsvReader.GetRecords<ProcessedBuildingUnit>()
                .Select(x => x.Id);

            var buildingUnitPersistentLocalIdsToProcess =
                buildingUnitPersistentLocalIds.Except(processedBuildingUnitPersistentLocalIds).ToList();
            return buildingUnitPersistentLocalIdsToProcess;
        }

        private static async Task<string> GetAccessToken()
        {
            if (_accessToken is not null && !_accessToken.IsExpired)
            {
                return _accessToken.Token;
            }

            var tokenClient = new TokenClient(
                () => new HttpClient(),
                new TokenClientOptions
                {
                    Address = TokendEndpoint,
                    ClientId = _clientId!,
                    ClientSecret = _clientSecret,
                    Parameters = new Parameters(new[] { new KeyValuePair<string, string>("scope", RequiredScopes) })
                });

            var response = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);

            _accessToken = new AccessToken(response.AccessToken!, response.ExpiresIn);

            return _accessToken.Token;
        }
    }

    public record ProcessedBuildingUnit(string Id, string TicketUrl);


    public class AccessToken
    {
        private readonly DateTime _expiresAt;

        public string Token { get; }

        // Let's regard it as expired 10 seconds before it actually expires.
        public bool IsExpired => _expiresAt < DateTime.Now.Add(TimeSpan.FromSeconds(10));

        public AccessToken(string token, int expiresIn)
        {
            _expiresAt = DateTime.Now.AddSeconds(expiresIn);
            Token = token;
        }
    }
}
