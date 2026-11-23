/* =========================================================
   DỮ LIỆU MẪU ĐỂ TEST EDUFLOW (Sprint 1 + Sprint 2)
   Chạy SAU file 01_create_eduflowdb.sql (dữ liệu bạn tự tạo vẫn được giữ):
     sqlcmd -S FAKER\FAKERDB -E -C -f 65001 -i docs\02_seed_demo.sql

   Mật khẩu chung của tất cả tài khoản mẫu:  EduFlow@2026
   (danh sách tài khoản xem ở cuối file hoặc docs/README.md)
   Xóa dữ liệu mẫu: chạy docs/03_clear_demo.sql
   ========================================================= */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
GO
USE EduFlowDB;
GO
IF EXISTS (SELECT 1 FROM users WHERE email LIKE '%@eduflow.test')
   OR EXISTS (SELECT 1 FROM categories WHERE slug IN ('lap-trinh', 'khoa-hoc-du-lieu', 'thiet-ke', 'ngoai-ngu'))
BEGIN
    RAISERROR(N'Dữ liệu mẫu đã có trong CSDL. Chạy docs/03_clear_demo.sql trước nếu muốn nạp lại.', 16, 1);
    SET NOEXEC ON;
END
GO
BEGIN TRANSACTION;

DECLARE @pw NVARCHAR(255) = N'AQAAAAIAAYagAAAAEA90kh/biBeA9HPmUzY71TxXOKMFTbe52l291BCvQntTNie1Tt4p3lh8lyox4Y19tw==';  -- mật khẩu EduFlow@2026 đã băm (PBKDF2, định dạng ASP.NET)
DECLARE @rAdmin INT = (SELECT id FROM roles WHERE code = 'admin');
DECLARE @rInstructor INT = (SELECT id FROM roles WHERE code = 'instructor');
DECLARE @rStudent INT = (SELECT id FROM roles WHERE code = 'student');
DECLARE @u BIGINT, @c BIGINT, @s BIGINT, @l BIGINT, @qq BIGINT;

