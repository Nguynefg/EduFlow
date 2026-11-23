using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduFlow.Web.Models;

// Học viên đã mua / đăng ký khóa học
[Table("enrollments")]
public class Enrollment
{
    [Key, Column("id")] public long Id { get; set; }
    [Column("user_id")] public long UserId { get; set; }
    [Column("course_id")] public long CourseId { get; set; }
    [Column("progress_percent", TypeName = "decimal(5,2)")] public decimal ProgressPercent { get; set; }
    [Column("status")] public string Status { get; set; } = EnrollmentStatus.Active;
    [Column("enrolled_at")] public DateTime EnrolledAt { get; set; } = DateTime.Now;
    [Column("completed_at")] public DateTime? CompletedAt { get; set; }

    public User? User { get; set; }
    public Course? Course { get; set; }
    public List<LessonProgress> Progress { get; set; } = new();
}

// Tiến độ từng bài học của một lượt đăng ký
[Table("lesson_progress")]
public class LessonProgress
{
    [Column("enrollment_id")] public long EnrollmentId { get; set; }
    [Column("lesson_id")] public long LessonId { get; set; }
    [Column("last_position_sec")] public int LastPositionSec { get; set; }
    [Column("watched_sec")] public int WatchedSec { get; set; }
    [Column("quiz_score")] public int? QuizScore { get; set; }
    [Column("is_completed")] public bool IsCompleted { get; set; }
    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }

    public Enrollment? Enrollment { get; set; }
    public Lesson? Lesson { get; set; }
}

// Đơn hàng mua khóa học
[Table("orders")]
public class Order
{
    [Key, Column("id")] public long Id { get; set; }
    [Column("user_id")] public long UserId { get; set; }
    [Column("coupon_id")] public int? CouponId { get; set; }
    [Column("subtotal", TypeName = "decimal(15,2)")] public decimal Subtotal { get; set; }
    [Column("discount_amount", TypeName = "decimal(15,2)")] public decimal DiscountAmount { get; set; }
    [Column("total_price", TypeName = "decimal(15,2)")] public decimal TotalPrice { get; set; }
    [Column("status")] public string Status { get; set; } = OrderStatus.Pending;
    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<OrderItem> Items { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
}

[Table("order_items")]
public class OrderItem
{
    [Key, Column("id")] public long Id { get; set; }
    [Column("order_id")] public long OrderId { get; set; }
    [Column("course_id")] public long CourseId { get; set; }
    [Column("price", TypeName = "decimal(15,2)")] public decimal Price { get; set; }
    [Column("instructor_earning", TypeName = "decimal(15,2)")] public decimal InstructorEarning { get; set; }

    public Order? Order { get; set; }
}

[Table("payments")]
public class Payment
{
    [Column("is_test")] public bool IsTest { get; set; } = true;
    [Key, Column("id")] public long Id { get; set; }
    [Column("order_id")] public long OrderId { get; set; }
    [Column("gateway")] public string Gateway { get; set; } = "";
    [Column("gateway_txn_id")] public string? GatewayTxnId { get; set; }
    [Column("payment_url")] public string? PaymentUrl { get; set; }
    [Column("amount", TypeName = "decimal(15,2)")] public decimal Amount { get; set; }
    [Column("status")] public string Status { get; set; } = "pending";
    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Order? Order { get; set; }
}
