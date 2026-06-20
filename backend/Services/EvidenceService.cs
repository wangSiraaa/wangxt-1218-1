using JudicialEvidence.Api.Auth;
using JudicialEvidence.Api.Models.Dtos;
using JudicialEvidence.Api.Models.Entities;
using JudicialEvidence.Api.Repositories;
using JudicialEvidence.Api.Services;

namespace JudicialEvidence.Api.Services;

public interface IEvidenceService
{
    Task<EvidenceDto> UploadAsync(long caseId, Stream stream, string fileName, string uploadedHash, long uploadedBy);
    Task<List<EvidenceDto>> ListByCaseAsync(long caseId);
    Task<EvidenceDetailDto> GetByIdAsync(long id);
    Task<AdoptionDto> AdoptAsync(long evidenceId, AdoptionRequest request, long reviewerId);
    Task<bool> VerifyIntegrityAsync(long evidenceId);
}

public class EvidenceService : IEvidenceService
{
    private readonly IEvidenceRepository _evidence;
    private readonly ICaseRepository _cases;
    private readonly IHashService _hash;
    private readonly IFileStorageService _storage;

    public EvidenceService(
        IEvidenceRepository evidence,
        ICaseRepository cases,
        IHashService hash,
        IFileStorageService storage)
    {
        _evidence = evidence;
        _cases = cases;
        _hash = hash;
        _storage = storage;
    }

    public async Task<EvidenceDto> UploadAsync(
        long caseId, Stream stream, string fileName, string uploadedHash, long uploadedBy)
    {
        var caseEntity = await _cases.GetByIdAsync(caseId)
            ?? throw new ServiceException("案件不存在", 404);

        var computed = await _hash.ComputeSha256Async(stream);
        if (!_hash.Verify(computed, uploadedHash))
        {
            throw new ServiceException("哈希校验失败：上传哈希与文件实际 SHA-256 不一致，证据不能入库", 422);
        }

        var relativePath = await _storage.SaveAsync(stream, fileName);

        var entity = new Evidence
        {
            CaseId = caseId,
            Name = Path.GetFileName(fileName),
            FilePath = relativePath,
            Sha256 = computed,
            UploadedHash = uploadedHash.Trim().ToLowerInvariant(),
            Status = nameof(EvidenceStatus.Pending),
            IsAdopted = false,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow
        };
        await _evidence.AddAsync(entity);
        await _evidence.SaveChangesAsync();

        var uploader = entity.Uploader?.FullName ?? "—";
        return EvidenceDto.From(entity, uploader);
    }

    public async Task<List<EvidenceDto>> ListByCaseAsync(long caseId)
    {
        var list = await _evidence.ListByCaseAsync(caseId);
        return list.Select(e => EvidenceDto.From(e, e.Uploader?.FullName ?? "—")).ToList();
    }

    public async Task<EvidenceDetailDto> GetByIdAsync(long id)
    {
        var e = await _evidence.GetWithDetailsAsync(id)
            ?? throw new ServiceException("证据不存在", 404);

        return new EvidenceDetailDto
        {
            Id = e.Id,
            CaseId = e.CaseId,
            Name = e.Name,
            Sha256 = e.Sha256,
            UploadedHash = e.UploadedHash,
            Status = e.Status,
            IsAdopted = e.IsAdopted,
            HashVerified = string.Equals(e.Sha256, e.UploadedHash, StringComparison.OrdinalIgnoreCase),
            UploadedBy = e.UploadedBy,
            UploaderName = e.Uploader?.FullName ?? "—",
            UploadedAt = e.UploadedAt,
            FilePath = e.FilePath,
            Adoptions = e.Adoptions
                .OrderBy(a => a.CreatedAt)
                .Select(a => AdoptionDto.From(a, a.Reviewer?.FullName ?? "—"))
                .ToList(),
            Retrievals = e.Retrievals
                .OrderBy(r => r.RetrievedAt)
                .Select(r => RetrievalLogDto.From(r, e.Name, string.Empty, r.User?.FullName ?? "—"))
                .ToList()
        };
    }

    public async Task<AdoptionDto> AdoptAsync(long evidenceId, AdoptionRequest request, long reviewerId)
    {
        var e = await _evidence.GetWithDetailsAsync(evidenceId)
            ?? throw new ServiceException("证据不存在", 404);

        if (e.IsAdopted)
        {
            throw new ServiceException("证据已被采纳，原文件已冻结，不可覆盖或更改意见", 409);
        }

        var adoption = new EvidenceAdoption
        {
            EvidenceId = evidenceId,
            ReviewerId = reviewerId,
            Opinion = request.Opinion,
            Adopted = request.Adopted,
            CreatedAt = DateTime.UtcNow
        };
        await _evidence.AddAdoptionAsync(adoption);

        e.IsAdopted = request.Adopted;
        e.Status = request.Adopted
            ? nameof(EvidenceStatus.Adopted)
            : nameof(EvidenceStatus.Rejected);
        await _evidence.SaveChangesAsync();

        var updated = await _evidence.GetWithDetailsAsync(evidenceId);
        var lastAdoption = updated?.Adoptions.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
        var reviewerName = lastAdoption?.Reviewer?.FullName ?? "—";
        return AdoptionDto.From(lastAdoption ?? adoption, reviewerName);
    }

    public async Task<bool> VerifyIntegrityAsync(long evidenceId)
    {
        var e = await _evidence.GetByIdAsync(evidenceId)
            ?? throw new ServiceException("证据不存在", 404);

        await using var stream = _storage.OpenRead(e.FilePath);
        var recomputed = await _hash.ComputeSha256Async(stream);
        return _hash.Verify(recomputed, e.Sha256);
    }
}
