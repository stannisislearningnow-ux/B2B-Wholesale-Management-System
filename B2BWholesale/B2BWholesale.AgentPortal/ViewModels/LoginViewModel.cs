using System.ComponentModel.DataAnnotations;

namespace B2BWholesale.AgentPortal.ViewModels;

public sealed class LoginViewModel
{
    [Required, StringLength(60)]
    public string UserName { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
