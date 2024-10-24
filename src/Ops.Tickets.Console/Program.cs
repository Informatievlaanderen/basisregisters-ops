namespace Ops.Tickets.Console
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
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

            var inputCsvPath = configuration.GetValue<string>("InputCsvPath");
            var outputCsvFolder = configuration.GetValue<string>("OutputCsvFolder");
            var apiKey = configuration.GetValue<string>("ApiKey");

            var tickets = GetTicketsToProcess(inputCsvPath);

            var cts = new CancellationTokenSource();
            var client = new HttpClient();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            }

            var completedTickets = new ConcurrentBag<TicketRecord>();
            var errorTickets = new ConcurrentBag<TicketErrorRecord>();
            var countProcessed = 0;
            try
            {
                await Parallel.ForEachAsync(
                    tickets,
                    new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = cts.Token },
                    async (ticket, token) =>
                    {
                        if (token.IsCancellationRequested)
                            return;

                        Interlocked.Increment(ref countProcessed);
                        Console.WriteLine($"Processing ticket: {ticket.TicketUrl} ({Math.Round((double)countProcessed / tickets.Count * 100, 2)}%)");

                        try
                        {
                            var done = false;
                            while (!done)
                            {
                                try
                                {
                                    var response = await client.GetFromJsonAsync<TicketResponse>(ticket.TicketUrl, token);

                                    done = response!.Status.Equals("complete") || response.Status.Equals("error");
                                    if (!done)
                                    {
                                        Console.WriteLine($"Ticket has status {response.Status}. Waiting 2 seconds.");
                                        await Task.Delay(TimeSpan.FromSeconds(2), token);
                                        continue;
                                    }

                                    if (response.Status.Equals("complete"))
                                    {
                                        completedTickets.Add(ticket);
                                    }

                                    if (response.Status.Equals("error"))
                                    {
                                        errorTickets.Add(new TicketErrorRecord(ticket.Id, ticket.TicketUrl, response.Result.Json));
                                    }
                                }
                                catch (WebException ex)
                                {
                                    if (ex.Response is HttpWebResponse response
                                        && response.StatusCode == HttpStatusCode.TooManyRequests)
                                    {
                                        await Task.Delay(50);
                                        continue;
                                    }

                                    throw;
                                }

                                done = true;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Failed to process id: {ticket.TicketUrl}");
                            Console.WriteLine(e);
                            await cts.CancelAsync();
                            throw;
                        }
                    });
            }
            finally
            {
                Directory.CreateDirectory(outputCsvFolder);

                var completedPath = Path.Join(outputCsvFolder, "completed.csv");
                if (File.Exists(completedPath))
                {
                    File.Delete(completedPath);
                }

                var errorPath = Path.Join(outputCsvFolder, "error.csv");
                if (File.Exists(errorPath))
                {
                    File.Delete(errorPath);
                }

                var completedTicketFileStream = new FileStream(completedPath, FileMode.CreateNew);
                await using var completedTicketStreamWriter = new StreamWriter(completedTicketFileStream);
                await using var completedTicketCsvWriter = new CsvWriter(completedTicketStreamWriter, CsvConfiguration);

                completedTicketCsvWriter.Context.RegisterClassMap<TicketRecordMap>();
                await completedTicketCsvWriter.WriteRecordsAsync(completedTickets, CancellationToken.None);
                await completedTicketCsvWriter.FlushAsync();

                Console.WriteLine($"Completed tickets: {completedTickets.Count}");
                Console.WriteLine($"Error tickets: {errorTickets.Count}");

                if (errorTickets.Any())
                {
                    var errorTicketFileStream = new FileStream(errorPath, FileMode.CreateNew);
                    await using var errorTicketStreamWriter = new StreamWriter(errorTicketFileStream);
                    await using var errorTicketCsvWriter = new CsvWriter(errorTicketStreamWriter, CsvConfiguration);

                    errorTicketCsvWriter.Context.RegisterClassMap<TicketErrorRecordMap>();
                    await errorTicketCsvWriter.WriteRecordsAsync(errorTickets, CancellationToken.None);
                    await errorTicketCsvWriter.FlushAsync();
                }
            }

            Console.WriteLine("Processing complete. Press any key to exit.");
            Console.Read();
        }

        private static List<TicketRecord> GetTicketsToProcess(
            string? inputCsvPath)
        {
            using var inputStreamReader = new StreamReader(inputCsvPath!);
            using var inputCsvReader = new CsvReader(inputStreamReader, CsvConfiguration);
            inputCsvReader.Context.RegisterClassMap<TicketRecordMap>();

            return inputCsvReader.GetRecords<TicketRecord>().ToList();
        }
    }
}
