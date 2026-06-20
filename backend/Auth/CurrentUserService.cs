using System.Security.Claims;

namespace JudicialEvidence.Api.Auth;

public interface ICurrentUserService
{
    long? UserId { get; }
    string? Username { get; }
    string? Role { get; }
    string? FullName { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public long? UserId
    {
        get
        {
            var id = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(id, out var v) ? v : null;
        }
    }

    public string? Username => Principal?.FindFirstValue(ClaimTypes.Name);
    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);
    public string? FullName => Principal?.FindFirst("fullname")?.Value;
}
