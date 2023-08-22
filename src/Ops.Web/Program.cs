using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Ops.Web;
using Ops.Web.Jobs;
using Ops.Web.Ticketing;
using TicketingService.Proxy.HttpProxy;

var builder = WebApplication
    .CreateBuilder(args)
    .AddOptions<TicketingOptions>()
    .AddOptions<JobsOptions>()
    .AddOptions<AuthOptions>();

// Add configuration
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddHealthChecks()
    .AddCheck("Health", () => HealthCheckResult.Healthy());

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var ticketingOptions = builder.GetAppOptions<TicketingOptions>();
builder.Services.AddHttpProxyTicketing(ticketingOptions.TicketingServiceUrl);
builder.Services.AddHttpClient<ITicketingApiProxy, TicketingApiProxy>(c =>
{
    c.BaseAddress = new Uri(ticketingOptions.MonitoringUrl.TrimEnd('/'));
});

var jobsOptions = builder.GetAppOptions<JobsOptions>();
builder.Services.AddHttpClient<IJobsApiProxy, JobsApiProxy>(c =>
{
    c.BaseAddress = new Uri(jobsOptions.ApiUrl.TrimEnd('/'));
});

var app = builder.Build();
app.UseHealthChecks(new PathString("/health"), new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
