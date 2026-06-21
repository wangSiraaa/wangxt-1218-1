using JudicialEvidence.Api.Models;
using JudicialEvidence.Api.Models.Dtos;
using JudicialEvidence.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JudicialEvidence.Api.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize(Roles = "Admin,Prosecutor,Clerk")]
public class LogsController : ControllerBase
{
    private readonly IRetrievalService _retrieval;
    public LogsController(IRetrievalService retrieval) => _retrieval = retrieval;

    [HttpGet]
    public async Task<ActionResult<List<RetrievalLogDto>>> List(
        [FromQuery] long? caseId,
        [FromQuery] long? userId,
        [FromQuery] RetrievalPurposeTag? purposeTag,
        [FromQuery] string? purpose)
        => Ok(await _retrieval.ListAsync(caseId, userId, purposeTag, purpose));
}
