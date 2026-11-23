# Sprint 5 Thống kê báo cáo và chức năng hỗ trợ

Sprint 5 bổ sung dashboard theo vai trò, xuất Excel, bộ lọc đánh giá, hộp thư mô phỏng, liên kết xác thực/đặt lại mật khẩu, thời gian học và bảo vệ video tải lên. Không thêm bảng CSDL. Cần có cấu trúc CSDL của các sprint trước, bao gồm 05_live_payments.sql, 06_personal_tax.sql và 07_sprint4.sql.

## Cách chạy

Dừng ứng dụng đang chạy, vào thư mục EduFlow, chạy dotnet restore rồi dotnet run. Có thể cần đăng nhập lại vì cookie nay gắn với trạng thái mật khẩu; cookie cũ hoặc cookie trước khi đổi/đặt lại mật khẩu sẽ không còn hợp lệ.

## Thống kê và Excel

- Admin: /Admin, chọn khoảng ngày tối đa 366 ngày để xem tiền bán, phần nền tảng, số đơn, lượt mua khóa học và người dùng mới. Biểu đồ theo ngày, khoảng dài hơn 62 ngày gộp biểu đồ theo tháng; bảng chi tiết vẫn theo ngày. Nút Excel doanh thu và Excel người dùng xuất XLSX chuẩn, có tiêu đề, kiểu số, lọc và cố định dòng đầu; không xuất mật khẩu/hash hay dữ liệu định danh ngân hàng.
- Giảng viên: menu Tổng quan giảng viên (/Dashboard/Instructor). Doanh thu lọc theo kỳ; số dư, học viên và tiến độ là tổng hiện tại. Báo cáo chỉ lấy khóa của giảng viên đang đăng nhập, có xuất Excel. Số học viên duy nhất và lượt ghi danh hiển thị riêng; tiến độ tính lại từ bài hoàn thành, không tin phần trăm cũ lưu sẵn.
- Học viên: menu Tổng quan học tập (/Dashboard/Student), xem khóa đang học, đã xong, tiến độ và thời gian ghi nhận. Video tính các khoảng đã xem; bài viết/quiz ghi nhận khoảng thời gian trang đang hoạt động theo nhịp server. Không ước đoán thời gian học cũ chưa được theo dõi, không dùng tổng thời lượng khóa học thay cho thời gian đã học.
- Tài chính vẫn là mô phỏng khi Development + EnableDemo=true. Dashboard và Excel dùng cùng sổ mô phỏng, không cộng lẫn giao dịch thật; khóa miễn phí ghi danh nhưng không tạo doanh thu bán hàng.

## Tìm kiếm và đường dẫn

Danh sách khóa học lọc theo từ khóa, danh mục, cấp độ, miễn phí/trả phí và điểm đánh giá tối thiểu; giữ bộ lọc khi đổi trang/sắp xếp. Lọc học phí dùng giá khuyến mãi đang hiệu lực, bao gồm khóa đang được giảm về 0 đồng. Không đưa khóa ẩn/xóa, danh mục không hoạt động hoặc giảng viên bị khóa vào danh sách công khai.

Đường dẫn công khai: /khoa-hoc/{slug} và /danh-muc/{slug}. Link học và các thao tác quản trị vẫn dùng mã có kiểm tra quyền ở server.

## Email mô phỏng

Development mặc định Mail:Mode=Simulation; không gọi SMTP, kể cả khi máy có cấu hình SMTP khác. Email nằm trong App_Data/mail và xem tại /Mailbox. Người dùng chỉ xem thư gửi đến email tài khoản của mình; admin có thể chọn Xem mọi thư để thực hành vai trò hộp thư cho luồng quên mật khẩu khi người dùng chưa đăng nhập. Hộp thư chỉ có trong Development và không phục vụ file trực tiếp qua wwwroot.

Email được tạo cho đăng ký/xác thực email, yêu cầu đặt lại mật khẩu, thanh toán thành công, kết quả duyệt KYC, khóa học và rút tiền. Email nghiệp vụ gửi sau khi commit; xử lý thanh toán lặp không gửi lại email. Không tự tạo email cho giao dịch lịch sử đã hoàn tất trước khi nâng cấp.

Liên kết xác thực/đặt lại mật khẩu có hạn một giờ, gắn với tài khoản, email, mục đích và trạng thái mật khẩu. Đặt lại chỉ thay mật khẩu sau khi token hợp lệ; không đổi ngay khi bấm Quên mật khẩu và không hiện mật khẩu tạm trên trang. Có thể gửi lại xác thực email trong Hồ sơ. Bản demo vẫn cho đăng nhập trước khi xác thực để giữ luồng đang có.

