namespace EduFlow.Web.Models.ViewModels;
public class ReviewsViewModel
{
    public long CourseId { get; set; }
    public bool CanReview { get; set; }
    public bool CanReply { get; set; }
    public bool IsAdmin { get; set; }
    public int Percent { get; set; }
    public int Total { get; set; }
    public decimal Average { get; set; }
    public int Page { get; set; }
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / 10d));
    public Dictionary<byte,int> Stars { get; set; } = new();
    public CourseReview? Mine { get; set; }
    public List<CourseReview> Items { get; set; } = new();
}
public class DiscussionViewModel
{
    public Lesson Lesson { get; set; } = null!;
    public long CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public bool CanModerate { get; set; }
    public long UserId { get; set; }
    public bool InLearn { get; set; }
    public string? Query { get; set; }
    public int Page { get; set; }
    public int Total { get; set; }
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / 10d));
    public List<Discussion> Questions { get; set; } = new();
    public List<Discussion> Answers { get; set; } = new();
    public HashSet<long> VotedIds { get; set; } = new();
}

public class DiscussionItemViewModel
{
    public Discussion Item { get; set; } = null!;
    public DiscussionViewModel Page { get; set; } = null!;
}