/* ================= TÀI KHOẢN ================= */
DECLARE @admin BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'admin@eduflow.test', @pw, N'Quản Trị Viên', N'0900000001', N'Quản trị hệ thống EduFlow.', 1, DATEADD(day, -120, GETDATE()), DATEADD(day, -120, GETDATE()), NULL);
SET @admin = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@admin, @rAdmin);
DECLARE @gv1 BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'tuan.nguyen@eduflow.test', @pw, N'Nguyễn Minh Tuấn', N'0901000001', N'Kỹ sư phần mềm 8 năm kinh nghiệm .NET và web.', 1, DATEADD(day, -100, GETDATE()), DATEADD(day, -100, GETDATE()), NULL);
SET @gv1 = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@gv1, @rStudent);
INSERT INTO user_roles(user_id, role_id) VALUES (@gv1, @rInstructor);
DECLARE @gv2 BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'ha.tran@eduflow.test', @pw, N'Trần Thu Hà', N'0901000002', N'Chuyên viên phân tích dữ liệu, giảng viên SQL và Python.', 1, DATEADD(day, -95, GETDATE()), DATEADD(day, -95, GETDATE()), NULL);
SET @gv2 = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@gv2, @rStudent);
INSERT INTO user_roles(user_id, role_id) VALUES (@gv2, @rInstructor);
DECLARE @gv3 BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'nam.le@eduflow.test', @pw, N'Lê Hoàng Nam', N'0901000003', N'Nhà thiết kế UI/UX và chuyên gia marketing số.', 1, DATEADD(day, -90, GETDATE()), DATEADD(day, -90, GETDATE()), NULL);
SET @gv3 = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@gv3, @rStudent);
INSERT INTO user_roles(user_id, role_id) VALUES (@gv3, @rInstructor);
DECLARE @gv4 BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'mai.pham@eduflow.test', @pw, N'Phạm Thanh Mai', N'0901000004', N'Giáo viên tiếng Anh IELTS 8.0, yêu âm nhạc.', 1, DATEADD(day, -85, GETDATE()), DATEADD(day, -85, GETDATE()), NULL);
SET @gv4 = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@gv4, @rStudent);
INSERT INTO user_roles(user_id, role_id) VALUES (@gv4, @rInstructor);
DECLARE @hv1 BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'an.hoang@eduflow.test', @pw, N'Hoàng Văn An', N'0902000001', N'Sinh viên năm 3 ngành CNTT.', 1, DATEADD(day, -40, GETDATE()), DATEADD(day, -40, GETDATE()), NULL);
SET @hv1 = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@hv1, @rStudent);
DECLARE @hv2 BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'binh.do@eduflow.test', @pw, N'Đỗ Thị Bình', N'0902000002', NULL, 1, DATEADD(day, -30, GETDATE()), DATEADD(day, -30, GETDATE()), NULL);
SET @hv2 = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@hv2, @rStudent);
DECLARE @hv3 BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'cuong.vu@eduflow.test', @pw, N'Vũ Quốc Cường', NULL, NULL, 1, DATEADD(day, -20, GETDATE()), DATEADD(day, -20, GETDATE()), NULL);
SET @hv3 = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@hv3, @rStudent);
DECLARE @hv4 BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'dung.ngo@eduflow.test', @pw, N'Ngô Mỹ Dung', NULL, NULL, 1, DATEADD(day, -6, GETDATE()), DATEADD(day, -6, GETDATE()), NULL);
SET @hv4 = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@hv4, @rStudent);
DECLARE @hv5 BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'em.bui@eduflow.test', @pw, N'Bùi Văn Em', NULL, NULL, 1, DATEADD(day, -3, GETDATE()), DATEADD(day, -3, GETDATE()), NULL);
SET @hv5 = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@hv5, @rStudent);
DECLARE @cho BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'giang.dang@eduflow.test', @pw, N'Đặng Hương Giang', N'0903000001', N'Muốn dạy kỹ năng mềm.', 1, DATEADD(day, -10, GETDATE()), DATEADD(day, -10, GETDATE()), NULL);
SET @cho = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@cho, @rStudent);
DECLARE @tuchoi BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'hung.ly@eduflow.test', @pw, N'Lý Minh Hùng', N'0903000002', NULL, 1, DATEADD(day, -12, GETDATE()), DATEADD(day, -12, GETDATE()), NULL);
SET @tuchoi = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@tuchoi, @rStudent);
DECLARE @khoa BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'khoa.bi@eduflow.test', @pw, N'Tài Khoản Bị Khóa', NULL, NULL, 0, DATEADD(day, -50, GETDATE()), DATEADD(day, -50, GETDATE()), NULL);
SET @khoa = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@khoa, @rStudent);
DECLARE @xoa BIGINT;
INSERT INTO users(email, password_hash, full_name, phone_number, bio, is_active, email_verified_at, created_at, deleted_at) VALUES (N'xoa.mem@eduflow.test', @pw, N'Tài Khoản Đã Xóa', NULL, NULL, 1, DATEADD(day, -60, GETDATE()), DATEADD(day, -60, GETDATE()), DATEADD(day, -5, GETDATE()));
SET @xoa = SCOPE_IDENTITY();
INSERT INTO user_roles(user_id, role_id) VALUES (@xoa, @rStudent);

