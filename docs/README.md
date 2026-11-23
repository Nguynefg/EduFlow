# EduFlow – Hệ thống học trực tuyến

ASP.NET Core MVC (.NET 9) + Entity Framework Core + SQL Server. Giao diện tông xanh dương (Tailwind CSS).

Đã có chức năng chính **Sprint 1**, **Sprint 2** và mã triển khai **Sprint 3** (giỏ hàng, coupon, đơn hàng, adapter thanh toán, rút tiền, học tuần tự). Thanh toán sandbox cần bổ sung thông tin merchant để nghiệm thu với nhà cung cấp.

Xem hướng dẫn Sprint 3 tại [SPRINT_3.md](SPRINT_3.md). Sau khi khôi phục backup cũ, chạy thêm `docs/04_sprint3.sql`.

## Chạy lần đầu

1. Tạo CSDL (chỉ chạy 1 lần):

   ```
   sqlcmd -S . -E -C -f 65001 -i docs\01_create_eduflowdb.sql
   ```

2. Kiểm tra chuỗi kết nối trong `appsettings.json` (mục `ConnectionStrings:DefaultConnection`).
3. Chạy web: `dotnet run` rồi mở http://localhost:5178
4. Vào http://localhost:5178/Account/Setup để tạo tài khoản **quản trị viên đầu tiên**.
   Trang này chỉ mở khi CSDL chưa có admin.
5. Admin vào **Danh mục** tạo vài danh mục trước, để giảng viên chọn khi tạo khóa học.

## Dữ liệu mẫu để test

Nạp dữ liệu mẫu (giữ nguyên dữ liệu bạn đã tự tạo):

```
sqlcmd -S . -E -C -f 65001 -i docs\02_seed_demo.sql
```

Xóa dữ liệu mẫu (chỉ xóa email `@eduflow.test`, khóa học mẫu, danh mục mẫu):

```
sqlcmd -S . -E -C -f 65001 -i docs\03_clear_demo.sql
```

Dữ liệu gồm 14 tài khoản, 14 danh mục (cha – con, 1 danh mục đang ẩn), 22 khóa học (18 công khai,
1 chờ duyệt, 1 bản nháp, 1 bị từ chối, 1 bị ẩn), 64 chương, 149 bài học (video, bài viết, quiz 3 câu),
22 tài liệu đính kèm (`wwwroot/samples/tai-lieu-khoa-hoc.txt`).

**Mật khẩu chung của mọi tài khoản mẫu: `EduFlow@2026`**

| Email | Vai trò / tình huống test |
|---|---|
| admin@eduflow.test | Quản trị viên |
| tuan.nguyen@eduflow.test | Giảng viên lập trình: 5 khóa công khai + 1 khóa **chờ duyệt** |
| ha.tran@eduflow.test | Giảng viên dữ liệu, tin học văn phòng |
| nam.le@eduflow.test | Giảng viên thiết kế, marketing: có khóa **bị từ chối** và khóa **bị ẩn** |
| mai.pham@eduflow.test | Giảng viên ngoại ngữ, âm nhạc: có 1 **bản nháp** |
| an.hoang@eduflow.test, binh.do@…, cuong.vu@…, dung.ngo@…, em.bui@… | Học viên |
| giang.dang@eduflow.test | Học viên có hồ sơ giảng viên **đang chờ duyệt** (để admin duyệt) |
| hung.ly@eduflow.test | Học viên có hồ sơ giảng viên **bị từ chối** (gửi lại được) |
| khoa.bi@eduflow.test | Tài khoản **bị khóa** – không đăng nhập được |
| xoa.mem@eduflow.test | Tài khoản **đã xóa mềm** – khôi phục ở Admin > Tài khoản đã xóa |

## Chức năng Sprint 1 – Tài khoản và phân quyền

| Chức năng | Đường dẫn |
|---|---|
| Đăng ký, đăng nhập, đăng xuất | /Account/Register, /Account/Login |
| Đăng nhập bằng Google | nút trên trang Đăng nhập / Đăng ký |
| Quên mật khẩu (gửi mật khẩu tạm qua email) | /Password/Forgot |
| Hồ sơ cá nhân, ảnh đại diện, mạng xã hội | /Profile |
| Đổi mật khẩu | /Profile/ChangePassword |
| Gửi hồ sơ KYC làm giảng viên | /Kyc |
| Admin: tổng quan | /Admin |
| Admin: quản lý tài khoản, phân quyền, khóa, xóa mềm, khôi phục | /Admin/Users |
| Admin: duyệt / từ chối hồ sơ giảng viên | /Admin/Kyc |

Quên mật khẩu: nếu chưa điền mục `Smtp` trong `appsettings.json`, khi chạy ở chế độ
Development mật khẩu tạm sẽ hiện ngay trên trang để tự kiểm thử.

## Cấu hình đăng nhập Google

Nút **Đăng nhập với Google** có sẵn ở trang Đăng nhập và Đăng ký. Khi chưa cấu hình, bấm vào sẽ báo "Chưa cấu hình đăng nhập Google".

1. Vào https://console.cloud.google.com → tạo Project (ví dụ `EduFlow`).
2. **APIs & Services → OAuth consent screen**: chọn *External*, điền tên ứng dụng, email hỗ trợ; ở mục *Test users* thêm email Google bạn dùng để thử.
3. **APIs & Services → Credentials → Create credentials → OAuth client ID**:
   - Application type: *Web application*
   - Authorized redirect URIs: `http://localhost:5178/signin-google`
