using JudicialEvidence.Api.Auth;
using JudicialEvidence.Api.Data;
using JudicialEvidence.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JudicialEvidence.Api.Services;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync())
        {
            return;
        }

        var seedUsers = new[]
        {
            new User { Username = "admin", FullName = "系统管理员", Role = RoleNames.Admin,
                       PasswordHash = hasher.Hash("admin123") },
            new User { Username = "police", FullName = "张警官(公安)", Role = RoleNames.Police,
                       PasswordHash = hasher.Hash("police123") },
            new User { Username = "prosecutor", FullName = "李检察官(检察院)", Role = RoleNames.Prosecutor,
                       PasswordHash = hasher.Hash("pro123") },
            new User { Username = "clerk", FullName = "王书记员(法院)", Role = RoleNames.Clerk,
                       PasswordHash = hasher.Hash("clerk123") }
        };

        await db.Users.AddRangeAsync(seedUsers);
        await db.SaveChangesAsync();
    }
}
