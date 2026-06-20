using JudicialEvidence.Api.Data;
using JudicialEvidence.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JudicialEvidence.Api.Repositories;

public interface IEvidenceRepository
{
    Task<List<Evidence>> ListByCaseAsync(long caseId);
    Task<Evidence?> GetByIdAsync(long id);
    Task<Evidence?> GetWithDetailsAsync(long id);
    Task AddAsync(Evidence entity);
    Task AddAdoptionAsync(EvidenceAdoption adoption);
    Task SaveChangesAsync();
}

public class EvidenceRepository : IEvidenceRepository
{
    private readonly AppDbContext _db;
    public EvidenceRepository(AppDbContext db) => _db = db;

    public Task<List<Evidence>> ListByCaseAsync(long caseId) =>
        _db.Evidence
           .Include(e => e.Uploader)
           .Where(e => e.CaseId == caseId)
           .OrderByDescending(e => e.UploadedAt)
           .ToListAsync();

    public Task<Evidence?> GetByIdAsync(long id) =>
        _db.Evidence.Include(e => e.Uploader).FirstOrDefaultAsync(e => e.Id == id);

    public Task<Evidence?> GetWithDetailsAsync(long id) =>
        _db.Evidence
           .Include(e => e.Uploader)
           .Include(e => e.Adoptions).ThenInclude(a => a.Reviewer)
           .Include(e => e.Retrievals).ThenInclude(r => r.User)
           .FirstOrDefaultAsync(e => e.Id == id);

    public async Task AddAsync(Evidence entity) => await _db.Evidence.AddAsync(entity);

    public async Task AddAdoptionAsync(EvidenceAdoption adoption) =>
        await _db.EvidenceAdoptions.AddAsync(adoption);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
