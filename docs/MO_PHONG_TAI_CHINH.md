# Mô phỏng toàn bộ tài chính

Chạy Development, Payments:Mode=Sandbox và Payments:EnableDemo=true (đã cấu hình sẵn). Không yêu cầu merchant hay tên miền. Mọi bước dưới đây là mô phỏng, không dùng tài khoản ngân hàng thật.

1. Học viên mua khóa có phí, mở QR rồi xác nhận thanh toán mô phỏng. Mỗi đơn chỉ ghi nhận thành công một lần.
2. Giảng viên của khóa đó mở Doanh thu: tự hiển thị sổ mô phỏng. Phần giảng viên và nền tảng theo tỷ lệ đã lưu khi mua (mặc định 70/30). Số dư tính từ đơn đã thanh toán, trừ khoản đang giữ và đã trả trong cùng sổ mô phỏng. Khóa miễn phí không tạo doanh thu.
3. Giảng viên mở Rút tiền, điền số tiền trước thuế, định danh mẫu hợp lệ và xác nhận loại thù lao. Vẫn kiểm tra KYC/ngân hàng, số dư và yêu cầu đang xử lý để quy trình giống thật. Có thể dùng thông tin giả hợp lệ về định dạng, không cần ngân hàng thật.
4. Admin vào Duyệt rút tiền, xác nhận đã kiểm tra hồ sơ rồi duyệt. Sau đó Xác nhận đã chi trả mô phỏng, không cần tải biên lai ngân hàng. Giảng viên/admin có thể tải biên nhận TXT đánh dấu MÔ PHỎNG rõ ràng.
5. Thuế và tiền thực nhận tính theo quy tắc thù lao hiện có. Sổ thuế hiển thị các khoản mô phỏng đã chi; dùng SIM-TAX mã yêu cầu để xác nhận nộp thuế mô phỏng và xuất CSV có cột chế độ.
6. Từ chối trả lại toàn bộ phần giữ của yêu cầu. Bấm lặp không cộng/trừ tiền thêm. Không thay đổi cờ IsTest để giả làm giao dịch thật.

Số dư mô phỏng không ghi vào cache Balance tiền thật. Tách hai sổ bằng is_test trên payment/payout. Production hoặc khi tắt EnableDemo không xử lý yêu cầu mô phỏng. Trong Development mô phỏng, cổng thanh toán ngoài bị ẩn, chỉ dùng QR cục bộ.

Cần đã chạy docs/05_live_payments.sql và docs/06_personal_tax.sql (hoặc file gộp EduFlow-cap-nhat-thue.sql) trước khi chạy bản mới. Lần sửa mô phỏng này không thêm cột. Yêu cầu cũ thiếu hồ sơ thuế cần từ chối rồi tạo lại; không tự sửa khoản cũ. Đơn phải Paid và có payment success mới tăng doanh thu; đơn đang chờ không tính. Khoảng ngày báo cáo theo ngày tạo đơn.
