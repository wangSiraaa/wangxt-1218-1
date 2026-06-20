using System.ComponentModel.DataAnnotations;
using JudicialEvidence.Api.Models.Entities;

namespace JudicialEvidence.Api.Models.Dtos;

public class CaseCreateRequest
{
    [Required]
    [MaxLength(64)]
    public string CaseNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;
}

public class CaseDto
{
    public long Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public long CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int EvidenceCount { get; set; }

    public static CaseDto From(Case c, string creatorName, int evidenceCount) => new()
    {
        Id = c.Id,
        CaseNumber = c.CaseNumber,
        Title = c.Title,
        Stage = c.Stage,
        CreatedBy = c.CreatedBy,
        CreatorName = creatorName,
        CreatedAt = c.CreatedAt,
        EvidenceCount = evidenceCount
    };
}
