# Sprint 3 — Mua khóa học và theo dõi học tập

Đã bổ sung giỏ hàng, mã giảm giá, đơn hàng, kết nối cổng thanh toán, yêu cầu rút tiền và học tuần tự. Mã mới dùng cùng cấu trúc Controller → Service → Model như Sprint 1–2.

## Chạy dự án

```powershell
cd "F:\Zalo Data\Công nghệ phần mềm\EduFlow"
dotnet restore
dotnet run
```

Mở http://localhost:5178. Trên máy hiện tại, script bổ sung Sprint 3 đã được áp dụng vào EduFlowDB. Nếu cài dự án ở máy khác hoặc khôi phục lại bản backup cũ, chạy thêm:

```powershell
sqlcmd -S . -E -C -d EduFlowDB -f 65001 -i docs\04_sprint3.sql
```

Script chỉ bổ sung bảng/cột/index khi còn thiếu, không xóa dữ liệu. Các bảng mới `video_watch_sessions`, `video_watch_segments` lưu phiên và các khoảng video đã xem. Các cột snapshot ở `payout_requests` giữ lại ngân hàng tại thời điểm yêu cầu; `payments.payment_url` giữ liên kết của cùng một giao dịch.

## Các trang mới

| Vai trò | Đường dẫn | Chức năng |
|---|---|---|
| Học viên | /Cart | Thêm/xóa khóa học, áp mã giảm, tạo đơn nhiều khóa |
| Học viên | /Orders | Lịch sử đơn hàng |
| Học viên | /Orders/Details/{id} | Chi tiết, chọn cổng, mở lại thanh toán, hỏi trạng thái cổng |
| Giảng viên | /Finance | Số dư khả dụng, yêu cầu rút tiền, lịch sử và biên lai |
| Admin | /Admin/Coupons | Tạo, sửa điều kiện khi chưa dùng, bật/tắt mã giảm giá |
| Admin | /Admin/Finance | Duyệt/từ chối yêu cầu, lưu biên lai sau khi chuyển khoản |
| Admin | /Admin/Finance/Orders | Đối soát 200 đơn gần nhất |

## Luồng mua khóa học

1. Học viên thêm các khóa vào giỏ. Không thêm khóa của mình hoặc khóa đã đăng ký.
2. Có thể nhập coupon phần trăm hoặc số tiền. Server kiểm tra thời gian, tổng lượt và mức giảm tối đa.
3. Khi tạo đơn, server chốt giá và chia phần giảm giá cho từng khóa. Hoa hồng tính trên giá sau giảm.
4. Đơn có tổng tiền 0 được ghi danh ngay. Đơn trả phí giữ trạng thái chờ, chưa cộng tiền cho giảng viên.
5. Cổng thanh toán xác nhận thành công: server kiểm tra chữ ký, mã đơn, số tiền, rồi cấp quyền học và cộng thu nhập trong cùng transaction.
6. Callback được gửi lại nhiều lần cũng chỉ cấp quyền và cộng tiền một lần.

Đơn chưa khởi tạo thanh toán có hiệu lực 30 phút và có thể hủy. Đơn đã gửi sang cổng không hủy tùy ý vì có thể đang thu tiền; người dùng có thể mở lại cùng liên kết hoặc bấm **Kiểm tra với cổng thanh toán**. Nếu cổng chưa xác định được kết quả, giữ nguyên đơn để đối soát, không tự giả định thành công. Lượt coupon của giao dịch này vẫn được giữ.

Không có chức năng hoàn tiền theo đặc tả. Luồng tự đánh dấu trả tiền khi đăng ký đã được gỡ bỏ. Chế độ mô phỏng riêng ở Development yêu cầu người dùng bấm xác nhận rõ ràng; giao dịch được ghi nhãn `demo`. Các đơn demo cũ trong backup vẫn giữ nguyên.

## Thanh toán mô phỏng để demo đồ án

Khi chạy Development, ppsettings.Development.json bật Payments:EnableDemo. Mở một đơn đang chờ, chưa hết hạn và chưa gửi tới cổng thật, rồi bấm **Xác nhận thanh toán mô phỏng**. Hệ thống ghi giao dịch demo, cấp quyền học và cộng số dư thử nghiệm đúng một lần. Không gọi ngân hàng, không trừ tiền thật. Dữ liệu mô phỏng dùng cho demo cục bộ; không dùng số dư thử nghiệm để đối soát tiền thật.

Production luôn chặn chức năng này, kể cả khi đặt flag thành true. Để tắt trên máy phát triển, đặt flag trong cấu hình Development thành false. Đơn đã khởi tạo giao dịch cổng thật không được chuyển sang mô phỏng. Đơn hết hạn cần tạo lại từ giỏ hàng.

## Cấu hình sandbox

