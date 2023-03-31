using System.Text.Json;
using System.Text.Json.Serialization;
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

    #region EventSpecificEndpoitns

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
    [Route("addTarget")]
    public async Task<ActionResult<TargetResponse>> AddTargetToEvent(AddTargetToEventRequest request)
    {
        var managedEvent = await _context.Events.FindAsync(request.EventID);
        if (managedEvent == null)
        {
            return BadRequest("Event not found");
        }

        Target target = new Target()
        {
            Name = request.Name,
            Event = managedEvent,
        };

        _context.Targets.Attach(target);
        await _context.SaveChangesAsync();

        return Ok(new TargetResponse()
        {
            TargetName = target.Name ?? "",
            TargetID = target.ID,
            EventID = request.EventID
        });
    }

    [HttpPut, Authorize]
    [Route("addParticipant")]
    public async Task<ActionResult<ParticipantResponse>> AddParticipant(AddParticipantToEventRequest request)
    {
        var managedEvent = await _context.Events.FindAsync(request.EventID);

        if (managedEvent == null)
            return BadRequest("Event not found");

        var localNickName = request.Nickname;
        var localFirstName = request.FirstName;
        var localLastName = request.LastName;

        if (request.UserEmail != null)
        {
            var managedUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.UserEmail);
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
                FirstName = request.FirstName,
                LastName = request.LastName,
            };
            newParticipant = true;
            
        }
        
        managedParticipant.Events.Add(managedEvent);
        
        if (newParticipant)
        {
            _context.Participants.Add(managedParticipant);
        }
        else
        {
            _context.Participants.Update(managedParticipant);
        }
        
        await _context.SaveChangesAsync();

        return Ok(new ParticipantResponse()
        {
            NickName = managedParticipant.NickName ?? "",
            IsNewParticipant = newParticipant,
            FirstName = localFirstName,
            LastName = localLastName,
            EventID = managedEvent.ID
        });
    }

    #endregion

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
        var participants = (await _context.Events.FindAsync(request.EventID))?.Participants;

        Dictionary<string, string> returnUsers = new Dictionary<string, string>();

        if (participants != null)
        {
            foreach (var user in participants)
            {
                if (user.NickName != null && user.ApplicationUser != null)
                    returnUsers.Add(user.NickName, user.ApplicationUser.Id ?? "");
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