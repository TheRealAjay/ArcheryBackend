﻿using ArcheryBackend.Archery;
using ArcheryBackend.Authentication;
using ArcheryBackend.Contexts;
using ArcheryBackend.ControllerHelper;
using ArcheryBackend.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

    //Vorlage für meine Funktion
    [HttpPut, Authorize]
    [Route("createEvent")]
    public async Task<ActionResult<EventResponse>> CreateNewEvent(CreateEventRequest request )//request.email
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



    /*
    //Anmerkungen
    //get userbyEmail (request request.email )
    //{
    // UserResponse return so:
//    return Ok(new EventResponse
//        {
//            EventName = aEvent.Name ?? "",
//            EventID = aEvent.ID
//});
//HIer muss die Liste drinnen stehen:  List<Dict> (Id, username)
Request und response schreiben. Das sind zwei Klassen die ich selbst aufbauen muss.
    in ControllerHelper sind HelperKlassen 
    ArcheryResponse
    ArcheryRequest
    kann ich als Vorlage nehmen.
    Vorlage:
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
    Bei userByEmailRequest bekomme ich eine E-Mail zurück.

    public class UserByEmailRequest
    {
      [Required] public string email { get; set; } = null!;
    }
    


    //} 

    Test

    kein eventresponse
    kein tablersponse

    [HttpPut, Authorize]
    [Route("createEvent")]
    public async Task<ActionResult<EventResponse>> CreateNewEvent(CreateEventRequest request )//request.email
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


    */





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
}