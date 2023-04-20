using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using ArcheryBackend.Archery;
using ArcheryBackend.Authentication;
using ArcheryBackend.Contexts;
using ArcheryBackend.ControllerHelper;
using ArcheryBackend.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcheryBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ArcheryController : Controller
{
    private readonly ILogger<ArcheryController> _logger;
    private readonly ArcheryContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;

    public ArcheryController(ILogger<ArcheryController> logger, ArcheryContext context,
        UserManager<ApplicationUser> userManager, TokenService tokenService)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPut, Authorize]
    [Route("createEvent")]
    public async Task<ActionResult<EventResponse>> CreateNewEvent(CreateEventRequest request)
    {
        var managedUser = await _userManager.FindByEmailAsync(request.UserEmail);
        if (managedUser == null)
        {
            return BadRequest("User not found");
        }

        ArcheryEvent aEvent = new ArcheryEvent()
        {
            Name = request.Name,
            Street = request.Street,
            City = request.City,
            Zip = request.Zip,
            Date = DateOnly.Parse(request.Date),
            Time = TimeOnly.Parse(request.Time),
            User = managedUser,
            ArrowValue = request.ArrowValue,
            ScoringType = request.ScoringType,
            // Participants = new List<Participant>(),
            // Targets = new List<Target>(),
        };

        _context.Events.Attach(aEvent);
        await _context.SaveChangesAsync();

        return Ok(new EventResponse
        {
            EventName = aEvent.Name ?? "",
            EventID = aEvent.ID
        });
    }

    [HttpPut, Authorize]
    [Route("addTargets")]
    public async Task<ActionResult<BooleanResponse>> AddTargetsToEvent(AddTargetToEventRequest request)
    {
        var managedEvent = await _context.Events.FindAsync(request.EventID);
        if (managedEvent == null)
        {
            return BadRequest("Event not found");
        }

        foreach (var t in request.Targets.OrderBy(t => t.Key))
        {
            Target target = new Target()
            {
                Name = t.Value,
                EventID = request.EventID,
            };

            _context.Targets.Attach(target);
            await _context.SaveChangesAsync();
        }

        return Ok(new BooleanResponse()
        {
            Boolean = true
        });
    }

    [HttpPut, Authorize]
    [Route("addParticipant")]
    public async Task<ActionResult<ParticipantResponse>> AddParticipant(AddParticipantToEventRequest request)
    {
        var managedEvent = await _context.Events.FindAsync(request.EventID);

        if (managedEvent == null)
            return BadRequest("Event not found");

        foreach (var participant in request.Participants)
        {
            var localNickName = participant.Nickname;
            var localFirstName = participant.FirstName;
            var localLastName = participant.LastName;
            string? userId = null;

            if (participant.UserEmail != null)
            {
                var managedUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == participant.UserEmail);
                if (managedUser != null)
                {
                    localNickName = managedUser.UserName;
                    userId = managedUser.Id;
                }
            }

            var managedParticipant = await _context.Participants.SingleOrDefaultAsync(p => p.NickName == localNickName);
            bool newParticipant = false;

            if (managedParticipant == null)
            {
                managedParticipant = new Participant()
                {
                    NickName = localNickName,
                    FirstName = localFirstName,
                    LastName = localLastName,
                    UserID = userId
                };
                newParticipant = true;
            }


            if (newParticipant)
            {
                _context.Participants.Attach(managedParticipant);
            }
            else
            {
                _context.Participants.Update(managedParticipant);
            }

            await _context.SaveChangesAsync();

            var dbEvent = await _context.Events.FindAsync(request.EventID);
            var dbParticipant = await _context.Participants.SingleOrDefaultAsync(p => p.NickName == localNickName);
            var connection = new ArcheryEventParticipant()
            {
                EventID = dbEvent!.ID,
                Event = dbEvent,
                ParticipantID = dbParticipant!.ID,
                Participant = dbParticipant
            };
            _context.ArcheryEventParticipant.Add(connection);

            await _context.SaveChangesAsync();
        }


        return Ok(
            new ParticipantResponse()
            {
                EventID = managedEvent.ID
            }
        );
    }

    [HttpPut, Authorize]
    [Route("addScores")]
    public async Task<ActionResult<BooleanResponse>> AddScores(AddScoresToTargetRequest request)
    {
        foreach (var addScore in request.Scores)
        {
            var managedParticipant =
                await _context.Participants.SingleOrDefaultAsync(p => p.NickName == addScore.Nickname);

            if (managedParticipant == null)
            {
                continue;
            }

            Score score = new Score()
            {
                ParticipantID = managedParticipant.ID,
                TargetID = request.TargetID,
                Value = addScore.Value,
                Position = addScore.Position
            };

            _context.Scores.Attach(score);
            await _context.SaveChangesAsync();
        }

        return Ok(new BooleanResponse()
        {
            Boolean = true
        });
    }

    [HttpPost, Authorize]
    [Route("getTargets")]
    public async Task<ActionResult<TargetResponse>> GetTargets(GetScoresForEventRequest request)
    {
        var managedEvent = await _context.Events.SingleOrDefaultAsync(e => e.ID == request.EventID);
        if (managedEvent == null)
        {
            return BadRequest("No Event found");
        }

        var targetList = new List<TargetInformation>();

        var targets = _context.Targets.Where(t => t.EventID == managedEvent.ID).OrderBy(t => t.ID).ToList();

        int i = 1;
        foreach (var target in targets)
        {
            var scores = _context.Scores.Any(s => s.TargetID == target.ID);
            targetList.Add(new TargetInformation()
            {
                TargetID = target.ID,
                TargetName = target.Name ?? "NaN",
                TargetPos = i,
                HasScores = scores
            });
            i++;
        }

        return Ok(new TargetResponse()
        {
            Targets = targetList
        });
    }

    private static string ConvertToBase64(Stream stream)
    {
        byte[] bytes;
        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            bytes = memoryStream.ToArray();
        }

        string base64 = Convert.ToBase64String(bytes);
        return base64;
    }

    private string getProfilePicture(string firstname, string lastname)
    {
        string base64String = "";

        using (HttpClient client = new HttpClient())
        {
            var name = firstname + " " + lastname;
            var random = new Random();
            string requestURI = "https://source.boringavatars.com/beam/120/" + name;

            var stream = client.GetStreamAsync(requestURI);
            base64String = ConvertToBase64(stream.Result);
        }

        return base64String;
    }

    [HttpPost, Authorize]
    [Route("getScoresForEvent")]
    public async Task<ActionResult<ScoresResponse>> GetScoresForEvent(GetScoresForEventRequest request)
    {
        var managedEvent = await _context.Events.SingleOrDefaultAsync(e => e.ID == request.EventID);
        if (managedEvent == null)
        {
            return BadRequest("No Event found");
        }

        var responses = new List<ScorePerUserResponse>();

        var connections = _context.ArcheryEventParticipant.Where(aep => aep.EventID == managedEvent.ID).ToList();

        foreach (var connection in connections)
        {
            var participantScores = _context.Scores.Where(s =>
                s.ParticipantID == connection.ParticipantID && s.Target!.EventID == managedEvent.ID).ToList();

            var allScore = participantScores.Sum(s => s.Value);
            var participant = await _context.Participants.FindAsync(connection.ParticipantID);
            var response = new ScorePerUserResponse()
            {
                UserID = connection.ParticipantID,
                UserName = connection.Participant.NickName ?? "default",
                Value = allScore,
                Base64Picture =
                    "data:image/svg+xml;base64," +
                    (
                        connection.Participant.User?.Base64Picture ??
                        getProfilePicture(connection.Participant.FirstName!,
                            connection.Participant.LastName!)
                    )
            };
            responses.Add(response);
        }

        int i = 1;
        foreach (var response in responses.OrderByDescending(r => r.Value))
        {
            response.Place = i;
            i++;
        }

        return Ok(new ScoresResponse()
        {
            Scores = responses.OrderByDescending(r => r.Value).ToList()
        });
    }

    private int GetPlayerPosition(ArcheryEvent managedEvent, int participantId)
    {
        var responses = new Dictionary<int, int>();

        foreach (var eventParticipant in _context.ArcheryEventParticipant
                     .Where(aep => aep.EventID == managedEvent.ID).ToList())
        {
            var scoresForTarget = _context.Scores.Where(
                s => s.ParticipantID == eventParticipant.ParticipantID &&
                     s.Target.EventID == managedEvent.ID).ToList();

            var allScore = scoresForTarget.Sum(s => s.Value);
            responses.Add(eventParticipant.ParticipantID, allScore);
        }

        int i = 1;
        foreach (var response in responses.Distinct().OrderByDescending(r => r.Value).ToList())
        {
            if (response.Key == participantId) return i;
            i++;
        }

        return i;
    }

    [HttpPost, Authorize]
    [Route("getScoresForUser")]
    public async Task<ActionResult<IndividualScoreResponse>> GetScoresForUser(GetScoresForUserRequest request)
    {
        var managedEvent = await _context.Events.SingleOrDefaultAsync(e => e.ID == request.EventID);
        if (managedEvent == null)
        {
            return BadRequest("No Event found");
        }

        var participant = await _context.Participants.SingleOrDefaultAsync(p => p.NickName == request.ParticipantName);
        if (participant == null)
        {
            return BadRequest("No Participant found");
        }

        var position = GetPlayerPosition(managedEvent, participant.ID);
        var result = new IndividualScoreResponse()
        {
            Place = position,
            EventID = managedEvent.ID,
            UserID = participant.ID,
            ArrowScores = new List<ScorePerArrowResponse>()
        };

        var targets = _context.Targets.Where(t => t.EventID == managedEvent.ID).OrderBy(t => t.ID).ToList();

        int i = 1;
        foreach (var target in targets)
        {
            var scoreForTarget = await _context.Scores.FirstOrDefaultAsync(s =>
                s.TargetID == target.ID && s.Participant.NickName == request.ParticipantName);

            if (scoreForTarget == null) continue;

            var maxValue = 20; //TODO: maybe change this to pfeilwertung idk man I don't care

            result.ArrowScores.Add(new ScorePerArrowResponse()
            {
                Value = scoreForTarget.Value,
                TargetNumber = i,
                MaxValue = maxValue,
            });
            i++;
        }

        return Ok(result);
    }

    [HttpPost, Authorize]
    [Route("getParticipants")]
    public async Task<ActionResult<ParticipantsResponse>> GetParticipants(GetScoresForEventRequest request)
    {
        var managedEvent = await _context.Events.SingleOrDefaultAsync(e => e.ID == request.EventID);
        if (managedEvent == null)
        {
            return BadRequest("No Event found");
        }

        var participants = _context.ArcheryEventParticipant.Where(aep => aep.EventID == managedEvent.ID).ToList();

        List<ParticipantObj> list = new List<ParticipantObj>();

        foreach (var participantJoin in participants)
        {
            var participant =
                await _context.Participants.SingleOrDefaultAsync(p => p.ID == participantJoin.ParticipantID);

            list.Add(new ParticipantObj()
            {
                FirstName = participant.FirstName,
                LastName = participant.LastName,
                NickName = participant.NickName
            });
        }

        return Ok(new ParticipantsResponse()
        {
            Participants = list
        });
    }

    [HttpPost, Authorize]
    [Route("getEventInfo")]
    public async Task<ActionResult<EventResponse>> getEventInfo(GetScoresForEventRequest request)
    {
        var managedEvent = await _context.Events.SingleOrDefaultAsync(e => e.ID == request.EventID);
        if (managedEvent == null)
        {
            return BadRequest("No Event found");
        }

        return Ok(new EventResponse()
        {
            EventID = managedEvent.ID,
            EventName = managedEvent.Name,
            EventAddress = managedEvent.Zip + " " + managedEvent.City + ", " + managedEvent.Street,
            FormattedDate = managedEvent.Date.Day + "." + managedEvent.Date.Month + "." + managedEvent.Date.Year,
            FormattedTime = managedEvent.Time.ToString(),
            ScoringType = managedEvent.ScoringType,
        });
    }

    [HttpPost, Authorize]
    [Route("getAllEvents")]
    public async Task<ActionResult<EventsResponse>> GetUpcomingEvents(GetEventsRequest request)
    {
        var managedUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.UserEmail);
        if (managedUser == null)
        {
            return BadRequest("User not found");
        }

        int dayNumber = DateOnly.FromDateTime(DateTime.Now).DayNumber;
        var events = _context.Events.Where((e) => e.User.Id == managedUser.Id).ToList();

        if (request.OldData)
        {
            var managedParticipant = await _context.Participants.SingleOrDefaultAsync(p => p.UserID == managedUser.Id);
            if (managedParticipant != null)
            {
                var managedEventParticipant =
                    _context.ArcheryEventParticipant.Where(aep => aep.ParticipantID == managedParticipant.ID).ToList();
                foreach (var mep in managedEventParticipant)
                {
                    var guestEvent = await _context.Events.SingleOrDefaultAsync(e => e.ID == mep.EventID);
                    if (guestEvent != null)
                    {
                        events.Add(guestEvent);
                    }
                }
            }

            events = events.OrderByDescending(e => e.Date).ToList();
        }
        else
        {
            events = events.OrderBy(e => e.Date).ToList();
        }

        var list = new List<EventResponse>();

        foreach (var ev in events.Distinct().ToList())
        {
            if (ev.Date.DayNumber < dayNumber && request.OldData == false)
                continue;

            if (ev.isFinished && request.OldData == false)
                continue;

            if (ev.Date.DayNumber >= dayNumber && request.OldData && ev.isFinished == false)
                continue;

            EventResponse listItem = new EventResponse
            {
                EventID = ev.ID,
                IsActiveEvent = ev.Date.DayNumber == dayNumber,
                EventName = ev.Name ?? "NaN",
                EventAddress = ev.Zip + " " + ev.City + ", " + ev.Street,
                FormattedDate = ev.Date.Day + "." + ev.Date.Month + "." + ev.Date.Year,
                FormattedTime = ev.Time.ToString()
            };

            list.Add(listItem);
        }

        return Ok(new EventsResponse()
        {
            Events = list
        });
    }

    [HttpPost, Authorize]
    [Route("getUsersByEmail")]
    public async Task<ActionResult<UserListResponse>> GetUsersByEmail(GetUserInfoRequest request)
    {
        // var users = from u in _context.Users where u.Email.Contains(request.Email) select u;

        var users = _context.Users.Where(u => u.Email!.Contains(request.Email));
        List<UserDataResponse> returnUsers = new List<UserDataResponse>();

        foreach (var user in users)
        {
            returnUsers.Add(new UserDataResponse()
            {
                NickName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Base64Img = "data:image/svg+xml;base64," + user.Base64Picture,
                UserEmail = user.Email ?? "",
            });
        }

        return Ok(new UserListResponse()
        {
            Users = returnUsers
        });
    }

    [HttpPost, Authorize]
    [Route("finishEvent")]
    public async Task<ActionResult<BooleanResponse>> FinishEvent(ParticipantResponse request)
    {
        var managedEvent = await _context.Events.SingleOrDefaultAsync(e => e.ID == request.EventID);
        if (managedEvent == null)
        {
            return BadRequest("No Event found");
        }

        managedEvent.isFinished = true;
        await _context.SaveChangesAsync();

        return Ok(new BooleanResponse()
        {
            Boolean = true
        });
    }

    [HttpPost, Authorize]
    [Route("getUserData")]
    public async Task<ActionResult<UserDataResponse>> GetUserData(GetEventsRequest request)
    {
        var managedUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.UserEmail);
        if (managedUser == null)
        {
            return BadRequest("User not found");
        }

        return Ok(new UserDataResponse()
        {
            FirstName = managedUser.FirstName,
            LastName = managedUser.LastName,
            NickName = managedUser.UserName,
            Base64Img = "data:image/svg+xml;base64," + managedUser.Base64Picture,
            UserEmail = managedUser.Email ?? "",
        });
    }

    [HttpPost, Authorize]
    [Route("getParticipantData")]
    public async Task<ActionResult<ParticipantDataResponse>> GetParticipantData(GetParticipantDataRequest request)
    {
        var managedParticipant = await _context.Participants.FindAsync(request.ParticipantID);
        if (managedParticipant == null)
        {
            return BadRequest("User not found");
        }

        var managedUser = await _context.Users.SingleOrDefaultAsync(u => u.Id == managedParticipant.UserID);

        return Ok(new ParticipantDataResponse()
        {
            FirstName = managedParticipant.FirstName,
            LastName = managedParticipant.LastName,
            UserEmail = managedUser?.Email ?? null,
        });
    }

    [HttpPost]
    [Route("getUserInfo")]
    public async Task<ActionResult<BooleanResponse>> GetIfUserExists(GetUserInfoRequest request)
    {
        bool returnVal = false;
        var user = new IdentityUser();

        if (request.Email != "")
        {
            user = _context.Users.SingleOrDefault(u => u.Email == request.Email);
        }

        if (request.Nickname != "")
        {
            user = _context.Users.SingleOrDefault(u => u.UserName == request.Nickname);
        }

        if (user != null)
        {
            returnVal = true;
        }

        return Ok(new BooleanResponse()
        {
            Boolean = returnVal
        });
    }
}