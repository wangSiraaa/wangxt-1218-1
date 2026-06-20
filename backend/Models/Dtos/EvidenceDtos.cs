using System.ComponentModel.DataAnnotations;
using JudicialEvidence.Api.Models.Entities;

namespace JudicialEvidence.Api.Models.Dtos;

public class EvidenceDto
{
    public long Id { get; set; }
    public long CaseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public string UploadedHash { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsAdopted { get; set; }
    public bool HashVerified { get; set; }
    public long UploadedBy { get; set; }
    public string UploaderName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }

    public static EvidenceDto From(Evidence e, string uploaderName) => new()
    {
        Id = e.Id,
        CaseId = e.CaseId,
        Name = e.Name,
        Sha256 = e.Sha256,
        UploadedHash = e.UploadedHash,
        Status = e.Status,
        IsAdopted = e.IsAdopted,
        HashVerified = string.Equals(e.Sha256, e.UploadedHash, StringComparison.OrdinalIgnoreCase),
        UploadedBy = e.UploadedBy,
        UploaderName = uploaderName,
        UploadedAt = e.UploadedAt
    };
}

public class EvidenceDetailDto : EvidenceDto
{
    public string FilePath { get; set; } = string.Empty;
    public List<AdoptionDto> Adoptions { get; set; } = new();
    public List<RetrievalLogDto> Retrievals { get; set; } = new();
}

public class AdoptionRequest
{
    [Required]
    [MaxLength(512)]
    public string Opinion { get; set; } = string.Empty;

    public bool Adopted { get; set; }
}

public class AdoptionDto
{
    public long Id { get; set; }
    public long EvidenceId { get; set; }
    public long ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string Opinion { get; set; } = string.Empty;
    public bool Adopted { get; set; }
    public DateTime CreatedAt { get; set; }

    public static AdoptionDto From(EvidenceAdoption a, string reviewerName) => new()
    {
        Id = a.Id,
        EvidenceId = a.EvidenceId,
        ReviewerId = a.ReviewerId,
        ReviewerName = reviewerName,
        Opinion = a.Opinion,
        Adopted = a.Adopted,
        CreatedAt = a.CreatedAt
    };
}
