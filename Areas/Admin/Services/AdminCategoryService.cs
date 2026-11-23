using EduFlow.Web.Helpers;
using EduFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Web.Areas.Admin.Services;

// Admin quản lý danh mục khóa học (có danh mục cha - con)
public class AdminCategoryService
{
    private readonly EduFlowDbContext _db;

    public AdminCategoryService(EduFlowDbContext db)
    {
        _db = db;
    }

    public async Task<List<Category>> ListAsync()
    {
        return await _db.Categories.Include(c => c.Parent).OrderBy(c => c.ParentId ?? c.Id).ThenBy(c => c.ParentId).ThenBy(c => c.Name).ToListAsync();
    }

    // Đếm số khóa học trong mỗi danh mục
    public async Task<Dictionary<int, int>> CountCoursesAsync()
    {
        return await _db.Courses.Where(c => c.DeletedAt == null)
            .GroupBy(c => c.CategoryId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    // Thêm (id = null) hoặc sửa danh mục
    public async Task<string?> SaveAsync(int? id, string name, int? parentId, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Nhập tên danh mục.";
        if (parentId != null && parentId == id) return "Danh mục cha không được là chính nó.";
        if (parentId != null && !await _db.Categories.AnyAsync(c => c.Id == parentId && c.ParentId == null))
            return "Chỉ chọn danh mục cấp 1 làm danh mục cha.";

        var slug = SlugHelper.ToSlug(name);
        if (await _db.Categories.AnyAsync(c => c.Slug == slug && c.Id != id))
            return "Đã có danh mục trùng tên.";

        Category? category;
        if (id == null)
        {
            category = new Category();
            _db.Categories.Add(category);
        }
        else
        {
            category = await _db.Categories.Include(c => c.Children).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return "Không tìm thấy danh mục.";
            if (parentId != null && category.Children.Count > 0)
                return "Danh mục đang có danh mục con nên không thể làm danh mục con.";
        }

        category.Name = name.Trim();
        category.Slug = slug;
        category.ParentId = parentId;
        category.IsActive = isActive;
        await _db.SaveChangesAsync();
        return null;
    }

    // Chỉ xóa được danh mục chưa có khóa học và chưa có danh mục con
    public async Task<string?> DeleteAsync(int id)
    {
        var category = await _db.Categories.Include(c => c.Children).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return "Không tìm thấy danh mục.";
        if (category.Children.Count > 0) return "Hãy xóa các danh mục con trước.";
        if (await _db.Courses.AnyAsync(c => c.CategoryId == id))
            return "Danh mục đang có khóa học. Hãy tắt hiển thị thay vì xóa.";
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return null;
    }
}