/* ================= HỒ SƠ GIẢNG VIÊN (KYC) ================= */
INSERT INTO instructor_profiles(user_id, headline, identity_card, tax_id, kyc_status, kyc_reject_reason, bank_name, bank_account_number, bank_account_name, balance, submitted_at) VALUES (@gv1, N'Kỹ sư phần mềm .NET', N'001090000001', N'8000000001', N'approved', NULL, N'Vietcombank', N'0011000000001', N'NGUYEN MINH TUAN', 1250000, DATEADD(day, -8, GETDATE()));
INSERT INTO instructor_profiles(user_id, headline, identity_card, tax_id, kyc_status, kyc_reject_reason, bank_name, bank_account_number, bank_account_name, balance, submitted_at) VALUES (@gv2, N'Chuyên gia phân tích dữ liệu', N'001090000002', NULL, N'approved', NULL, N'Techcombank', N'19030000000002', N'TRAN THU HA', 870000, DATEADD(day, -8, GETDATE()));
INSERT INTO instructor_profiles(user_id, headline, identity_card, tax_id, kyc_status, kyc_reject_reason, bank_name, bank_account_number, bank_account_name, balance, submitted_at) VALUES (@gv3, N'Nhà thiết kế UI/UX', N'001090000003', N'8000000003', N'approved', NULL, N'BIDV', N'21510000000003', N'LE HOANG NAM', 430000, DATEADD(day, -8, GETDATE()));
INSERT INTO instructor_profiles(user_id, headline, identity_card, tax_id, kyc_status, kyc_reject_reason, bank_name, bank_account_number, bank_account_name, balance, submitted_at) VALUES (@gv4, N'Giáo viên tiếng Anh IELTS 8.0', N'001090000004', NULL, N'approved', NULL, N'MB Bank', N'0801000000004', N'PHAM THANH MAI', 2150000, DATEADD(day, -8, GETDATE()));
INSERT INTO instructor_profiles(user_id, headline, identity_card, tax_id, kyc_status, kyc_reject_reason, bank_name, bank_account_number, bank_account_name, balance, submitted_at) VALUES (@cho, N'Giảng viên kỹ năng mềm', N'001090000005', NULL, N'pending', NULL, N'ACB', N'1234500000005', N'DANG HUONG GIANG', 0, DATEADD(day, -8, GETDATE()));
INSERT INTO instructor_profiles(user_id, headline, identity_card, tax_id, kyc_status, kyc_reject_reason, bank_name, bank_account_number, bank_account_name, balance, submitted_at) VALUES (@tuchoi, N'Lập trình viên tự do', N'00109000006', NULL, N'rejected', N'Số CCCD chưa đúng 12 chữ số, vui lòng kiểm tra lại.', N'Agribank', N'1500000000006', N'LY MINH HUNG', 0, DATEADD(day, -8, GETDATE()));

/* ================= DANH MỤC ================= */
INSERT INTO categories(name, slug) VALUES (N'Lập trình', N'lap-trinh');
INSERT INTO categories(name, slug) VALUES (N'Khoa học dữ liệu', N'khoa-hoc-du-lieu');
INSERT INTO categories(name, slug) VALUES (N'Thiết kế', N'thiet-ke');
INSERT INTO categories(name, slug) VALUES (N'Ngoại ngữ', N'ngoai-ngu');
INSERT INTO categories(name, slug) VALUES (N'Kinh doanh', N'kinh-doanh');
INSERT INTO categories(name, slug) VALUES (N'Marketing', N'marketing');
INSERT INTO categories(name, slug) VALUES (N'Tin học văn phòng', N'tin-hoc-van-phong');
INSERT INTO categories(name, slug) VALUES (N'Nhiếp ảnh', N'nhiep-anh');
INSERT INTO categories(name, slug) VALUES (N'Âm nhạc', N'am-nhac');
INSERT INTO categories(parent_id, name, slug) VALUES ((SELECT id FROM categories WHERE slug = N'lap-trinh'), N'Lập trình Web', N'lap-trinh-web');
INSERT INTO categories(parent_id, name, slug) VALUES ((SELECT id FROM categories WHERE slug = N'lap-trinh'), N'Lập trình di động', N'lap-trinh-di-dong');
INSERT INTO categories(parent_id, name, slug) VALUES ((SELECT id FROM categories WHERE slug = N'ngoai-ngu'), N'Tiếng Anh', N'tieng-anh');
INSERT INTO categories(parent_id, name, slug) VALUES ((SELECT id FROM categories WHERE slug = N'ngoai-ngu'), N'Tiếng Nhật', N'tieng-nhat');
INSERT INTO categories(name, slug, is_active) VALUES (N'Danh mục đang ẩn', 'danh-muc-an', 0);

