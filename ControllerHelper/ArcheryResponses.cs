using System.Collections.Specialized;

namespace ArcheryBackend.ControllerHelper;

public class EventResponse
{
    public string EventName { get; set; } = null!;
    public int EventID { get; set; }
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
    
    public int TargetID { get; set; }
    
    public int EventID { get; set; }
}

public class UserResponse
{
    public List<Dictionary<int, string>> UserList { get; set; }
}