using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduFlow.Web.Models;

[Table("courses")]
public class Course
{
    [Key, Column("id")]
    public long Id { get; set; }
    [Column("instructor_id")] public long InstructorId { get; set; }
    [Column("category_id")] public int CategoryId { get; set; }
    [Column("title")] public string Title { get; set; } = "";
    [Column("slug")] public string Slug { get; set; } = "";
    [Column("short_desc")] public string? ShortDesc { get; set; }
    [Column("description")] public string? Description { get; set; }
    [Column("objectives")] public string? Objectives { get; set; }
    [Column("requirements")] public string? Requirements { get; set; }
    [Column("thumbnail_url")] public string? ThumbnailUrl { get; set; }
    [Column("trailer_url")] public string? TrailerUrl { get; set; }
    [Column("level")] public string Level { get; set; } = "beginner";
    [Column("price", TypeName = "decimal(15,2)")] public decimal Price { get; set; }
    [Column("discount_price", TypeName = "decimal(15,2)")] public decimal? DiscountPrice { get; set; }
    [Column("discount_start")] public DateTime? DiscountStart { get; set; }
    [Column("discount_end")] public DateTime? DiscountEnd { get; set; }
    [Column("status")] public string Status { get; set; } = "draft";
    [Column("reject_reason")] public string? RejectReason { get; set; }
    [Column("avg_rating", TypeName = "decimal(3,2)")] public decimal AvgRating { get; set; }
    [Column("published_at")] public DateTime? PublishedAt { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.Now;
    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
    [Column("deleted_at")] public DateTime? DeletedAt { get; set; }

    public User? Instructor { get; set; }
    public Category? Category { get; set; }
    public List<Section> Sections { get; set; } = new();

    // Giá đang áp dụng: có giá khuyến mãi và đang trong thời gian khuyến mãi thì lấy giá khuyến mãi
    [NotMapped]
    public decimal CurrentPrice
    {
        get
        {
            var now = DateTime.Now;
            bool onSale = DiscountPrice != null
                && (DiscountStart == null || DiscountStart <= now)
                && (DiscountEnd == null || DiscountEnd >= now);
            return onSale ? DiscountPrice!.Value : Price;
        }
    }
}
