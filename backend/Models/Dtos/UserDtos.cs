using System.ComponentModel.DataAnnotations;
using JudicialEvidence.Api.Models.Entities;

namespace JudicialEvidence.Api.Models.Dtos;

public class UserCreateRequest
{
    [Required]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = RoleNames.Police;
}

public class UserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static UserDto From(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        FullName = u.FullName,
        Role = u.Role,
        CreatedAt = u.CreatedAt
    };
}
