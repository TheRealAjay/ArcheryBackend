﻿using System.Net;
using System.Text;
using ArcheryBackend.Authentication;
using ArcheryBackend.Contexts;
using ArcheryBackend.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArcheryBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ArcheryContext _context;
    private readonly TokenService _tokenService;

    public AuthController(UserManager<ApplicationUser> userManager, ArcheryContext context, TokenService tokenService)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
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

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        string base64String = "";
        using (HttpClient client = new HttpClient())
        {
            var name = request.FirstName + " " + request.LastName;
            var random = new Random();
            var color1 = String.Format("#{0:X6}", random.Next(0x1000000)); // = "#A197B9";
            var color2 = String.Format("#{0:X6}", random.Next(0x1000000));
            var color3 = String.Format("#{0:X6}", random.Next(0x1000000));
            string requestURI = "https://source.boringavatars.com/beam/120/" + name + "?colors=" + color1 + "," +
                                color2 + "," + color3 + "";

            var stream = client.GetStreamAsync(requestURI);
            base64String = ConvertToBase64(stream.Result);
        }

        var result = await _userManager.CreateAsync(
            new ApplicationUser()
            {
                UserName = request.Username, Email = request.Email, FirstName = request.FirstName,
                LastName = request.LastName, Base64Picture = base64String,
            },
            request.Password
        );

        if (result.Succeeded)
        {
            request.Password = "";
            return CreatedAtAction(nameof(Register), new { email = request.Email }, request);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return BadRequest(ModelState);
    }

    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var managedUser = await _userManager.FindByEmailAsync(request.Email);
        if (managedUser == null)
        {
            return BadRequest("Bad credentials");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password);
        if (!isPasswordValid)
        {
            return BadRequest("Bad credentials");
        }

        var userInDb = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (userInDb is null)
            return Unauthorized();
        var accessToken = _tokenService.CreateToken(userInDb);
        await _context.SaveChangesAsync();
        return Ok(new AuthResponse
        {
            Username = userInDb.UserName ?? "",
            Email = userInDb.Email ?? "",
            Base64String = userInDb.Base64Picture,
            Token = accessToken,
        });
    }
}