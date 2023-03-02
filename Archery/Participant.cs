using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArcheryBackend.Authentication;

namespace ArcheryBackend.Archery;

public class Participant
{
    [Key]
    public int ID { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? NickName { get; set; }

    public ApplicationUser? ApplicationUser { get; set; }

    /** HasOne Relation to Event */
    [ForeignKey(name: "ArcheryEvent")]
    public int EventID { get; set; }

    public ArcheryEvent? Event { get; set; }

    /** HasMany Relation to Scores */
    public List<Score>? Scores { get; set; }
}