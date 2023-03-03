using System.ComponentModel.DataAnnotations;

namespace ArcheryBackend.Request;

public class RegistrationRequest
{
    [Required] public string Email { get; set; } = null!;
    [Required] public string Username { get; set; } = null!;
    [Required] public string Password { get; set; } = null!;
    [Required] public string FirstName { get; set; } = null!;
    [Required] public string LastName { get; set; } = null!;
}