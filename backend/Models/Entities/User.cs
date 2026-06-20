using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JudicialEvidence.Api.Models.Entities;

public class User
{
    public long Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string Role { get; set; } = RoleNames.Police;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Case> Cases { get; set; } = new List<Case>();
    public ICollection<Evidence> UploadedEvidence { get; set; } = new List<Evidence>();
    public ICollection<RetrievalLog> Retrievals { get; set; } = new List<RetrievalLog>();
}
