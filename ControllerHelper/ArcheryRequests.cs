using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using ArcheryBackend.Archery;

namespace ArcheryBackend.Request;

public class CreateEventRequest
{
    [Required] public string Name { get; set; } = null!;
    [Required] public string Street { get; set; } = null!;
    [Required] public string Zip { get; set; } = null!;
    [Required] public string City { get; set; } = null!;
    [Required] public string Date { get; set; } = null!;
    [Required] public string Time { get; set; } = null!;
    [Required] public int ArrowValue { get; set; } = 0;
    [Required] public string UserEmail { get; set; } = null!;
}

public class GetEventsRequest
{
    [Required] public string UserEmail { get; set; } = null!;

    public bool OldData { get; set; } = false;
}

public class AddTargetToEventRequest
{
    [Required] public int EventID { get; set; }

    [Required] public Dictionary<int, string> Targets { get; set; }
}

public class ParticipantRequest
{
    [Required] public string FirstName { get; set; } = null!;
    [Required] public string LastName { get; set; } = null!;
    [Required] public string Nickname { get; set; } = null!;
    public string? UserEmail { get; set; } = null;
}

public class AddParticipantToEventRequest
{
    [Required] public int EventID { get; set; }

    public List<ParticipantRequest> Participants { get; set; }
}

public class GetUserInfoRequest
{
    public string Email { get; set; }
    public string Nickname { get; set; }
}

public class GetUserByEventAndNickRequest
{
    [Required] public string Name { get; set; }
    [Required] public int EventID { get; set; }
}

public class AddScore
{
    [Required] public string Nickname { get; set; }
    [Required] public int Value { get; set; }
    [Required] public int Position { get; set; }
}

public class AddScoresToTargetRequest
{
    [Required] public int TargetID { get; set; }
    public List<AddScore> Scores { get; set; }
}

public class GetScoresForEventRequest
{
    [Required] public int EventID { get; set; }
}

public class GetScoresForUserRequest
{
    [Required] public int EventID { get; set; }
    [Required] public string ParticipantName { get; set; }
}