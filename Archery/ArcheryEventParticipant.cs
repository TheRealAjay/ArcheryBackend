using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcheryBackend.Archery;

public class ArcheryEventParticipant
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    public int EventID { get; set; }
    public ArcheryEvent Event { get; set; }

    public int ParticipantID { get; set; }
    public Participant Participant { get; set; }
}