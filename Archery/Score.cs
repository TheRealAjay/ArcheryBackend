using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Formats.Asn1;

namespace ArcheryBackend.Archery;

public class Score
{
    [Key] public int ID { get; set; }

    /** erster, zweiter oder dritter Pfeil */
    public int Position { get; set; }

    public int Value { get; set; }

    /** HasOne Relation to Participant */
    [ForeignKey(name: "Participant")]
    public int ParticipantID { get; set; }

    public Participant? Participant { get; set; }

    /** HasOne Relation to Target */
    [ForeignKey(name: "Target")]
    public int TargetID { get; set; }

    public Target? Target { get; set; }
}