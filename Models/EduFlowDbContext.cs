using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Models;

// Kết nối tới CSDL EduFlowDB (tạo bằng file docs/01_create_eduflowdb.sql)
public class EduFlowDbContext : DbContext
{
    public EduFlowDbContext(DbContextOptions<EduFlowDbContext> options) : base(options) { }

    // Sprint 1: tài khoản và phân quyền
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<InstructorProfile> InstructorProfiles { get; set; }

    // Sprint 2: khóa học và nội dung
    public DbSet<Category> Categories { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<LessonResource> LessonResources { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizOption> QuizOptions { get; set; }

    // Sprint 3: mua khóa học và học tập
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<LessonProgress> LessonProgress { get; set; }

    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<PayoutRequest> PayoutRequests { get; set; }
    public DbSet<VideoWatchSession> VideoWatchSessions { get; set; }
    public DbSet<VideoWatchSegment> VideoWatchSegments { get; set; }

    public DbSet<CourseReview> CourseReviews { get; set; }
    public DbSet<Discussion> Discussions { get; set; }
    public DbSet<DiscussionVote> DiscussionVotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CourseReview>().HasKey(x => new { x.CourseId, x.UserId });
        modelBuilder.Entity<DiscussionVote>().HasKey(x => new { x.DiscussionId, x.UserId });
        modelBuilder.Entity<DiscussionVote>().HasOne<Discussion>().WithMany().HasForeignKey(x => x.DiscussionId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DiscussionVote>().HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Discussion>().HasOne<Discussion>().WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CartItem>().HasKey(x => new { x.UserId, x.CourseId });
        modelBuilder.Entity<VideoWatchSegment>().HasKey(x => new { x.EnrollmentId, x.LessonId, x.StartSec });
        modelBuilder.Entity<Enrollment>().HasIndex(x => new { x.UserId, x.CourseId }).IsUnique();
        // Khóa chính kép của user_roles
        modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });

        // Quan hệ 1-1 giữa users và instructor_profiles
        modelBuilder.Entity<InstructorProfile>()
            .HasOne(x => x.User)
            .WithOne(x => x.InstructorProfile)
            .HasForeignKey<InstructorProfile>(x => x.UserId);

        // Danh mục cha - con
        modelBuilder.Entity<Category>()
            .HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId);

        // Khóa học thuộc về 1 giảng viên
        modelBuilder.Entity<Course>()
            .HasOne(x => x.Instructor)
            .WithMany()
            .HasForeignKey(x => x.InstructorId);

        // Khóa chính kép của lesson_progress
        modelBuilder.Entity<LessonProgress>().HasKey(x => new { x.EnrollmentId, x.LessonId });
    }
}
