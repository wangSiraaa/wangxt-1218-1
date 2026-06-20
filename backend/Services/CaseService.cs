using JudicialEvidence.Api.Data;
using JudicialEvidence.Api.Models.Dtos;
using JudicialEvidence.Api.Models.Entities;
using JudicialEvidence.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JudicialEvidence.Api.Services;

public interface ICaseService
{
    Task<List<CaseDto>> ListAsync();
    Task<CaseDto> GetByIdAsync(long id);
    Task<CaseDto> CreateAsync(CaseCreateRequest request, long createdBy);
}

public class CaseService : ICaseService
{
    private readonly ICaseRepository _cases;
    private readonly AppDbContext _db;

    public CaseService(ICaseRepository cases, AppDbContext db)
    {
        _cases = cases;
        _db = db;
    }

    public async Task<List<CaseDto>> ListAsync()
    {
        var cases = await _cases.ListAsync();
        var ids = cases.Select(c => c.Id).ToList();
        var counts = await _db.Evidence
            .Where(e => ids.Contains(e.CaseId))
            .GroupBy(e => e.CaseId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        return cases.Select(c => CaseDto.From(
            c,
            c.Creator?.FullName ?? "—",
            counts.TryGetValue(c.Id, out var n) ? n : 0)).ToList();
    }

    public async Task<CaseDto> GetByIdAsync(long id)
    {
        var c = await _cases.GetByIdAsync(id)
            ?? throw new ServiceException("案件不存在", 404);
        var count = await _db.Evidence.CountAsync(e => e.CaseId == id);
        return CaseDto.From(c, c.Creator?.FullName ?? "—", count);
    }

    public async Task<CaseDto> CreateAsync(CaseCreateRequest request, long createdBy)
    {
        if (await _cases.NumberExistsAsync(request.CaseNumber))
        {
            throw new ServiceException("案件编号已存在");
        }

        var entity = new Case
        {
            CaseNumber = request.CaseNumber,
            Title = request.Title,
            Stage = nameof(CaseStage.Police),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
        await _cases.AddAsync(entity);
        await _cases.SaveChangesAsync();

        return await GetByIdAsync(entity.Id);
    }
}
