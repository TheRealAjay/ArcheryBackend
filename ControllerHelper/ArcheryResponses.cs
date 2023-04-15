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

public class ScorePerArrowResponse
{
    public int TargetNumber { get; set; }
    public int Value { get; set; }
    public int MaxValue { get; set; }
}

public class IndividualScoreResponse
{
    public int UserID { get; set; }
    public int Place { get; set; }
    public int EventID { get; set; }
    public List<ScorePerArrowResponse> ArrowScores { get; set; }
}

public class ScorePerUserResponse
{
    public int UserID { get; set; }
    public string UserName { get; set; }
    public string Base64Picture { get; set; }
    public int Value { get; set; }
    public int Place { get; set; }
}

public class ScoresResponse
{
    public List<ScorePerUserResponse> Scores { get; set; }
}

public class ParticipantResponse
{
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