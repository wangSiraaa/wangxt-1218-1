using JudicialEvidence.Api.Auth;
using JudicialEvidence.Api.Models.Dtos;
using JudicialEvidence.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JudicialEvidence.Api.Controllers;

[ApiController]
[Route("api/cases")]
[Authorize]
public class CasesController : ControllerBase
{
    private readonly ICaseService _cases;
    private readonly ICurrentUserService _current;

    public CasesController(ICaseService cases, ICurrentUserService current)
    {
        _cases = cases;
        _current = current;
    }

    [HttpGet]
    public async Task<ActionResult<List<CaseDto>>> List()
        => Ok(await _cases.ListAsync());

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CaseDto>> GetById(long id)
        => Ok(await _cases.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin,Police")]
    public async Task<ActionResult<CaseDto>> Create([FromBody] CaseCreateRequest request)
    {
        var userId = _current.UserId
            ?? throw new ServiceException("未识别到当前用户", 401);
        var created = await _cases.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
