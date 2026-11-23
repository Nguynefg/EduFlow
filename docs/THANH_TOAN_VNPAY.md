# Chuẩn bị tích hợp VNPay

Hiện chưa có merchant và tên miền HTTPS, nên chưa thể nhận tiền thật. Production mặc định tắt LiveEnabled và không chứa khóa bí mật.

## Cập nhật ngay trước khi chạy lại

1. Dừng ứng dụng.
2. Mở docs/05_live_payments.sql trong SSMS, kiểm tra đúng tên EduFlowDB rồi Execute. Script thêm is_test vào payments và payout_requests, giữ nguyên dữ liệu cũ và đánh dấu dữ liệu cũ là thử nghiệm/chưa đối soát. Sao lưu CSDL trước khi cập nhật.
3. Chạy lại ứng dụng. Nếu chưa chạy script, ứng dụng sẽ lỗi thiếu cột.

Demo trong Development mở quyền học và cộng vào sổ doanh thu/số dư mô phỏng để thực hành rút tiền, thuế và chi trả. Không cộng vào sổ tiền thật. Xem MO_PHONG_TAI_CHINH.md. Số dư rút được tính từ giao dịch thật thành công, trừ yêu cầu rút tiền thật đang giữ hoặc đã chi. Yêu cầu rút tiền cũ chưa đối soát bị chặn chi trả.

## Khi có merchant và HTTPS

- Dùng merchant sandbox để kiểm tra trước; cấu hình Mode=Sandbox, URL HTTPS công khai, TmnCode và HashSecret sandbox.
- Đăng ký IPN URL: https://ten-mien/Payment/VnpayIpn. Return URL: https://ten-mien/Payment/Return. Return URL chỉ hiển thị kết quả, không tự cấp quyền học.
- Kiểm tra thực tế thành công, thất bại, hủy, sai chữ ký, sai tiền, callback lặp, callback đến muộn và tra cứu trạng thái trên SQL Server.
- Production dùng ASPNETCORE_ENVIRONMENT=Production. Đặt khóa bằng biến môi trường hoặc kho bí mật, không đưa vào git hay chat. Ví dụ tên biến: Payments__vnpay__TmnCode, Payments__vnpay__HashSecret, Payments__vnpay__Endpoint, Payments__vnpay__QueryEndpoint, Payments__PublicBaseUrl.
- Endpoint và QueryEndpoint Production lấy từ hồ sơ tích hợp VNPay được cấp. Host phải là pay.vnpay.vn; sandbox dùng sandbox.vnpayment.vn. Không dùng khóa sandbox cho Live.
- Đối soát thủ công giao dịch và yêu cầu rút cũ. Không cập nhật hàng loạt is_test=0. Chỉ bật Payments__LegacyDataReviewed=true sau khi đã xác minh dữ liệu cũ; đây là xác nhận vận hành, không tự chuyển loại dữ liệu.
- Chỉ sau nghiệm thu và đối soát mới bật Payments__LiveEnabled=true với Mode=Live. Thiếu các điều kiện này, hệ thống không tạo thanh toán Live.
- Nếu có reverse proxy, cấu hình Hosting:TrustedProxies bằng IP proxy tin cậy và chuyển tiếp HTTPS đúng. Không tin mọi proxy.

## Phạm vi và kiểm chứng

38 kiểm tra cục bộ đã qua, gồm chữ ký, sai tiền, callback lặp, trạng thái chờ và tách số dư thật/thử nghiệm. Kiểm tra dùng SQLite trong bộ nhớ, không gửi yêu cầu thanh toán ra ngoài. Chưa nghiệm thu merchant sandbox, khóa đồng thời SQL Server hoặc triển khai HTTPS.

Rút tiền vẫn là quy trình yêu cầu và admin đối soát/xác nhận thủ công, chưa có API tự chuyển tiền ngân hàng. Đã bổ sung luồng khấu trừ thuế thù lao cá nhân, xem THUE_THU_LAO.md và chạy thêm 06_personal_tax.sql; chưa tự kê khai, nộp thuế hay cấp chứng từ thuế điện tử. MoMo và ZaloPay chưa mở Live.

Tài liệu giao thức VNPay: https://sandbox.vnpayment.vn/apis/docs/thanh-toan-pay/pay.html


