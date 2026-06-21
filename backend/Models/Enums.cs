namespace JudicialEvidence.Api.Models;

public enum UserRole
{
    Admin = 0,
    Police = 1,
    Prosecutor = 2,
    Clerk = 3
}

public enum CaseStage
{
    Police = 0,
    Review = 1,
    Court = 2,
    Closed = 3
}

public enum EvidenceStatus
{
    Pending = 0,
    Adopted = 1,
    Rejected = 2,
    Retrieved = 3
}

public enum RetrievalPurposeTag
{
    CourtHearingExhibit = 0,
    CourtDebate = 1,
    CollegiatePanel = 2,
    CaseReport = 3,
    ProsecutorialCommittee = 4,
    Other = 99
}

public static class RetrievalPurposeTagNames
{
    public const string CourtHearingExhibit = "庭审质证展示";
    public const string CourtDebate = "法庭辩论展示";
    public const string CollegiatePanel = "合议庭评议参考";
    public const string CaseReport = "案件汇报讨论";
    public const string ProsecutorialCommittee = "检察委员会审议";
    public const string Other = "其他";

    public static string From(RetrievalPurposeTag tag) => tag switch
    {
        RetrievalPurposeTag.CourtHearingExhibit => CourtHearingExhibit,
        RetrievalPurposeTag.CourtDebate => CourtDebate,
        RetrievalPurposeTag.CollegiatePanel => CollegiatePanel,
        RetrievalPurposeTag.CaseReport => CaseReport,
        RetrievalPurposeTag.ProsecutorialCommittee => ProsecutorialCommittee,
        RetrievalPurposeTag.Other => Other,
        _ => Other
    };

    public static readonly Dictionary<RetrievalPurposeTag, string> All = new()
    {
        { RetrievalPurposeTag.CourtHearingExhibit, CourtHearingExhibit },
        { RetrievalPurposeTag.CourtDebate, CourtDebate },
        { RetrievalPurposeTag.CollegiatePanel, CollegiatePanel },
        { RetrievalPurposeTag.CaseReport, CaseReport },
        { RetrievalPurposeTag.ProsecutorialCommittee, ProsecutorialCommittee },
        { RetrievalPurposeTag.Other, Other }
    };
}

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Police = "Police";
    public const string Prosecutor = "Prosecutor";
    public const string Clerk = "Clerk";

    public static string From(UserRole role) => role switch
    {
        UserRole.Admin => Admin,
        UserRole.Police => Police,
        UserRole.Prosecutor => Prosecutor,
        UserRole.Clerk => Clerk,
        _ => Police
    };
}
