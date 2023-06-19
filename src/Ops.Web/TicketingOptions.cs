namespace Ops.Web;

using System.ComponentModel.DataAnnotations;

public class TicketingOptions
{
    [Required]
    public string BaseUrl { get; set; }
}
