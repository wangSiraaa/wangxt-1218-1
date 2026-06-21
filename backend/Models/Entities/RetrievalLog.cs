using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JudicialEvidence.Api.Models;

namespace JudicialEvidence.Api.Models.Entities;

public class RetrievalLog
{
    public long Id { get; set; }

    public long EvidenceId { get; set; }

    [ForeignKey(nameof(EvidenceId))]
    public Evidence? Evidence { get; set; }

    public long CaseId { get; set; }

    [ForeignKey(nameof(CaseId))]
    public Case? Case { get; set; }

    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public RetrievalPurposeTag PurposeTag { get; set; }

    [Required]
    [MaxLength(256)]
    public string Purpose { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string CopyPath { get; set; } = string.Empty;

    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
}
