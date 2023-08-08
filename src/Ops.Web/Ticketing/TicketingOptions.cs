namespace Ops.Web.Ticketing;

using System.ComponentModel.DataAnnotations;

public class TicketingOptions
{
    [Required]
    public string MonitoringUrl { get; set; }

    [Required]
    public string TicketingServiceUrl { get; set; }

    [Required]
    public string PublicApiTicketingUrl { get; set; }
}