Sandbox là môi trường thử của nhà cung cấp, dùng tài khoản/thẻ thử và không thu tiền thật. Mã đã có adapter VNPay, MoMo, ZaloPay; chưa có thông tin merchant của bạn nên chưa thể chạy thử một giao dịch thật với hệ thống sandbox của họ.

Các endpoint sandbox có sẵn trong `appsettings.json`. Cần điền:

| Cấu hình | Giá trị cần cung cấp |
|---|---|
| Payments:PublicBaseUrl | URL HTTPS công khai của ứng dụng, để cổng gọi callback |
| Payments:vnpay | TmnCode, HashSecret |
| Payments:momo | PartnerCode, AccessKey, SecretKey |
| Payments:zalopay | AppId, Key1, Key2 |

Dùng `dotnet user-secrets` hoặc biến môi trường, không ghi secret thật vào mã nguồn. Ví dụ:

```powershell
dotnet user-secrets set "Payments:PublicBaseUrl" "https://your-test-domain.example"
dotnet user-secrets set "Payments:vnpay:TmnCode" "YOUR_TMN_CODE"
dotnet user-secrets set "Payments:vnpay:HashSecret" "YOUR_SECRET"
```

Development đọc user-secrets theo UserSecretsId đã có trong project. Cổng nào thiếu cấu hình sẽ không xuất hiện ở màn hình chọn thanh toán. Cần đăng ký callback với merchant portal:

- VNPay IPN: `https://your-domain/Payment/VnpayIpn`
- MoMo IPN: `https://your-domain/Payment/MomoIpn`
- ZaloPay callback: `https://your-domain/Payment/ZaloIpn`
- Trang quay về: `https://your-domain/Payment/Return`

Trang quay về chỉ hiển thị hướng dẫn kiểm tra đơn; không dùng query của trình duyệt để cấp quyền học. Khi chuyển production, đổi endpoint và dùng đúng thông tin merchant production.

Tài liệu chính thức dùng cho adapter:

- VNPay thanh toán: https://sandbox.vnpayment.vn/apis/docs/thanh-toan-pay/pay.html
- VNPay đối soát: https://sandbox.vnpayment.vn/apis/docs/truy-van-hoan-tien/querydr%26refund.html
- MoMo: https://developers.momo.vn/v3/docs/payment/api/wallet/onetime/
- MoMo đối soát: https://developers.momo.vn/v3/docs/payment/api/payment-api/query/
- ZaloPay: https://docs.zalopay.vn/docs/specs/order-create/
- ZaloPay callback: https://docs.zalopay.vn/docs/developer-tools/knowledge-base/callback/
- ZaloPay đối soát: https://developer.zalopay.vn/v2/general/overview.html

## Luồng rút tiền

Giảng viên gửi yêu cầu trong số dư khả dụng. Số tiền được giữ lại ngay để không thể tạo nhiều yêu cầu vượt số dư. Admin duyệt, chuyển khoản ngân hàng bên ngoài hệ thống rồi tải biên lai JPG/PNG/WebP lên và xác nhận đã trả. Từ chối yêu cầu phải có lý do và trả số tiền giữ lại đúng một lần. Biên lai lưu ngoài wwwroot, chỉ chủ yêu cầu và Admin có thể tải.

## Luồng học tập

- Mở bài đầu tiên chưa hoàn thành. Các bài sau bị khóa kể cả khi gọi URL trực tiếp.
- Video: HTML5 và YouTube gửi vị trí định kỳ. Server so sánh với thời gian thực, bỏ qua thao tác tua và gộp các khoảng đã xem để không đếm trùng. Đủ 80% thời lượng thì tự hoàn thành.
- Mỗi học viên chỉ có một phiên xem hoạt động cho một bài. Mở tab mới sẽ thay thế phiên cũ.
- Bài viết: học viên bấm đánh dấu hoàn thành.
- Quiz: chấm ở server; đạt điểm tối thiểu mới hoàn thành, giữ điểm cao nhất.
- Vị trí và phần trăm lưu vào CSDL để học tiếp trên thiết bị khác.

Thời lượng video phải lớn hơn 0. Với video cũ bị thiếu thời lượng, giảng viên cần sửa bài và lưu thời lượng đúng. Cơ chế theo dõi chặn tua và gửi tiến độ quá nhanh; không phải DRM hay cơ chế chứng minh người học thực sự chú ý vào màn hình.

## Các file chính để đọc và thuyết trình

