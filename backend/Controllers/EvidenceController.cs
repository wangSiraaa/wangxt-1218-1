using JudicialEvidence.Api.Auth;
using JudicialEvidence.Api.Models.Dtos;
using JudicialEvidence.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JudicialEvidence.Api.Controllers;

[ApiController]
[Route("api/evidence")]
[Authorize]
public class EvidenceController : ControllerBase
{
    private readonly IEvidenceService _evidence;
    private readonly ICurrentUserService _current;

    public EvidenceController(IEvidenceService evidence, ICurrentUserService current)
    {
        _evidence = evidence;
        _current = current;
    }

    [HttpGet]
    public async Task<ActionResult<List<EvidenceDto>>> List([FromQuery] long caseId)
        => Ok(await _evidence.ListByCaseAsync(caseId));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<EvidenceDetailDto>> GetById(long id)
        => Ok(await _evidence.GetByIdAsync(id));

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,Police")]
    [RequestSizeLimit(512_000_000)]
    public async Task<ActionResult<EvidenceDto>> Upload(
        [FromForm] long caseId,
        [FromForm] string hash,
        IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ServiceException("请上传证据文件");
        }
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ServiceException("请提供证据哈希值");
        }

        var userId = _current.UserId
            ?? throw new ServiceException("未识别到当前用户", 401);

        await using var stream = file.OpenReadStream();
        var result = await _evidence.UploadAsync(caseId, stream, file.FileName, hash, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:long}/adopt")]
    [Authorize(Roles = "Admin,Prosecutor")]
    public async Task<ActionResult<AdoptionDto>> Adopt(long id, [FromBody] AdoptionRequest request)
    {
        var reviewerId = _current.UserId
            ?? throw new ServiceException("未识别到当前用户", 401);
        return Ok(await _evidence.AdoptAsync(id, request, reviewerId));
    }

    [HttpGet("{id:long}/verify")]
    public async Task<ActionResult<object>> Verify(long id)
    {
        var ok = await _evidence.VerifyIntegrityAsync(id);
        return Ok(new { evidenceId = id, integrityOk = ok });
    }
}
