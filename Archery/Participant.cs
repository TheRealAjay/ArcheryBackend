using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArcheryBackend.Authentication;
using ArcheryBackend.Contexts;

namespace ArcheryBackend.Archery;

public class Participant
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? NickName { get; set; }

    public ApplicationUser? ApplicationUser { get; set; }

    /** HasMany Relation to Event */
    public List<ArcheryEventParticipant> ArcheryEventParticipant { get; set; }

    /** HasMany Relation to Scores */
    public List<Score>? Scores { get; set; }

}