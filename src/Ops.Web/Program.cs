using Ops.Web;
using Ops.Web.Ticketing;
using TicketingService.Proxy.HttpProxy;

var builder = WebApplication
    .CreateBuilder(args)
    .AddOptions<TicketingOptions>();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// add configuration
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

var ticketingOptions = builder.GetAppOptions<TicketingOptions>();

// builder.Services.AddSingleton<ITicketingApiProxy, FakeTicketingApiProxy>();
builder.Services.AddHttpProxyTicketing(ticketingOptions.TicketingServiceUrl);
builder.Services.AddHttpClient<ITicketingApiProxy, TicketingApiProxy>(c =>
{
    c.BaseAddress = new Uri(ticketingOptions.MonitoringUrl.TrimEnd('/'));
});

var app = builder.Build();

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
