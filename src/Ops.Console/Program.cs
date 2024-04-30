namespace Ops.Console
{
    using System;
    using System.Collections.Concurrent;
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
        private const string TokenEndpoint = "https://authenticatie.vlaanderen.be/op/v1/token";

        private const string RequiredScopes =
            "dv_ar_adres_beheer dv_ar_adres_uitzonderingen dv_gr_geschetstgebouw_beheer dv_gr_geschetstgebouw_uitzonderingen dv_gr_ingemetengebouw_uitzonderingen";

        private static string? _clientId;
        private static string? _clientSecret;
        private static AccessToken? _accessToken;

        private static readonly CsvConfiguration CsvConfiguration = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";",
            Encoding = System.Text.Encoding.UTF8,
        };

        protected Program()
        {
        }

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

            const string actionUrl =
                "https://api.basisregisters.vlaanderen.be/v2/gebouweenheden/{0}/acties/corrigeren/realisatie";

            var inputCsvPath = configuration.GetValue<string>("InputCsvPath");
            var outputCsvPath = configuration.GetValue<string>("OutputCsvPath");

            var idsToProcess = GetIdsToProcess(inputCsvPath, outputCsvPath);

            var client = new HttpClient();
            var processedRecords = new ConcurrentBag<ProcessedRecord>();
            try
            {
                foreach (var id in idsToProcess)
                {
                    Console.WriteLine($"Processing id: {id}");

                    try
                    {
                        var requestUrl = string.Format(actionUrl, id);

                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", await GetAccessToken());
                        var response = await client.PostAsync(requestUrl, null);

                        // Ensure the post was successful
                        if (response.IsSuccessStatusCode)
                        {
                            var ticketUri = response.Headers.Location;
                            processedRecords.Add(new ProcessedRecord(id, ticketUri.ToString()));
                        }
                        else
                        {
                            Console.WriteLine($"Failed to process id: {id}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to process id: {id}");
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
            finally
            {
                var fileStream = new FileStream(outputCsvPath!, FileMode.Append);
                await using var streamWriter = new StreamWriter(fileStream);
                await using var csvWriter = new CsvWriter(streamWriter, CsvConfiguration);
                csvWriter.Context.RegisterClassMap<ProcessedRecordMap>();
                await csvWriter.WriteRecordsAsync(processedRecords);
                await csvWriter.FlushAsync();
                await csvWriter.DisposeAsync();
            }

            //TODO: get processed records and call ticketurl, make output to csv where the ticket is status complete or error

            Console.WriteLine("Processing complete. Press any key to exit.");
            Console.Read();
        }

        private static List<string> GetIdsToProcess(
            string? inputCsvPath,
            string? processedCsvPath)
        {
            using var inputStreamReader = new StreamReader(inputCsvPath!);
            using var inputCsvReader = new CsvReader(inputStreamReader, CsvConfiguration);
            var inputIds = inputCsvReader.GetRecords<string>();

            var processedIds = GetProcessedIds(processedCsvPath);

            var idsToProcess = inputIds.Except(processedIds).ToList();
            return idsToProcess;
        }

        private static IEnumerable<string> GetProcessedIds(string? processedCsvPath)
        {
            using var outputStreamReader = new StreamReader(processedCsvPath!);
            using var outputCsvReader = new CsvReader(outputStreamReader, CsvConfiguration);
            outputCsvReader.Context.RegisterClassMap<ProcessedRecordMap>();

            var processedIds = outputCsvReader.GetRecords<ProcessedRecord>()
                .Select(x => x.Id);
            return processedIds;
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
                    Address = TokenEndpoint,
                    ClientId = _clientId!,
                    ClientSecret = _clientSecret,
                    Parameters = new Parameters(new[] { new KeyValuePair<string, string>("scope", RequiredScopes) })
                });

            var response = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);

            _accessToken = new AccessToken(response.AccessToken!, response.ExpiresIn);

            return _accessToken.Token;
        }
    }

    public record ProcessedRecord(string Id, string TicketUrl);

    public class ProcessedRecordMap : ClassMap<ProcessedRecord>
    {
        public ProcessedRecordMap()
        {
            Map(m => m.Id).Name("Id");
            Map(m => m.TicketUrl).Name("Ticket");
        }
    }

    public sealed class AccessToken
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
