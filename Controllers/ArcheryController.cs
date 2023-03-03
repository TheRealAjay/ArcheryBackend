using ArcheryBackend.Archery;
using ArcheryBackend.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcheryBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ArcheryController : ControllerBase
{
    private readonly ILogger<ArcheryController> _logger;

    public ArcheryController(ILogger<ArcheryController> logger)
    {
        _logger = logger;
    }

    [HttpPut(Name = "CreateNewEvent"), Authorize]
    public ArcheryEvent CreateNewEvent(CreateEventRequest request)
    {
        var aEvent = new ArcheryEvent()
        {
            Name = request.Name,
            Street = request.Street,
            City = request.City,
            Zip = request.Zip,
            Date = DateOnly.Parse(request.Date),
            Time = TimeOnly.Parse(request.Time)
        };
        
        return aEvent;
    }
}