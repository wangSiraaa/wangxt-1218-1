using JudicialEvidence.Api.Models.Dtos;
using JudicialEvidence.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JudicialEvidence.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;
    public AdminController(IAdminService admin) => _admin = admin;

    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> ListUsers()
        => Ok(await _admin.ListUsersAsync());

    [HttpPost("users")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserCreateRequest request)
    {
        var user = await _admin.CreateUserAsync(request);
        return Ok(user);
    }
}
