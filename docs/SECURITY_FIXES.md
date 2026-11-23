# Gia cố bảo mật EduFlow

## Tài liệu bài học
- Tài liệu mới lưu trong App_Data/uploads/resources. Link tải dùng /MediaStream/Resource/{id}.
- Máy chủ kiểm tra quyền ghi danh, trạng thái mở khóa bài hoặc quyền quản lý; bài học thử chỉ công khai nếu đã bật học thử trong khóa đang công khai.
- Tài liệu cũ trong wwwroot vẫn đọc được qua endpoint có kiểm tra quyền. Mọi truy cập trực tiếp /uploads/resources bị chặn, không xóa hoặc di chuyển dữ liệu cũ.
- Áp dụng middleware ASP.NET Core cho tất cả request. Nếu reverse proxy/CDN tự phục vụ thư mục uploads, cần chặn resources và video tại đó; không để proxy bỏ qua ứng dụng.

## Video
- Khi lưu bài, trailer hoặc mô tả có video, chỉ dùng video của chủ khóa học. Kiểm tra lại khi phục vụ file để chặn cả liên kết sai trong dữ liệu cũ.
- Video tải trực tiếp mới nằm trong App_Data/uploads/videos/{chủ khóa học}/. Video trong thư viện media phải thuộc đúng chủ khóa học.
- Video trực tiếp đời cũ chưa có thư mục chủ sở hữu được đối chiếu với bài học đã lưu. Nếu cùng URL bị gắn vào bài của nhiều chủ khác nhau, hệ thống chặn và cần chủ khóa học tải lại video đúng của mình. Không tự suy đoán chủ sở hữu hay xóa file.

## Tạo admin đầu tiên
- /Account/Setup mặc định trả 404. Admin hiện có đăng nhập bình thường, không cần thiết lập lại.
- Chỉ khi cài mới: đặt Security__SetupKey trong môi trường máy chủ hoặc Security:SetupKey qua dotnet user-secrets; dùng khóa ngẫu nhiên tối thiểu 32 ký tự. Không ghi khóa vào Git, appsettings hoặc chia sẻ trong chat.
- Mở /Account/Setup và nhập đúng khóa cùng thông tin tài khoản. Kiểm tra khóa bằng so sánh thời gian cố định trên mã băm.
- Tạo admin dùng transaction Serializable và khóa dòng vai trò admin UPDLOCK/HOLDLOCK, kiểm tra lại trước khi ghi. Có admin thì trang đóng; xóa mềm admin không mở lại trang.
- Xóa khóa thiết lập khỏi môi trường sau khi hoàn tất.

## Giới hạn yêu cầu
- POST đăng nhập/đăng ký/thiết lập/quên mật khẩu/đặt lại mật khẩu/gửi lại xác thực: 20 request/IP/phút, vượt trả HTTP 429 và Retry-After.
- Đăng nhập: tối đa 10 lần thử/tài khoản/15 phút, không phân biệt IP. Sau thời gian chờ có thể đăng nhập lại.
- Email xác thực và đặt lại mật khẩu: mỗi loại tối đa 1 email/phút và 5 email/giờ/địa chỉ. Trang quên mật khẩu không tiết lộ tài khoản tồn tại hay bị giới hạn.
- Bộ đếm nằm trong bộ nhớ từng tiến trình, mất khi khởi động lại. Nếu chạy nhiều instance, cần dùng bộ đếm chia sẻ hoặc giới hạn bổ sung tại proxy. Chỉ khai báo proxy tin cậy để tránh giả mạo IP.

Không thay đổi dữ liệu tài chính hay gọi thanh toán thật. Không cần migration CSDL cho các thay đổi này.

## Kiểm chứng
- Build thành công, không cảnh báo hoặc lỗi biên dịch.
- 71 kiểm tra đã qua trên SQLite tạm và HTTP TestHost: tải tài liệu theo quyền, tài liệu cũ/mới, bài bị khóa, thu hồi quyền học, video khác chủ, video nhúng/trailer, giới hạn đồng thời, giới hạn tài khoản/IP/email, khóa thiết lập và chặn tạo admin thứ hai.
- Bộ kiểm tra SQL tạm loại bỏ cú pháp khóa riêng của SQL Server. Chưa nghiệm thu tranh chấp đồng thời trên SQL Server thật; không ghi vào EduFlowDB đang dùng, không gửi SMTP hoặc thanh toán thật.
- Nguồn kiểm tra lỗ hổng NuGet không truy cập được trong phiên này; không kết luận gói phụ thuộc không có lỗ hổng mới.
- Bộ đếm giới hạn dùng bộ nhớ một tiến trình. Bộ kiểm tra và dữ liệu tạm được dọn sau khi chạy.
