using System.ComponentModel.DataAnnotations;

namespace PersonnelManager.Web.Models;

public sealed class LoginViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
