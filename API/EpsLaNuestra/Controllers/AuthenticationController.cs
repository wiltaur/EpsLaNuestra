using EpsLaNuestra.Application.Features.Authentication.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EpsLaNuestra.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(IMediator mediator) : ControllerBase
{
    #region MainMethods

    /// <summary>
    /// Get token to consume the APIs.
    /// </summary>
    /// <param name="userId">For authenticate the user.</param>
    /// <returns> One token.</returns>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string userId)
    {
        var command = new GetTokenAuthenticateQuery(userId);
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    #endregion MainMethods
}