/* ================= KHÓA HỌC + NỘI DUNG ================= */
-- 1. Lập trình C# từ cơ bản đến nâng cao (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv1, (SELECT id FROM categories WHERE slug = N'lap-trinh'), N'Lập trình C# từ cơ bản đến nâng cao', N'khoa-hoc-mau-1', N'Nắm vững C# và lập trình hướng đối tượng qua dự án thực tế.', N'Khóa học Lập trình C# từ cơ bản đến nâng cao được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Lập trình C# từ cơ bản đến nâng cao' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 499000, 299000, DATEADD(day, -3, GETDATE()), DATEADD(day, 30, GETDATE()), N'published', NULL, 4.8, DATEADD(day, -60, GETDATE()), DATEADD(day, -65, GETDATE()), DATEADD(day, -60, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Lập trình C# từ cơ bản đến nâng cao</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 2. ASP.NET Core MVC thực chiến (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv1, (SELECT id FROM categories WHERE slug = N'lap-trinh-web'), N'ASP.NET Core MVC thực chiến', N'khoa-hoc-mau-2', N'Xây dựng website bán hàng hoàn chỉnh với ASP.NET Core MVC và SQL Server.', N'Khóa học ASP.NET Core MVC thực chiến được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của ASP.NET Core MVC thực chiến' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'intermediate', 899000, NULL, NULL, NULL, N'published', NULL, 4.7, DATEADD(day, -45, GETDATE()), DATEADD(day, -50, GETDATE()), DATEADD(day, -45, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>ASP.NET Core MVC thực chiến</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 3. HTML, CSS và JavaScript cho người mới (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv1, (SELECT id FROM categories WHERE slug = N'lap-trinh-web'), N'HTML, CSS và JavaScript cho người mới', N'khoa-hoc-mau-3', N'Tự tay làm trang web đầu tiên chỉ sau 2 tuần.', N'Khóa học HTML, CSS và JavaScript cho người mới được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của HTML, CSS và JavaScript cho người mới' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 0, NULL, NULL, NULL, N'published', NULL, 4.9, DATEADD(day, -80, GETDATE()), DATEADD(day, -85, GETDATE()), DATEADD(day, -80, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>HTML, CSS và JavaScript cho người mới</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 4. Lập trình Android với Kotlin (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv1, (SELECT id FROM categories WHERE slug = N'lap-trinh-di-dong'), N'Lập trình Android với Kotlin', N'khoa-hoc-mau-4', N'Phát triển ứng dụng Android hiện đại với Kotlin và Jetpack Compose.', N'Khóa học Lập trình Android với Kotlin được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Lập trình Android với Kotlin' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'intermediate', 699000, NULL, NULL, NULL, N'published', NULL, 4.4, DATEADD(day, -20, GETDATE()), DATEADD(day, -25, GETDATE()), DATEADD(day, -20, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Lập trình Android với Kotlin</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 5. Lập trình Python cơ bản (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv1, (SELECT id FROM categories WHERE slug = N'lap-trinh'), N'Lập trình Python cơ bản', N'khoa-hoc-mau-5', N'Ngôn ngữ dễ học nhất cho người mới bắt đầu lập trình.', N'Khóa học Lập trình Python cơ bản được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Lập trình Python cơ bản' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 0, NULL, NULL, NULL, N'published', NULL, 4.6, DATEADD(day, -70, GETDATE()), DATEADD(day, -75, GETDATE()), DATEADD(day, -70, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Lập trình Python cơ bản</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 6. SQL Server cho người mới bắt đầu (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv2, (SELECT id FROM categories WHERE slug = N'khoa-hoc-du-lieu'), N'SQL Server cho người mới bắt đầu', N'khoa-hoc-mau-6', N'Thiết kế CSDL, viết truy vấn SELECT, JOIN, GROUP BY thành thạo.', N'Khóa học SQL Server cho người mới bắt đầu được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của SQL Server cho người mới bắt đầu' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 399000, NULL, NULL, NULL, N'published', NULL, 4.6, DATEADD(day, -50, GETDATE()), DATEADD(day, -55, GETDATE()), DATEADD(day, -50, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>SQL Server cho người mới bắt đầu</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 7. Phân tích dữ liệu với Python và Pandas (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv2, (SELECT id FROM categories WHERE slug = N'khoa-hoc-du-lieu'), N'Phân tích dữ liệu với Python và Pandas', N'khoa-hoc-mau-7', N'Làm sạch, phân tích và trực quan hóa dữ liệu thực tế.', N'Khóa học Phân tích dữ liệu với Python và Pandas được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Phân tích dữ liệu với Python và Pandas' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'intermediate', 999000, 699000, DATEADD(day, -3, GETDATE()), DATEADD(day, 30, GETDATE()), N'published', NULL, 4.8, DATEADD(day, -35, GETDATE()), DATEADD(day, -40, GETDATE()), DATEADD(day, -35, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Phân tích dữ liệu với Python và Pandas</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 8. Machine Learning cơ bản (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv2, (SELECT id FROM categories WHERE slug = N'khoa-hoc-du-lieu'), N'Machine Learning cơ bản', N'khoa-hoc-mau-8', N'Hiểu và xây dựng các mô hình học máy đầu tiên với scikit-learn.', N'Khóa học Machine Learning cơ bản được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Machine Learning cơ bản' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'advanced', 1299000, NULL, NULL, NULL, N'published', NULL, 4.5, DATEADD(day, -15, GETDATE()), DATEADD(day, -20, GETDATE()), DATEADD(day, -15, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Machine Learning cơ bản</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 9. Excel từ cơ bản đến nâng cao (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv2, (SELECT id FROM categories WHERE slug = N'tin-hoc-van-phong'), N'Excel từ cơ bản đến nâng cao', N'khoa-hoc-mau-9', N'Hàm, bảng tính, PivotTable và biểu đồ cho dân văn phòng.', N'Khóa học Excel từ cơ bản đến nâng cao được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Excel từ cơ bản đến nâng cao' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 299000, NULL, NULL, NULL, N'published', NULL, 4.7, DATEADD(day, -90, GETDATE()), DATEADD(day, -95, GETDATE()), DATEADD(day, -90, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Excel từ cơ bản đến nâng cao</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 10. Thiết kế UI/UX với Figma (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv3, (SELECT id FROM categories WHERE slug = N'thiet-ke'), N'Thiết kế UI/UX với Figma', N'khoa-hoc-mau-10', N'Thiết kế giao diện ứng dụng đẹp, dễ dùng từ con số 0.', N'Khóa học Thiết kế UI/UX với Figma được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Thiết kế UI/UX với Figma' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 599000, 399000, DATEADD(day, -3, GETDATE()), DATEADD(day, 30, GETDATE()), N'published', NULL, 4.7, DATEADD(day, -40, GETDATE()), DATEADD(day, -45, GETDATE()), DATEADD(day, -40, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Thiết kế UI/UX với Figma</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 11. Photoshop cho người mới (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv3, (SELECT id FROM categories WHERE slug = N'thiet-ke'), N'Photoshop cho người mới', N'khoa-hoc-mau-11', N'Chỉnh sửa ảnh và thiết kế banner chuyên nghiệp.', N'Khóa học Photoshop cho người mới được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Photoshop cho người mới' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 349000, NULL, NULL, NULL, N'published', NULL, 4.3, DATEADD(day, -55, GETDATE()), DATEADD(day, -60, GETDATE()), DATEADD(day, -55, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Photoshop cho người mới</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 12. Digital Marketing từ A đến Z (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv3, (SELECT id FROM categories WHERE slug = N'marketing'), N'Digital Marketing từ A đến Z', N'khoa-hoc-mau-12', N'Facebook Ads, SEO, email marketing và đo lường hiệu quả.', N'Khóa học Digital Marketing từ A đến Z được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Digital Marketing từ A đến Z' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 499000, NULL, NULL, NULL, N'published', NULL, 4.2, DATEADD(day, -25, GETDATE()), DATEADD(day, -30, GETDATE()), DATEADD(day, -25, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Digital Marketing từ A đến Z</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 13. Khởi nghiệp và quản trị doanh nghiệp nhỏ (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv3, (SELECT id FROM categories WHERE slug = N'kinh-doanh'), N'Khởi nghiệp và quản trị doanh nghiệp nhỏ', N'khoa-hoc-mau-13', N'Từ ý tưởng đến mô hình kinh doanh có lãi.', N'Khóa học Khởi nghiệp và quản trị doanh nghiệp nhỏ được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Khởi nghiệp và quản trị doanh nghiệp nhỏ' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'intermediate', 0, NULL, NULL, NULL, N'published', NULL, 4.1, DATEADD(day, -10, GETDATE()), DATEADD(day, -15, GETDATE()), DATEADD(day, -10, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Khởi nghiệp và quản trị doanh nghiệp nhỏ</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 14. Nhiếp ảnh bằng điện thoại (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv3, (SELECT id FROM categories WHERE slug = N'nhiep-anh'), N'Nhiếp ảnh bằng điện thoại', N'khoa-hoc-mau-14', N'Chụp ảnh đẹp chỉ với chiếc điện thoại của bạn.', N'Khóa học Nhiếp ảnh bằng điện thoại được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Nhiếp ảnh bằng điện thoại' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 199000, NULL, NULL, NULL, N'published', NULL, 4.5, DATEADD(day, -5, GETDATE()), DATEADD(day, -10, GETDATE()), DATEADD(day, -5, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Nhiếp ảnh bằng điện thoại</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 15. Tiếng Anh giao tiếp cơ bản (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv4, (SELECT id FROM categories WHERE slug = N'tieng-anh'), N'Tiếng Anh giao tiếp cơ bản', N'khoa-hoc-mau-15', N'Tự tin giao tiếp tiếng Anh trong công việc và du lịch.', N'Khóa học Tiếng Anh giao tiếp cơ bản được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Tiếng Anh giao tiếp cơ bản' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 349000, NULL, NULL, NULL, N'published', NULL, 4.8, DATEADD(day, -65, GETDATE()), DATEADD(day, -70, GETDATE()), DATEADD(day, -65, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Tiếng Anh giao tiếp cơ bản</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 16. Luyện thi IELTS 6.5 (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv4, (SELECT id FROM categories WHERE slug = N'tieng-anh'), N'Luyện thi IELTS 6.5', N'khoa-hoc-mau-16', N'Chiến lược làm bài 4 kỹ năng Listening, Reading, Writing, Speaking.', N'Khóa học Luyện thi IELTS 6.5 được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Luyện thi IELTS 6.5' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'intermediate', 1499000, 1199000, DATEADD(day, -3, GETDATE()), DATEADD(day, 30, GETDATE()), N'published', NULL, 4.6, DATEADD(day, -30, GETDATE()), DATEADD(day, -35, GETDATE()), DATEADD(day, -30, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Luyện thi IELTS 6.5</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 17. Tiếng Nhật N5 cho người mới (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv4, (SELECT id FROM categories WHERE slug = N'tieng-nhat'), N'Tiếng Nhật N5 cho người mới', N'khoa-hoc-mau-17', N'Bảng chữ cái, từ vựng và ngữ pháp sơ cấp N5.', N'Khóa học Tiếng Nhật N5 cho người mới được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Tiếng Nhật N5 cho người mới' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 0, NULL, NULL, NULL, N'published', NULL, 4.4, DATEADD(day, -18, GETDATE()), DATEADD(day, -23, GETDATE()), DATEADD(day, -18, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Tiếng Nhật N5 cho người mới</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 18. Guitar đệm hát cơ bản (published)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv4, (SELECT id FROM categories WHERE slug = N'am-nhac'), N'Guitar đệm hát cơ bản', N'khoa-hoc-mau-18', N'Đệm hát 20 bài hát quen thuộc chỉ với 6 hợp âm.', N'Khóa học Guitar đệm hát cơ bản được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Guitar đệm hát cơ bản' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 299000, NULL, NULL, NULL, N'published', NULL, 4.5, DATEADD(day, -8, GETDATE()), DATEADD(day, -13, GETDATE()), DATEADD(day, -8, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Guitar đệm hát cơ bản</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 19. Lập trình Java Spring Boot (pending)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv1, (SELECT id FROM categories WHERE slug = N'lap-trinh-web'), N'Lập trình Java Spring Boot', N'khoa-hoc-mau-19', N'Xây dựng REST API với Spring Boot và JPA.', N'Khóa học Lập trình Java Spring Boot được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Lập trình Java Spring Boot' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'advanced', 1099000, NULL, NULL, NULL, N'pending', NULL, 0, NULL, DATEADD(day, -7, GETDATE()), DATEADD(day, -2, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Lập trình Java Spring Boot</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 20. Tiếng Hàn sơ cấp (draft)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv4, (SELECT id FROM categories WHERE slug = N'ngoai-ngu'), N'Tiếng Hàn sơ cấp', N'khoa-hoc-mau-20', N'Học bảng chữ Hangeul và giao tiếp cơ bản.', N'Khóa học Tiếng Hàn sơ cấp được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Tiếng Hàn sơ cấp' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 399000, NULL, NULL, NULL, N'draft', NULL, 0, NULL, DATEADD(day, -6, GETDATE()), DATEADD(day, -1, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Tiếng Hàn sơ cấp</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

