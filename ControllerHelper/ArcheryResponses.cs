using ArcheryBackend.Authentication;

namespace ArcheryBackend.ControllerHelper;

public class EventResponse
{
    public string EventName { get; set; } = null!;
    public string EventDesc { get; set; } = null!;
    public string FormattedDate { get; set; } = null!;
    public string FormattedTime { get; set; } = null!;
    public bool IsActiveEvent { get; set; } = false;
    public int EventID { get; set; }
}

public class EventsResponse
{
    public List<EventResponse> Events { get; set; }
}

public class TargetResponse
{
    public string TargetName { get; set; } = null!;
    public int TargetID { get; set; }
    public int EventID { get; set; }
}

public class ParticipantResponse
{
    public string NickName { get; set; } = null!;
    public bool IsNewParticipant { get; set; } = false;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int EventID { get; set; }
}

public class UserListResponse
{
    public Dictionary<string, string> Users { get; set; }
}

public class BooleanResponse
{
    public bool Boolean { get; set; }
}