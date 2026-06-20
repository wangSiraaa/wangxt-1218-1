using JudicialEvidence.Api.Data;
using JudicialEvidence.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JudicialEvidence.Api.Repositories;

public interface ICaseRepository
{
    Task<List<Case>> ListAsync();
    Task<Case?> GetByIdAsync(long id);
    Task<Case?> GetByNumberAsync(string caseNumber);
    Task<bool> NumberExistsAsync(string caseNumber);
    Task AddAsync(Case entity);
    Task SaveChangesAsync();
}

public class CaseRepository : ICaseRepository
{
    private readonly AppDbContext _db;
    public CaseRepository(AppDbContext db) => _db = db;

    public Task<List<Case>> ListAsync() =>
        _db.Cases.Include(c => c.Creator).OrderByDescending(c => c.CreatedAt).ToListAsync();

    public Task<Case?> GetByIdAsync(long id) =>
        _db.Cases.Include(c => c.Creator).FirstOrDefaultAsync(c => c.Id == id);

    public Task<Case?> GetByNumberAsync(string caseNumber) =>
        _db.Cases.FirstOrDefaultAsync(c => c.CaseNumber == caseNumber);

    public Task<bool> NumberExistsAsync(string caseNumber) =>
        _db.Cases.AnyAsync(c => c.CaseNumber == caseNumber);

    public async Task AddAsync(Case entity) => await _db.Cases.AddAsync(entity);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
