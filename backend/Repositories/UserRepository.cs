using JudicialEvidence.Api.Data;
using JudicialEvidence.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JudicialEvidence.Api.Repositories;

public interface IUserRepository
{
    Task<List<User>> ListAsync();
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByUsernameAsync(string username);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task<List<User>> ListAsync() => _db.Users.OrderBy(u => u.Id).ToListAsync();

    public Task<User?> GetByIdAsync(long id) => _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByUsernameAsync(string username) =>
        _db.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task AddAsync(User user) => await _db.Users.AddAsync(user);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
