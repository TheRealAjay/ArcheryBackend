using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ArcheryBackend.Authentication;

public class ApplicationUser : IdentityUser
{
    [PersonalData] public string? FirstName { get; set; }
    [PersonalData] public string? LastName { get; set; }

    [PersonalData] public int UsernameChangeLimit { get; set; } = 10;

    [PersonalData] public byte[]? ProfilePicture { get; set; }
}