using JudicialEvidence.Api.Data;
using JudicialEvidence.Api.Models;
using JudicialEvidence.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JudicialEvidence.Api.Repositories;

public interface IRetrievalLogRepository
{
    Task<List<RetrievalLog>> ListAsync(long? caseId, long? userId, RetrievalPurposeTag? purposeTag, string? purpose);
    Task<RetrievalLog?> GetByIdAsync(long id);
    Task AddAsync(RetrievalLog entity);
    Task SaveChangesAsync();
}

public class RetrievalLogRepository : IRetrievalLogRepository
{
    private readonly AppDbContext _db;
    public RetrievalLogRepository(AppDbContext db) => _db = db;

    public async Task<List<RetrievalLog>> ListAsync(long? caseId, long? userId, RetrievalPurposeTag? purposeTag, string? purpose)
    {
        var q = _db.RetrievalLogs
            .Include(r => r.Evidence)
            .Include(r => r.Case)
            .Include(r => r.User)
            .AsQueryable();

        if (caseId.HasValue) q = q.Where(r => r.CaseId == caseId.Value);
        if (userId.HasValue) q = q.Where(r => r.UserId == userId.Value);
        if (purposeTag.HasValue)
            q = q.Where(r => r.PurposeTag == purposeTag.Value);
        if (!string.IsNullOrWhiteSpace(purpose))
            q = q.Where(r => r.Purpose.Contains(purpose));

        return await q.OrderByDescending(r => r.RetrievedAt).ToListAsync();
    }

    public Task<RetrievalLog?> GetByIdAsync(long id) =>
        _db.RetrievalLogs
           .Include(r => r.Evidence)
           .Include(r => r.Case)
           .Include(r => r.User)
           .FirstOrDefaultAsync(r => r.Id == id);

    public async Task AddAsync(RetrievalLog entity) => await _db.RetrievalLogs.AddAsync(entity);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
