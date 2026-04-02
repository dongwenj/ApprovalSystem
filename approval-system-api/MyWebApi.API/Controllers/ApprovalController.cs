using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebApi.Application.DTOs.Request;
using MyWebApi.Application.DTOs.Respon;
using MyWebApi.Application.Services;

namespace MyWebApi.API.Controllers;

[Route("api/form")]
[ApiController]
public class ApprovalController : ControllerBase
{
    private readonly ApprovalService _mainService;

    public ApprovalController(ApprovalService mainService)
    {
        _mainService = mainService;
    }

    [HttpGet("query")]
    [Authorize]
    public async Task<IActionResult> ApplicationFormQuery([FromQuery] ApplicationFormQuery_Req req)
    {
        var result = await _mainService.ApplicationFormQuery(req);
        return Ok(result);
    }

    [HttpGet("view")]
    [Authorize]
    public async Task<IActionResult> ApplicationFormView([FromQuery] ApplicationFormView_Req model)
    {
        var result = await _mainService.ApplicationFormView(model);
        return Ok(result);
    }

    [HttpPost("add")]
    [Authorize]
    public async Task<IActionResult> ApplicationFormAdd([FromBody] ApplicationFormAdd_Req req)
    {
        var result = await _mainService.ApplicationFormAdd(req);
        return Ok(result);
    }

    [HttpPut("edit")]
    [Authorize]
    public async Task<IActionResult> ApplicationFormEdit([FromBody] ApplicationFormEdit_Req req)
    {
        var result = await _mainService.ApplicationFormEdit(req);
        return Ok(result);
    }

    [HttpDelete("delete")]
    [Authorize]
    public async Task<IActionResult> ApplicationFormDelete([FromQuery] ApplicationFormDelete_Req req)
    {
        var result = await _mainService.ApplicationFormDelete(req);
        return Ok(result);
    }

    [HttpPost("present")]
    [Authorize]
    public async Task<IActionResult> ApplicationFormPresent([FromBody] ApplicationFormSubmit_Req req)
    {
        var result = await _mainService.ApplicationFormPresent(req);
        return Ok(result);
    }

    [HttpPost("review")]
    [Authorize]
    public async Task<IActionResult> ApplicationFormReview([FromBody] ApplicationFormReview_Req req)
    {
        var result = await _mainService.ApplicationFormReview(req);
        return Ok(result);
    }

    [HttpPost("send")]
    [Authorize]
    public async Task<IActionResult> ApplicationFormSend([FromBody] ApplicationFormSend_Req req)
    {
        var result = await _mainService.ApplicationFormSend(req);
        return Ok(result);
    }
}
