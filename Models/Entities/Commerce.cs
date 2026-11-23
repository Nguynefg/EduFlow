using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduFlow.Web.Models;
[Table("cart_items")]
public class CartItem
{
    [Column("user_id")]
    public long UserId { get; set; }

    [Column("course_id")]
    public long CourseId { get; set; }

    [Column("added_at")]
    public DateTime AddedAt { get; set; } = DateTime.Now;
    public Course? Course { get; set; }
}

[Table("coupons")]
public class Coupon
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Required, StringLength(50), Column("code")]
    public string Code { get; set; } = "";

    [Column("discount_type")]
    public string DiscountType { get; set; } = "percent";

    [Range(1, 100000000), Column("discount_value", TypeName = "decimal(15,2)")]
    public decimal DiscountValue { get; set; }

    [Range(1, 100000000), Column("max_discount", TypeName = "decimal(15,2)")]
    public decimal? MaxDiscount { get; set; }

    [Range(1, int.MaxValue), Column("total_limit")]
    public int? TotalLimit { get; set; }

    [Column("used_count")]
    public int UsedCount { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; } = DateTime.Now;

    [Column("end_date")]
    public DateTime EndDate { get; set; } = DateTime.Now.AddDays(30);

    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}

[Table("payout_requests")]
public class PayoutRequest
{
    [Column("tax_rate", TypeName = "decimal(5,2)")] public decimal? TaxRate { get; set; }
    [Column("tax_amount", TypeName = "decimal(15,2)")] public decimal? TaxAmount { get; set; }
    [Column("tax_rule"), StringLength(100)] public string? TaxRule { get; set; }
    [Column("tax_id_snapshot"), StringLength(50)] public string? TaxIdSnapshot { get; set; }
    [Column("payee_name_snapshot"), StringLength(255)] public string? PayeeNameSnapshot { get; set; }
    [Column("tax_declared")] public bool TaxDeclared { get; set; }
    [Column("tax_voluntary")] public bool TaxVoluntary { get; set; }
    [Column("tax_reviewed_by")] public long? TaxReviewedBy { get; set; }
    [Column("tax_reviewed_at")] public DateTime? TaxReviewedAt { get; set; }
    [Column("paid_at")] public DateTime? PaidAt { get; set; }
    [Column("tax_remitted_on")] public DateTime? TaxRemittedOn { get; set; }
    [Column("tax_remittance_ref"), StringLength(200)] public string? TaxRemittanceRef { get; set; }
    [Column("tax_remitted_by")] public long? TaxRemittedBy { get; set; }
    [NotMapped] public decimal? NetAmount => TaxAmount.HasValue ? Amount - TaxAmount.Value : null;

    [Column("is_test")] public bool IsTest { get; set; } = true;
    [Key, Column("id")]
    public long Id { get; set; }

    [Column("instructor_id")]
    public long InstructorId { get; set; }

    [Column("bank_name_snapshot")]
    public string? BankNameSnapshot { get; set; }

    [Column("bank_number_snapshot")]
    public string? BankNumberSnapshot { get; set; }

    [Column("bank_owner_snapshot")]
    public string? BankOwnerSnapshot { get; set; }

    [Column("amount", TypeName = "decimal(15,2)")]
    public decimal Amount { get; set; }

    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("admin_note")]
    public string? AdminNote { get; set; }

    [Column("receipt_url")]
    public string? ReceiptUrl { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }
    public User? Instructor { get; set; }
}

[Table("video_watch_sessions")]
public class VideoWatchSession
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [Column("enrollment_id")]
    public long EnrollmentId { get; set; }

    [Column("lesson_id")]
    public long LessonId { get; set; }

    [Column("position_sec")]
    public int PositionSec { get; set; }

    [Column("last_seen_utc")]
    public DateTime LastSeenUtc { get; set; }
}

[Table("video_watch_segments")]
public class VideoWatchSegment
{
    [Column("enrollment_id")]
    public long EnrollmentId { get; set; }

    [Column("lesson_id")]
    public long LessonId { get; set; }

    [Column("start_sec")]
    public int StartSec { get; set; }

    [Column("end_sec")]
    public int EndSec { get; set; }
}
