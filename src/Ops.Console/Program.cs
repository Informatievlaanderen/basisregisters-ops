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
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Microsoft.Extensions.Configuration;

    public sealed class Program
    {
        private static readonly CsvConfiguration CsvConfiguration = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            Encoding = Encoding.UTF8,
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

            var acmIdmService = AcmIdmService.GetInstance(configuration);

            var inputCsvPath = configuration.GetValue<string>("InputCsvPath");
            var outputCsvPath = configuration.GetValue<string>("OutputCsvPath");
            var apiKey = configuration.GetValue<string>("ApiKey");

            var idsToProcess = GetIdsToProcess(inputCsvPath, outputCsvPath);
            Console.WriteLine($"Processing {idsToProcess.Count} records...");

            var cts = new CancellationTokenSource();
            var client = new HttpClient();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            }

            var processedRecords = new ConcurrentBag<ProcessedRecord>();
            var unprocessedRecords = new ConcurrentBag<string>();
            var countProcessed = 0;
            try
            {
                await Parallel.ForEachAsync(
                    idsToProcess,
                    new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = cts.Token },
                    async (inputRecord, token) =>
                {
                    if(token.IsCancellationRequested)
                        return;

                    Interlocked.Increment(ref countProcessed);

                    Console.WriteLine(
                        $"Processing id: {inputRecord.Id} ({Math.Round((double)countProcessed / idsToProcess.Count * 100, 2)}%)");

                    try
                    {
                        var requestUrl = inputRecord.Url.Replace("{id}", inputRecord.Id);

                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", acmIdmService.GetAccessToken());

                        HttpResponseMessage response;
                        if (!string.IsNullOrWhiteSpace(inputRecord.Body))
                        {
                            var jsonDocument = JsonDocument.Parse(inputRecord.Body);
                            response = await client.PostAsJsonAsync(requestUrl, jsonDocument, token);
                        }
                        else
                        {
                           response = await client.PostAsync(requestUrl, null, token);
                        }

                        // Ensure the post was successful
                        if (response.IsSuccessStatusCode)
                        {
                            var ticketUri = response.Headers.Location;
                            processedRecords.Add(new ProcessedRecord(inputRecord.Id, ticketUri!.ToString()));
                        }
                        else
                        {
                            unprocessedRecords.Add(inputRecord.Id);
                            Console.WriteLine($"Failed to process id: {inputRecord.Id}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to process id: {inputRecord.Id}");
                        Console.WriteLine(e);
                        await cts.CancelAsync();
                        throw;
                    }
                });

                if (unprocessedRecords.Any())
                {
                    Console.WriteLine("Failed to process following records");
                    foreach (var unprocessedRecord in unprocessedRecords)
                    {
                        Console.WriteLine(unprocessedRecord);
                    }
                }
            }
            finally
            {
                if (GetProcessedIds(outputCsvPath).Any())
                {
                    CsvConfiguration.HasHeaderRecord = false;
                }

                var fileStream = new FileStream(outputCsvPath!, FileMode.Append);
                await using var streamWriter = new StreamWriter(fileStream);
                await using var csvWriter = new CsvWriter(streamWriter, CsvConfiguration);

                csvWriter.Context.RegisterClassMap<ProcessedRecordMap>();
                await csvWriter.WriteRecordsAsync(processedRecords, CancellationToken.None);
                await csvWriter.FlushAsync();
            }

            Console.WriteLine("Processing complete. Press any key to exit.");
            Console.Read();
        }

        private static List<InputRecord> GetIdsToProcess(
            string? inputCsvPath,
            string? processedCsvPath)
        {
            using var inputStreamReader = new StreamReader(inputCsvPath!);
            using var inputCsvReader = new CsvReader(inputStreamReader, CsvConfiguration);
            inputCsvReader.Context.RegisterClassMap<InputRecordMap>();

            var inputRecords = inputCsvReader.GetRecords<InputRecord>().ToList();
            var processedIds = GetProcessedIds(processedCsvPath);

            return inputRecords.Where(x => !processedIds.Contains(x.Id)).ToList();
        }

        private static IList<string> GetProcessedIds(string? processedCsvPath)
        {
            if (!File.Exists(processedCsvPath))
                return new List<string>();

            using var outputStreamReader = new StreamReader(processedCsvPath!);
            using var outputCsvReader = new CsvReader(outputStreamReader, CsvConfiguration);
            outputCsvReader.Context.RegisterClassMap<ProcessedRecordMap>();

            var processedIds = outputCsvReader.GetRecords<ProcessedRecord>()
                .Select(x => x.Id);
            return processedIds.ToList();
        }
    }
}
