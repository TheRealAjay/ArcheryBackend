using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArcheryBackend.Archery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ArcheryBackend.Authentication;

public class ApplicationUser : IdentityUser
{
    [PersonalData] public string? FirstName { get; set; }
    
    [PersonalData] public string? LastName { get; set; }
    
    public int UsernameChangeLimit { get; set; } = 10;
    
    public byte[]? ProfilePicture { get; set; }
    
    public List<Archery.ArcheryEvent>? Events { get; set; }
}