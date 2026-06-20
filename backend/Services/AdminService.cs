using JudicialEvidence.Api.Auth;
using JudicialEvidence.Api.Models.Dtos;
using JudicialEvidence.Api.Models.Entities;
using JudicialEvidence.Api.Repositories;

namespace JudicialEvidence.Api.Services;

public interface IAdminService
{
    Task<List<UserDto>> ListUsersAsync();
    Task<UserDto> CreateUserAsync(UserCreateRequest request);
}

public class AdminService : IAdminService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        RoleNames.Admin, RoleNames.Police, RoleNames.Prosecutor, RoleNames.Clerk
    };

    public AdminService(IUserRepository users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public async Task<List<UserDto>> ListUsersAsync()
    {
        var users = await _users.ListAsync();
        return users.Select(UserDto.From).ToList();
    }

    public async Task<UserDto> CreateUserAsync(UserCreateRequest request)
    {
        if (!ValidRoles.Contains(request.Role))
        {
            throw new ServiceException($"无效角色：{request.Role}");
        }
        if (await _users.GetByUsernameAsync(request.Username) is not null)
        {
            throw new ServiceException("用户名已存在");
        }

        var user = new User
        {
            Username = request.Username,
            PasswordHash = _hasher.Hash(request.Password),
            FullName = request.FullName,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };
        await _users.AddAsync(user);
        await _users.SaveChangesAsync();
        return UserDto.From(user);
    }
}
