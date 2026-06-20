using JudicialEvidence.Api.Auth;
using JudicialEvidence.Api.Models.Dtos;
using JudicialEvidence.Api.Models.Entities;
using JudicialEvidence.Api.Repositories;

namespace JudicialEvidence.Api.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _tokens;

    public AuthService(IUserRepository users, IPasswordHasher hasher, IJwtTokenService tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _users.GetByUsernameAsync(request.Username);
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            throw new ServiceException("账号或密码错误", 401);
        }

        return new LoginResponse
        {
            Token = _tokens.IssueToken(user),
            User = new AuthUserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role
            }
        };
    }
}
