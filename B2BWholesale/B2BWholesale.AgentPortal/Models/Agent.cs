using System.ComponentModel.DataAnnotations;

namespace B2BWholesale.AgentPortal.Models;

public sealed class Agent
{
    public int AgentID { get; set; }

    [Required, StringLength(120)]
    public string AgentName { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    public AppUser? User { get; set; }
    public ICollection<WholesaleOrder> Orders { get; set; } = new List<WholesaleOrder>();
}
