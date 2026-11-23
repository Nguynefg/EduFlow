using EduFlow.Web.Services;

namespace EduFlow.Web.Models.ViewModels;
public class CartViewModel
{
    public List<long> SelectedIds { get; set; } = new();
    public List<CartItem> SelectedItems => Items.Where(item => SelectedIds.Contains(item.CourseId)).ToList();
    public List<Order> PendingOrders { get; set; } = new();
    public List<CartItem> Items { get; set; } = new();
    public string? CouponCode { get; set; }
    public string? Error { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total => Subtotal - Discount;
    public List<string> Gateways { get; set; } = new();
}

public class OrderDetailsViewModel
{
    public bool DemoEnabled { get; set; }
    public Order Order { get; set; } = null!;
    public Dictionary<long, Course> Courses { get; set; } = new();
    public bool CanPay => Order.Status == OrderStatus.Pending && Order.CreatedAt.AddMinutes(30) > DateTime.Now;
    public List<string> Gateways { get; set; } = new();
}

public class PayoutViewModel
{
    public bool SimulationEnabled { get; set; }
    public decimal WithdrawableBalance { get; set; }
    public InstructorProfile Profile { get; set; } = null!;
    public List<PayoutRequest> Requests { get; set; } = new();
}
