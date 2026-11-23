using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduFlow.Web.Models;

[Table("instructor_profiles")]
public class InstructorProfile
{
    [Key, Column("user_id")]
    public long UserId { get; set; }
    [Column("headline")]
    public string? Headline { get; set; }
    [Column("identity_card")]
    public string? IdentityCard { get; set; }
    [Column("tax_id")]
    public string? TaxId { get; set; }
    [Column("kyc_status")]
    public string KycStatus { get; set; } = "pending";
    [Column("kyc_reject_reason")]
    public string? KycRejectReason { get; set; }
    [Column("commission_rate", TypeName = "decimal(5,2)")]
    public decimal CommissionRate { get; set; } = 70;
    [Column("bank_name")]
    public string? BankName { get; set; }
    [Column("bank_account_number")]
    public string? BankAccountNumber { get; set; }
    [Column("bank_account_name")] 
    public string? BankAccountName { get; set; }
    [Column("balance", TypeName = "decimal(15,2)")] 
    public decimal Balance { get; set; }
    [Column("submitted_at")] 
    public DateTime? SubmittedAt { get; set; }

    public User? User { get; set; }
}