4. Sao chép **Client ID** và **Client secret**, rồi chạy trong thư mục project (lưu ngoài mã nguồn, không bị đẩy lên Git):

   ```
   dotnet user-secrets set "Authentication:Google:ClientId" "<Client ID>"
   dotnet user-secrets set "Authentication:Google:ClientSecret" "<Client secret>"
   ```

5. Chạy lại web.

Cách hoạt động: Google trả về email, họ tên, ảnh đại diện.
- Email đã có tài khoản EduFlow → đăng nhập vào tài khoản đó (tài khoản bị khóa / đã xóa thì báo lỗi).
- Chưa có → tạo tài khoản **học viên** mới, không có mật khẩu, `email_verified_at` = thời điểm đăng nhập.
  Người dùng có thể đặt mật khẩu sau trong trang **Đổi mật khẩu** (bỏ trống ô mật khẩu hiện tại).

## Chức năng Sprint 2 – Khóa học và nội dung

| Chức năng | Đường dẫn |
|---|---|
| Trang chủ: khóa học nổi bật, mới nhất | / |
| Danh sách khóa học, tìm kiếm, lọc danh mục / cấp độ | /Courses |
| Chi tiết khóa học, video giới thiệu, mục lục | /Courses/Details/{id} |
| Học thử bài miễn phí | /Courses/Preview/{lessonId} |
| GV: danh sách khóa học, tạo, sửa, gửi duyệt, rút yêu cầu, xóa bản nháp | /Instructor |
| GV: chương, bài học (video / bài viết / quiz), tài liệu, kéo-thả sắp xếp, câu hỏi quiz | /Curriculum/Index/{courseId} |
| Admin: duyệt, từ chối, ẩn, hiện lại, xóa khóa học, xem toàn bộ nội dung | /Admin/Courses |
| Admin: danh mục cha - con | /Admin/Categories |

Trạng thái khóa học: **Bản nháp → Chờ duyệt → Đang công khai / Bị từ chối**. Admin có thể **Tạm ẩn** khóa học đang công khai.
Giảng viên chỉ gửi duyệt được khi khóa học có ít nhất 1 bài học.

Bài viết dùng trình soạn thảo Summernote. HTML được lọc mã độc (thư viện HtmlSanitizer) trước khi lưu.
Video: dán link YouTube / .mp4 hoặc tải file .mp4, .webm (tối đa 500 MB). File lưu trong `wwwroot/uploads`.

## Kiến trúc 3 tầng

```
Controller  (nhận request, kiểm tra form, trả về View)
    ↓ gọi
Service     (xử lý nghiệp vụ: kiểm tra dữ liệu, lưu, phân quyền)
    ↓ dùng
Model       (Entity + EduFlowDbContext → SQL Server)
```

Phía người dùng (`Services/`):

| Service | Dùng bởi | Nhiệm vụ |
|---|---|---|
| AccountService | AccountController, PasswordController | Đăng nhập, đăng ký, tạo admin đầu tiên, mật khẩu tạm |
| ProfileService | ProfileController | Xem/sửa hồ sơ, đổi mật khẩu |
| KycService | KycController | Học viên gửi / gửi lại hồ sơ KYC |
| CourseService | HomeController, CoursesController | Trang chủ, danh sách, chi tiết, học thử khóa học |
| InstructorCourseService | InstructorController | GV tạo, sửa, gửi duyệt, xóa khóa học |
| CurriculumService | CurriculumController | GV quản lý chương, bài học, tài liệu, câu hỏi quiz |

Khu quản trị (`Areas/Admin/Services/`):

| Service | Dùng bởi | Nhiệm vụ |
|---|---|---|
| AdminUserService | Admin/HomeController, Admin/UsersController | Thống kê, tìm kiếm, thêm, sửa, phân quyền, khóa, xóa mềm, khôi phục |
| AdminKycService | Admin/KycController, AdminMenuComponent | Duyệt / từ chối hồ sơ giảng viên, đếm hồ sơ chờ duyệt |
| AdminCourseService | Admin/CoursesController, AdminMenuComponent | Duyệt, từ chối, ẩn, xóa khóa học |
| AdminCategoryService | Admin/CategoriesController | Thêm, sửa, ẩn, xóa danh mục |

Quy ước: hàm service trả về `string?` là thông báo lỗi, `null` nghĩa là thành công.

## Cấu trúc thư mục

```
Areas/Admin     Khu quản trị, tách riêng:
  Controllers     Home, Users, Kyc, Courses, Categories
  Services        AdminUserService, AdminKycService, AdminCourseService, AdminCategoryService
  Models          ViewModel của admin, menu
  Components      Menu bên trái
  Views           Giao diện admin
Components      ViewComponent menu tài khoản
Controllers     Account, Password, Profile, Kyc, Home, Courses, Instructor, Curriculum
Services        Tầng xử lý nghiệp vụ phía người dùng
Extensions      Đăng ký dịch vụ (CSDL, service, đăng nhập cookie)
Helpers         Hàm dùng chung: cookie đăng nhập, mật khẩu, upload file, slug, video YouTube, gửi email
Models          Entity (bảng CSDL), DbContext, ViewModels, hằng số trạng thái
Views           Giao diện Razor
wwwroot         CSS, JS, file upload
docs            Tài liệu yêu cầu, script tạo CSDL
```
