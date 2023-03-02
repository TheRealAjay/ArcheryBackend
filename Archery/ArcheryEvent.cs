using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using ArcheryBackend.Authentication;
using Microsoft.AspNetCore.Identity;

namespace ArcheryBackend.Archery;

public class ArcheryEvent
{
    [Key]
    public int ID { get; set; }
    
    public string? Name { get; set; }

    public string? Street { get; set; }

    public string? Zip { get; set; }

    public string? City { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly Time { get; set; }

    // /** HasOne Relation to User */
    // [ForeignKey(name: "ApplicationUser")]
    // public int UserID { get; set; }
    
    public ApplicationUser? User { get; set; }

    /** HasMany Relation to Targets */
    public List<Target>? Targets { get; set; }

    /** HasMany Relation to Participants */
    public List<Participant>? Participants { get; set; }
}