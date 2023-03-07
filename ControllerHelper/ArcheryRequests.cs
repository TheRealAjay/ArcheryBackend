using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace ArcheryBackend.Request;

public class CreateEventRequest
{
    [Required] public string Name { get; set; } = null!;
    [Required] public string Street { get; set; } = null!;
    [Required] public string Zip { get; set; } = null!;
    [Required] public string City { get; set; } = null!;
    [Required] public string Date { get; set; } = null!;
    [Required] public string Time { get; set; } = null!;
    [Required] public string UserEmail { get; set; } = null!;
}

public class AddTargetToEventRequest
{
    [Required] public string Name { get; set; } = null!;
    [Required] public int EventID { get; set; }
}

public class AddParticipantToEventRequest
{
    [Required] public string FirstName { get; set; } = null!;
    [Required] public string LastName { get; set; } = null!;
    [Required] public string Nickname { get; set; } = null!;
    [Required] public int EventID { get; set; }
    public string UserEmail { get; set; } = null!;
}

public class AddScoreToParticipant
{
    [Required] public int Position { get; set; }
    [Required] public int Value { get; set; }
    [Required] public int ParticipantID { get; set; }
    [Required] public int TargetID { get; set; }
}