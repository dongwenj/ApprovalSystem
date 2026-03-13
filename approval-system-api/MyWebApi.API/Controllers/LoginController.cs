using Microsoft.AspNetCore.Mvc;
using MyWebApi.Application.DTOs.Request;
using MyWebApi.Application.Interfaces;

namespace MyWebApi.API.Controllers;

[ApiController]
[Route("api")]
public class LoginController : ControllerBase
{
    public readonly IAuthService _authService;

    public LoginController(IAuthService authService)
    {
        _authService = authService;
    }
        
    [HttpPost("Login")]
    public IActionResult Login([FromBody]Login_Req user)
    {
        var token = _authService.GenerateStandardToken(user);
        return Ok(new { token });
    }
}