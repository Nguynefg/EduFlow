namespace EduFlow.Web.Areas.Admin.Models;

public record AdminMenuItem(string Controller, string Icon, string Label, int? Badge = null);
