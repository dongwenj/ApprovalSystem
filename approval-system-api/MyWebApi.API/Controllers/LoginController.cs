using Microsoft.AspNetCore.Mvc;
using MyWebApi.Application.DTOs.Request;
using MyWebApi.Application.Interfaces;
using MyWebApi.Domain.Entities;

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
        
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody]Login_Req user)
    {
        var loginInfo = await _authService.LoginInfo(user);
        return Ok(loginInfo);
    }
}