| File | Nhiệm vụ |
|---|---|
| Models/Entities/Commerce.cs | Entity giỏ hàng, coupon, rút tiền và phiên xem |
| Models/ViewModels/CommerceViewModels.cs | Dữ liệu các trang Sprint 3 |
| Services/CommerceService.cs | Giỏ hàng, đơn, chốt giá, cấp quyền sau thanh toán |
| Services/PaymentGatewayService.cs | Tạo yêu cầu, kiểm tra chữ ký và hỏi trạng thái từng cổng |
| Services/FinanceService.cs | Quản lý coupon và nghiệp vụ rút tiền |
| Services/VideoWatchService.cs | Kiểm tra thời gian xem và tự hoàn thành video |
| Services/LearningService.cs | Học tuần tự, bài viết và quiz |
| Controllers/CartController.cs | Nhận thao tác giỏ hàng |
| Controllers/OrdersController.cs | Lịch sử và chi tiết đơn |
| Controllers/PaymentController.cs | Chuyển cổng và nhận callback |
| Controllers/FinanceController.cs | Giảng viên yêu cầu rút tiền, tải biên lai |
| Areas/Admin/Controllers/CouponsController.cs | Admin quản lý mã giảm |
| Areas/Admin/Controllers/FinanceController.cs | Admin duyệt rút tiền, đối soát đơn |
| docs/04_sprint3.sql | Bổ sung CSDL an toàn khi chạy lại |

Quy ước giữ như source cũ: service trả `string?` để báo lỗi; `null` là thành công. Controller nhận form, gọi service, gán TempData và trả View/Redirect. Các constructor viết rõ trường `private readonly` và không thêm repository hay framework nghiệp vụ khác.

## Kiểm thử và tích hợp thanh toán

Bộ test trong source đã được xóa theo yêu cầu. Hướng dẫn cập nhật CSDL, giới hạn demo/sandbox và chuẩn bị VNPay thật: xem THANH_TOAN_VNPAY.md. Các số kiểm tra bên dưới là kết quả lịch sử tại thời điểm thực hiện.

### Chọn khóa trong giỏ và QR mô phỏng
- Đánh dấu từng khóa hoặc Chọn tất cả. Tổng tiền và mã giảm giá được tính lại theo lựa chọn.
- Nút Thanh toán các khóa đã chọn chỉ tạo đơn cho những khóa đã đánh dấu; các khóa còn lại giữ trong giỏ.
- Trong môi trường Development với Payments:EnableDemo=true, đơn có phí mở trang QR mô phỏng. Khóa miễn phí được đăng ký ngay.
- QR được tạo cục bộ, chứa mã đơn và số tiền demo, không có tài khoản ngân hàng hay yêu cầu chuyển tiền. Quét QR không tự xác nhận giao dịch.
- Bấm Xác nhận thanh toán mô phỏng để hoàn tất demo và mở quyền học. Đơn có hiệu lực 30 phút; server kiểm tra lại hạn và người sở hữu.
- Nếu khóa đã có đơn chờ, mở đơn đó hoặc bỏ chọn khóa đó để mua khóa khác.
- Kiểm chứng: 119 kiểm tra tự động qua; QR PNG đã đọc lại đúng mã đơn/số tiền demo; hai tệp JavaScript qua kiểm tra cú pháp.
- Sau khi cập nhật, dừng ứng dụng bằng Ctrl+C rồi chạy lại dotnet run trong thư mục EduFlow.


### Quyền admin quản lý khóa học
Admin vào Quản trị → Quản lý khóa học → chọn khóa → Sửa thông tin hoặc Quản lý nội dung. Nút Tạo và chỉnh sửa khóa học mở danh sách toàn bộ khóa, cho phép tạo mới và sửa khóa của mọi giảng viên. Admin được quản lý chương, bài học, tài liệu, quiz và dùng thư viện media của mình; các thao tác duyệt, ẩn, hiện và xóa mềm vẫn có trong trang quản trị. Sửa khóa không đổi giảng viên sở hữu. Quyền admin được kiểm tra tại service từ CSDL; giảng viên vẫn bị giới hạn theo chủ sở hữu. Không cần thay đổi cấu trúc CSDL. Đã qua 146 kiểm tra tự động, gồm lưu form bằng HTTP và kiểm tra chặn truy cập trái quyền.


### Trình sửa ngay trong khu vực admin
Danh sách Quản lý khóa học có nút Sửa thông tin và Quản lý nội dung trên từng dòng. Form tạo/sửa, chương, bài học và quiz dùng đường dẫn /Admin/Instructor/... và /Admin/Curriculum/..., giữ sidebar và menu quản trị. Lưu thông tin hoặc chương xong vẫn ở khu vực admin. Dùng chung xử lý và kiểm tra dữ liệu với trình giảng viên để tránh hai bộ logic khác nhau. Đã qua 159 kiểm tra, gồm render sidebar, địa chỉ form, lưu dữ liệu và chặn giảng viên truy cập trang admin.


### Mô phỏng đầy đủ tài chính
Theo cấu hình Development hiện tại, mua QR mô phỏng được tính vào doanh thu/số dư mô phỏng; có luồng rút tiền, thuế và biên nhận mô phỏng. Xem MO_PHONG_TAI_CHINH.md. Không chuyển tiền thật và không thay đổi số dư của sổ tiền thật.

