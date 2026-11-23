/* =========================================================
   XÓA DỮ LIỆU MẪU (do 02_seed_demo.sql tạo ra)
   Chỉ xóa tài khoản có email @eduflow.test, khóa học mẫu và danh mục mẫu.
   Dữ liệu bạn tự tạo (email khác) được giữ nguyên.
   Chạy: sqlcmd -S FAKER\FAKERDB -E -C -f 65001 -i docs\03_clear_demo.sql
   ========================================================= */
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO
USE EduFlowDB;
GO
BEGIN TRANSACTION;

-- Khóa học mẫu (chương, bài, tài liệu, câu hỏi tự xóa theo ON DELETE CASCADE)
DELETE FROM courses WHERE slug LIKE 'khoa-hoc-mau-%'
   OR instructor_id IN (SELECT id FROM users WHERE email LIKE '%@eduflow.test');

-- Danh mục mẫu (chỉ xóa khi không còn khóa học nào dùng)
DELETE FROM categories
WHERE parent_id IS NOT NULL
  AND slug IN ('lap-trinh-web', 'lap-trinh-di-dong', 'tieng-anh', 'tieng-nhat')
  AND NOT EXISTS (SELECT 1 FROM courses c WHERE c.category_id = categories.id);
DELETE FROM categories
WHERE slug IN ('lap-trinh', 'khoa-hoc-du-lieu', 'thiet-ke', 'ngoai-ngu', 'kinh-doanh', 'marketing',
               'tin-hoc-van-phong', 'nhiep-anh', 'am-nhac', 'danh-muc-an')
  AND NOT EXISTS (SELECT 1 FROM courses c WHERE c.category_id = categories.id)
  AND NOT EXISTS (SELECT 1 FROM categories ch WHERE ch.parent_id = categories.id);

-- Tài khoản mẫu
DELETE FROM instructor_profiles WHERE user_id IN (SELECT id FROM users WHERE email LIKE '%@eduflow.test');
DELETE FROM user_roles WHERE user_id IN (SELECT id FROM users WHERE email LIKE '%@eduflow.test');
DELETE FROM users WHERE email LIKE '%@eduflow.test';

COMMIT;
PRINT N'Đã xóa dữ liệu mẫu.';
GO
