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
            ArrowValue = request.ArrowValue
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

        foreach (var t in request.Targets)
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

            if (participant.UserEmail != null)
            {
                var managedUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == participant.UserEmail);
                if (managedUser != null)
                    localNickName = managedUser.UserName;
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
                    ApplicationUser = null
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
            var color1 = String.Format("#{0:X6}", random.Next(0x1000000)); // = "#A197B9";
            var color2 = String.Format("#{0:X6}", random.Next(0x1000000));
            var color3 = String.Format("#{0:X6}", random.Next(0x1000000));
            string requestURI = "https://source.boringavatars.com/beam/120/" + name + "?colors=" + color1 + "," +
                                color2 + "," + color3 + "";

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

        foreach (var participant in managedEvent.ArcheryEventParticipant)
        {
            var scoresForTarget = _context.Scores.Where(s =>
                s.ParticipantID == participant.ID && s.Target!.EventID == managedEvent.ID);

            var participantScores = scoresForTarget.Where(s => s.ParticipantID == participant.ID);
            var allScore = participantScores.Sum(s => s.Value);
            Console.WriteLine(participant.Participant.NickName);
            Console.WriteLine(allScore);
            Console.WriteLine("------");
            var response = new ScorePerUserResponse()
            {
                UserID = participant.ID,
                UserName = participant.Participant.NickName ?? "default",
                Value = allScore,
                Base64Picture = participant.Participant.ApplicationUser?.Base64Picture ??
                                getProfilePicture(participant.Participant.FirstName!,
                                    participant.Participant.LastName!),
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
            Scores = responses
        });
    }

    private int getPlayerPosition(ArcheryEvent managedEvent, int participantID)
    {
        var responses = new Dictionary<int, int>();


        foreach (var participant in managedEvent.ArcheryEventParticipant)
        {
            var scoresForTarget = _context.Scores.Where(s =>
                s.ParticipantID == participant.ID && s.Target.EventID == managedEvent.ID);

            var participantScores = scoresForTarget.Where(s => s.ParticipantID == participant.ID);
            var allScore = participantScores.Sum(s => s.Value);
            responses.Add(participant.ID, allScore);
        }

        int i = 1;
        foreach (var response in responses.OrderByDescending(r => r.Value))
        {
            if (response.Key == participantID) return i;
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

        var result = new IndividualScoreResponse();

        int i = 1;
        foreach (var target in _context.Targets.Where(t => t.EventID == managedEvent.ID).OrderBy(t => t.ID))
        {
            var scoreForTarget = _context.Scores.SingleOrDefaultAsync(s =>
                s.TargetID == target.ID && s.Participant.NickName == request.ParticipantName);

            if (scoreForTarget.Result == null) continue;

            var maxValue = 20; //TODO: maybe change this to pfeilwertung idk man I don't care
            var arrowScore = new ScorePerArrowResponse()
            {
                Value = scoreForTarget.Result.Value,
                TargetNumber = i,
                MaxValue = maxValue,
            };
            i++;

            result.ArrowScores.Add(arrowScore);
        }

        var position = getPlayerPosition(managedEvent, participant.ID);

        result.Place = position;
        result.EventID = managedEvent.ID;
        result.UserID = participant.ID;

        return Ok(result);
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
        var events = _context.Events.Where((e) => e.User.Id == managedUser.Id).OrderBy(e => e.Date);
        var list = new List<EventResponse>();

        foreach (var ev in events)
        {
            if (ev.Date.DayNumber < dayNumber && request.OldData == false)
                continue;

            if (ev.Date.DayNumber >= dayNumber && request.OldData)
                continue;

            EventResponse listItem = new EventResponse
            {
                EventID = ev.ID,
                IsActiveEvent = ev.Date.DayNumber == dayNumber,
                EventName = ev.Name,
                EventDesc = "None",
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
        var users = from u in _context.Users where u.Email.Contains(request.Email) select u;

        Dictionary<string, string> returnUsers = new Dictionary<string, string>();

        foreach (var user in users)
        {
            returnUsers.Add(user.Id, user.UserName ?? "");
        }

        return Ok(new UserListResponse()
        {
            Users = returnUsers
        });
    }

    [HttpPost, Authorize]
    [Route("getUsersByName")]
    public async Task<ActionResult<UserListResponse>> GetUsersByName(GetUserByEventAndNickRequest request)
    {
        var participants = (await _context.Events.FindAsync(request.EventID))?.ArcheryEventParticipant;

        Dictionary<string, string> returnUsers = new Dictionary<string, string>();

        if (participants != null)
        {
            foreach (var user in participants)
            {
                if (user.ParticipantID != null && user.Participant.ApplicationUser != null)
                    returnUsers.Add(user.Participant.NickName, user.Participant.ApplicationUser.Id ?? "");
            }
        }


        return Ok(new UserListResponse()
        {
            Users = returnUsers
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