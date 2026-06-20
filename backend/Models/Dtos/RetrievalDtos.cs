using System.ComponentModel.DataAnnotations;
using JudicialEvidence.Api.Models.Entities;

namespace JudicialEvidence.Api.Models.Dtos;

public class RetrievalRequest
{
    [Required]
    public long EvidenceId { get; set; }

    [Required]
    [MaxLength(256)]
    public string Purpose { get; set; } = string.Empty;
}

public class RetrievalLogDto
{
    public long Id { get; set; }
    public long EvidenceId { get; set; }
    public string EvidenceName { get; set; } = string.Empty;
    public long CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public DateTime RetrievedAt { get; set; }

    public static RetrievalLogDto From(
        RetrievalLog r, string evidenceName, string caseNumber, string userName) => new()
        {
            Id = r.Id,
            EvidenceId = r.EvidenceId,
            EvidenceName = evidenceName,
            CaseId = r.CaseId,
            CaseNumber = caseNumber,
            UserId = r.UserId,
            UserName = userName,
            Purpose = r.Purpose,
            RetrievedAt = r.RetrievedAt
        };
}