Mail:PublicBaseUrl mặc định http://localhost:5178 theo launchSettings.json. Nếu đổi cổng/địa chỉ chạy, cập nhật giá trị này; không lấy hostname từ form hoặc Host header để tạo liên kết. Khi cần SMTP thật, cấu hình Mail:Mode=Smtp, Smtp:Host/Port/From/Username/Password; khóa bí mật đặt bằng biến môi trường hoặc secret store. Production cần PublicBaseUrl HTTPS hợp lệ. SMTP thật, tên miền và HTTPS triển khai chưa được nghiệm thu trong phiên này. Lỗi gửi thư không đảo ngược giao dịch đã commit; lỗi được ghi log và chưa có hàng đợi gửi lại tự động.

## Video và giao diện

Video tải lên mới lưu trong App_Data; file video cũ còn ở wwwroot cũng bị chặn truy cập qua URL tĩnh /uploads. /MediaStream/Lesson/{id} kiểm tra quyền ghi danh, trạng thái bài bị khóa, chủ sở hữu hoặc admin trước khi phục vụ file; bài học thử công khai vẫn được xem. Hỗ trợ HTTP Range để phát/tua video, không đưa đường dẫn lưu trữ vào player của học viên. Video trong bài viết/mô tả được thay bằng endpoint theo mã nội dung và thứ tự video. Thư viện media vẫn cho chủ sở hữu/admin xem trước; không cho giảng viên khác đọc file riêng.

Link video MP4 bên ngoài không được proxy tùy ý; cần tải video lên hoặc dùng YouTube. YouTube vẫn là dịch vụ nhúng công khai nên không thể giấu ID video. Đây là kiểm soát truy cập cho video tải lên, không phải DRM; trình duyệt đã được cấp quyền xem vẫn có thể ghi lại nội dung. Giao diện dùng controlslist=nodownload và không đưa nút tải file gốc.

Các trang mới có nhãn biểu mẫu, focus bàn phím, bảng cuộn trong vùng riêng, bố cục thu gọn theo màn hình và hỗ trợ theme tối. Biểu đồ có bảng số liệu thay thế, không chỉ truyền đạt bằng màu.

## Kiểm chứng

102 kiểm tra đã qua trên SQLite tạm và HTTP TestHost, gồm thống kê, XLSX/XML và kiểu số, token/email/quyền hộp thư, video có kiểm tra quyền/Range, đăng ký học miễn phí, coupon, thanh toán/rút tiền/thuế mô phỏng, đánh giá/hỏi đáp và các trang Razor. Không gửi email/thanh toán thật, không ghi vào EduFlowDB đang sử dụng. File kiểm tra và dữ liệu thử đã được dọn khỏi workspace; không thêm bộ test vào source. Chưa kiểm tra đồng thời trên SQL Server, SMTP thật, trình duyệt di động thực hoặc DRM.

HtmlSanitizer đã nâng lên 11.0.26 (kèm AngleSharp 1.10.0). Kiểm tra NuGet trực tiếp và phụ thuộc gián tiếp không báo gói có lỗ hổng từ nguồn hiện tại tại thời điểm kiểm tra. Đây không phải cam kết ứng dụng không còn mọi lỗ hổng.

Nguồn đối chiếu bản vá:
- https://github.com/mganss/HtmlSanitizer/security/advisories/GHSA-464h-4j8h-q5mf
- https://github.com/mganss/HtmlSanitizer/security/advisories/GHSA-j92c-7v7g-gj3f
- https://github.com/advisories/GHSA-pgww-w46g-26qg

### Tổng quan học tập và lưu tiến độ
- Tổng quan có bài đang học dở, vị trí video đã lưu, bộ lọc và tiến độ từng khóa.
- Video gửi vị trí tự động, khi tạm dừng/rời trang và qua nút Lưu tiến độ; phản hồi thành công hiện thời điểm lưu từ máy chủ.
- Nhịp gửi chậm được so với thời gian thực để tránh coi nhầm việc xem bình thường là tua; phiên theo dõi thay đổi sẽ tự kết nối lại.
- Không tải lại trang khi video đạt ngưỡng hoàn thành; mở nút bài tiếp theo và cập nhật số bài đã xong.
- Phần trăm khóa học tính theo số bài hoàn thành; vị trí xem dở lưu riêng. Không thay đổi ngưỡng xem video 80%.
- Dùng bảng tiến độ hiện có, không cần chạy thêm SQL cho thay đổi này.
- Kiểm tra: build thành công; 11 kiểm tra JavaScript mô phỏng sự kiện phát/tạm dừng/tua/lưu/nối lại phiên. Chưa xác minh trên CSDL đang chạy do kết nối SQL bị từ chối.
