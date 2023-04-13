﻿using System.ComponentModel.DataAnnotations;
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

public class GetEventsRequest
{
    [Required] public string UserEmail { get; set; } = null!;

    public bool OldData { get; set; } = false;
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
    public string? UserEmail { get; set; } = null;
}

public class AddScoreToParticipantRequest
{
    [Required] public int Position { get; set; }
    [Required] public int Value { get; set; }
    [Required] public int ParticipantID { get; set; }
    [Required] public int TargetID { get; set; }
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