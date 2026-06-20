using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JudicialEvidence.Api.Models.Entities;

public class EvidenceAdoption
{
    public long Id { get; set; }

    public long EvidenceId { get; set; }

    [ForeignKey(nameof(EvidenceId))]
    public Evidence? Evidence { get; set; }

    public long ReviewerId { get; set; }

    [ForeignKey(nameof(ReviewerId))]
    public User? Reviewer { get; set; }

    [Required]
    [MaxLength(512)]
    public string Opinion { get; set; } = string.Empty;

    public bool Adopted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
