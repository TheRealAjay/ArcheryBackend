using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcheryBackend.Archery;

public class Target
{
    [Key]
    public int ID { get; set; }

    public string? Name { get; set; }

    /** HasOne Relation to Event */
    [ForeignKey(name: "ArcheryEvent")]
    public int EventID { get; set; }

    public ArcheryEvent? Event { get; set; }
}