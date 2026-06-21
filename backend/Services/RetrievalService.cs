using JudicialEvidence.Api.Models;
using JudicialEvidence.Api.Models.Dtos;
using JudicialEvidence.Api.Models.Entities;
using JudicialEvidence.Api.Repositories;

namespace JudicialEvidence.Api.Services;

public interface IRetrievalService
{
    Task<RetrievalLogDto> CreateAsync(RetrievalRequest request, long userId);
    Task<(Stream Stream, string FileName, string ContentType)> DownloadAsync(long logId);
    Task<List<RetrievalLogDto>> ListAsync(long? caseId, long? userId, RetrievalPurposeTag? purposeTag, string? purpose);
}

public class RetrievalService : IRetrievalService
{
    private readonly IRetrievalLogRepository _logs;
    private readonly IEvidenceRepository _evidence;
    private readonly ICaseRepository _cases;
    private readonly IFileStorageService _storage;

    public RetrievalService(
        IRetrievalLogRepository logs,
        IEvidenceRepository evidence,
        ICaseRepository cases,
        IFileStorageService storage)
    {
        _logs = logs;
        _evidence = evidence;
        _cases = cases;
        _storage = storage;
    }

    public async Task<RetrievalLogDto> CreateAsync(RetrievalRequest request, long userId)
    {
        var evidence = await _evidence.GetByIdAsync(request.EvidenceId)
            ?? throw new ServiceException("证据不存在", 404);

        var caseEntity = await _cases.GetByIdAsync(evidence.CaseId)
            ?? throw new ServiceException("案件不存在", 404);

        var suffix = $"court_{DateTime.UtcNow:yyyyMMddHHmmss}";
        var copyPath = await _storage.CopyToAsync(evidence.FilePath, suffix);

        var log = new RetrievalLog
        {
            EvidenceId = evidence.Id,
            CaseId = evidence.CaseId,
            UserId = userId,
            PurposeTag = request.PurposeTag,
            Purpose = request.Purpose,
            CopyPath = copyPath,
            RetrievedAt = DateTime.UtcNow
        };
        await _logs.AddAsync(log);
        await _logs.SaveChangesAsync();

        var savedLog = await _logs.GetByIdAsync(log.Id);
        var userName = savedLog?.User?.FullName ?? string.Empty;
        return RetrievalLogDto.From(savedLog ?? log, evidence.Name, caseEntity.CaseNumber, userName);
    }

    public async Task<(Stream Stream, string FileName, string ContentType)> DownloadAsync(long logId)
    {
        var log = await _logs.GetByIdAsync(logId)
            ?? throw new ServiceException("调阅记录不存在", 404);

        var evidence = await _evidence.GetByIdAsync(log.EvidenceId)
            ?? throw new ServiceException("证据不存在", 404);

        var stream = _storage.OpenRead(log.CopyPath);
        var fileName = $"court_{log.Id}_{evidence.Name}";
        var contentType = GetContentType(evidence.Name);
        return (stream, fileName, contentType);
    }

    public async Task<List<RetrievalLogDto>> ListAsync(long? caseId, long? userId, RetrievalPurposeTag? purposeTag, string? purpose)
    {
        var logs = await _logs.ListAsync(caseId, userId, purposeTag, purpose);
        return logs.Select(r => RetrievalLogDto.From(
            r,
            r.Evidence?.Name ?? "—",
            r.Case?.CaseNumber ?? "—",
            r.User?.FullName ?? "—")).ToList();
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".mp4" => "video/mp4",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
