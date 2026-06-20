using JudicialEvidence.Api.Auth;
using JudicialEvidence.Api.Models.Dtos;
using JudicialEvidence.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JudicialEvidence.Api.Controllers;

[ApiController]
[Route("api/retrieval")]
[Authorize]
public class RetrievalController : ControllerBase
{
    private readonly IRetrievalService _retrieval;
    private readonly ICurrentUserService _current;

    public RetrievalController(IRetrievalService retrieval, ICurrentUserService current)
    {
        _retrieval = retrieval;
        _current = current;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Clerk")]
    public async Task<ActionResult<RetrievalLogDto>> Create([FromBody] RetrievalRequest request)
    {
        var userId = _current.UserId
            ?? throw new ServiceException("未识别到当前用户", 401);
        var log = await _retrieval.CreateAsync(request, userId);
        return CreatedAtAction(nameof(Download), new { id = log.Id }, log);
    }

    [HttpGet("{id:long}/download")]
    [Authorize(Roles = "Admin,Clerk")]
    public async Task<IActionResult> Download(long id)
    {
        var (stream, fileName, contentType) = await _retrieval.DownloadAsync(id);
        return File(stream, contentType, fileName);
    }
}
