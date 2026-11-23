namespace EduFlow.Web.Areas.Admin.Models;

public record AdminMenuViewModel(string? Controller, bool Deleted, IReadOnlyList<AdminMenuItem> Items);
