namespace EduFlow.Web.Models;

public static class AppRoles
{
    public const string Admin = "admin";
    public const string Instructor = "instructor";
    public const string Student = "student";

    public static readonly string[] All = [Student, Instructor, Admin];
}

public static class KycStatus
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
}