-- 21. Bán hàng trên TikTok (rejected)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv3, (SELECT id FROM categories WHERE slug = N'marketing'), N'Bán hàng trên TikTok', N'khoa-hoc-mau-21', N'Xây kênh TikTok bán hàng hiệu quả.', N'Khóa học Bán hàng trên TikTok được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Bán hàng trên TikTok' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 299000, NULL, NULL, NULL, N'rejected', N'Video giới thiệu chưa rõ nội dung, chương 2 còn thiếu bài học.', 0, NULL, DATEADD(day, -9, GETDATE()), DATEADD(day, -4, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Bán hàng trên TikTok</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
-- 22. Kỹ năng thuyết trình (hidden)
INSERT INTO courses(instructor_id, category_id, title, slug, short_desc, description, objectives, requirements, trailer_url, level, price, discount_price, discount_start, discount_end, status, reject_reason, avg_rating, published_at, created_at, updated_at) VALUES (@gv3, (SELECT id FROM categories WHERE slug = N'kinh-doanh'), N'Kỹ năng thuyết trình', N'khoa-hoc-mau-22', N'Tự tin nói trước đám đông.', N'Khóa học Kỹ năng thuyết trình được thiết kế cho người học muốn nắm chắc kiến thức theo lộ trình rõ ràng. Mỗi chương có bài giảng video, bài đọc và bài kiểm tra cuối chương.', N'Hiểu kiến thức nền tảng của Kỹ năng thuyết trình' + CHAR(10) + N'Thực hành qua bài tập và dự án nhỏ' + CHAR(10) + N'Tự tin áp dụng vào công việc', N'Có máy tính hoặc điện thoại kết nối Internet' + CHAR(10) + N'Tinh thần tự học', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', N'beginner', 249000, NULL, NULL, NULL, N'hidden', N'Tạm ẩn để giảng viên cập nhật lại tài liệu.', 4.0, DATEADD(day, -75, GETDATE()), DATEADD(day, -80, GETDATE()), DATEADD(day, -75, GETDATE()));
SET @c = SCOPE_IDENTITY();
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 1: Giới thiệu', 1); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, is_free_preview, order_index) VALUES (@s, N'Giới thiệu khóa học', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 420, 1, 1);
INSERT INTO lessons(section_id, title, lesson_type, content, is_free_preview, order_index) VALUES (@s, N'Lộ trình học và tài liệu', 'article', N'<h2>Lộ trình học</h2><p>Khóa học <b>Kỹ năng thuyết trình</b> gồm 3 chương:</p><ol><li>Giới thiệu</li><li>Kiến thức nền tảng</li><li>Thực hành và kiểm tra</li></ol><p>Hãy tải tài liệu đính kèm bên dưới để học tốt hơn.</p>', 1, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO lesson_resources(lesson_id, file_name, file_url, file_size) VALUES (@l, N'tai-lieu-khoa-hoc.txt', N'/samples/tai-lieu-khoa-hoc.txt', 1024);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 2: Kiến thức nền tảng', 2); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Khái niệm cơ bản', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 900, 1);
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Ví dụ minh họa', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1200, 2);
INSERT INTO lessons(section_id, title, lesson_type, content, order_index) VALUES (@s, N'Tóm tắt chương 2', 'article', N'<h2>Ghi nhớ</h2><ul><li>Nắm vững khái niệm trước khi làm bài tập.</li><li>Xem lại video ví dụ nếu chưa hiểu.</li></ul>', 3);
INSERT INTO sections(course_id, title, order_index) VALUES (@c, N'Chương 3: Thực hành và kiểm tra', 3); SET @s = SCOPE_IDENTITY();
INSERT INTO lessons(section_id, title, lesson_type, video_url, duration_sec, order_index) VALUES (@s, N'Bài thực hành', 'video', N'https://www.youtube.com/watch?v=GhQdlIFylQ8', 1500, 1);
INSERT INTO lessons(section_id, title, lesson_type, pass_score, order_index) VALUES (@s, N'Bài kiểm tra cuối khóa', 'quiz', 70, 2); SET @l = SCOPE_IDENTITY();
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Khóa học này gồm bao nhiêu chương?', 'single', 1); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'2 chương', 0), (@qq, N'3 chương', 1), (@qq, N'4 chương', 0), (@qq, N'5 chương', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Muốn học bài tiếp theo, bạn cần đạt tối thiểu bao nhiêu % điểm bài kiểm tra?', 'single', 2); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'50%', 0), (@qq, N'60%', 0), (@qq, N'70%', 1), (@qq, N'100%', 0);
INSERT INTO quiz_questions(lesson_id, question_text, question_type, order_index) VALUES (@l, N'Điền từ còn thiếu: Học đi đôi với ___', 'fill', 3); SET @qq = SCOPE_IDENTITY();
INSERT INTO quiz_options(question_id, option_text, is_correct) VALUES (@qq, N'hành', 1);

