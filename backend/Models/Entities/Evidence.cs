using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JudicialEvidence.Api.Models.Entities;

public class Evidence
{
    public long Id { get; set; }

    public long CaseId { get; set; }

    [ForeignKey(nameof(CaseId))]
    public Case? Case { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Sha256 { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string UploadedHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string Status { get; set; } = nameof(EvidenceStatus.Pending);

    public bool IsAdopted { get; set; }

    public long UploadedBy { get; set; }

    [ForeignKey(nameof(UploadedBy))]
    public User? Uploader { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EvidenceAdoption> Adoptions { get; set; } = new List<EvidenceAdoption>();
    public ICollection<RetrievalLog> Retrievals { get; set; } = new List<RetrievalLog>();
}
