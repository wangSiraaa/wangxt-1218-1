using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JudicialEvidence.Api.Models.Entities;

public class Case
{
    public long Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string CaseNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string Stage { get; set; } = nameof(CaseStage.Police);

    public long CreatedBy { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public User? Creator { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Evidence> Evidence { get; set; } = new List<Evidence>();
    public ICollection<RetrievalLog> Retrievals { get; set; } = new List<RetrievalLog>();
}