COMMIT;
GO
SET NOEXEC OFF;
GO

/* ================= TÀI KHOẢN MẪU (mật khẩu: EduFlow@2026) =================
   admin@eduflow.test           Quản trị viên
   tuan.nguyen@eduflow.test     Giảng viên (lập trình, 6 khóa: 5 công khai + 1 chờ duyệt)
   ha.tran@eduflow.test         Giảng viên (dữ liệu, tin học văn phòng)
   nam.le@eduflow.test          Giảng viên (thiết kế, marketing, kinh doanh; có khóa bị từ chối và khóa bị ẩn)
   mai.pham@eduflow.test        Giảng viên (ngoại ngữ, âm nhạc; có 1 bản nháp)
   an.hoang@eduflow.test ...    Học viên (an.hoang, binh.do, cuong.vu, dung.ngo, em.bui)
   giang.dang@eduflow.test      Học viên có hồ sơ giảng viên ĐANG CHỜ DUYỆT
   hung.ly@eduflow.test         Học viên có hồ sơ giảng viên BỊ TỪ CHỐI (gửi lại được)
   khoa.bi@eduflow.test         Tài khoản BỊ KHÓA (không đăng nhập được)
   xoa.mem@eduflow.test         Tài khoản ĐÃ XÓA MỀM (xem ở Admin > Tài khoản đã xóa)
   ========================================================================== */
