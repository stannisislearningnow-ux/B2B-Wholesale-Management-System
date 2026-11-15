using System.ComponentModel.DataAnnotations;

namespace B2BWholesale.AgentPortal.Models;

public sealed class AppUser
{
    public int UserID { get; set; }

    [Required, StringLength(60)]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool Lock { get; set; }
    public int FailedLoginAttempts { get; set; }

    [Required, StringLength(20)]
    public string Role { get; set; } = string.Empty;

    public int? AgentID { get; set; }
    public Agent? Agent { get; set; }
}
