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
