# Thuế thù lao cá nhân

Phạm vi: cá nhân cư trú nhận thù lao, không HĐLĐ hoặc HĐLĐ dưới 3 tháng. Không áp dụng cho giảng viên kinh doanh, cá nhân không cư trú hoặc HĐLĐ từ 3 tháng. Cam kết tạm chưa khấu trừ cần kế toán xử lý ngoài luồng; không có nút miễn thuế tùy ý.

Nguồn: Khoản 2 Điều 50 Nghị định 253/2026/NĐ-CP, hiệu lực 01/07/2026. Từ 5 triệu đồng/lần khấu trừ 10% toàn bộ thù lao trước thuế; dưới ngưỡng chỉ khấu trừ nếu cá nhân yêu cầu. Không chia nhỏ một lần chi để tránh khấu trừ. https://xaydungchinhsach.chinhphu.vn/quy-dinh-moi-ve-khau-tru-thue-thu-nhap-ca-nhan-119260703150410707.htm

Sao lưu CSDL, dừng app, chạy 05_live_payments.sql rồi 06_personal_tax.sql trên đúng EduFlowDB trước khi chạy lại. Migration giữ nguyên dữ liệu cũ; thuế NULL nghĩa chưa xác định, không phải miễn thuế. Yêu cầu cũ thiếu hồ sơ phải từ chối/gửi lại, khoản đã chi phải đối soát với kế toán, không ghi đè tự động.

Yêu cầu rút lưu bản chụp tên, MST/định danh, loại đối tượng và lựa chọn tự nguyện. Thuế dự kiến tính trên khoản thù lao yêu cầu rút (phần giảng viên sau chia doanh thu, trước thuế), không tính lại trên cả tiền học viên trả. Admin kiểm tra hợp đồng và số tiền thực sự chi trong một lần, không gom/tách khoản chuyển ngoài hệ thống làm sai ngưỡng. Hợp đồng phải quy định thù lao là GROSS, nếu thỏa thuận NET cần tính gross-up bên ngoài và đối soát trước, luồng này chưa hỗ trợ gross-up.

Admin duyệt xác nhận đối tượng trước khi khóa số thuế. Khi chuyển khoản chỉ chuyển thực nhận = trước thuế - thuế. Khi từ chối trả lại toàn bộ số tiền đã giữ, không cộng thuế vào phần nền tảng giữ. Thuế được làm tròn VND nguyên, nửa đồng làm tròn lên.

Mỗi giảng viên chỉ có một yêu cầu tiền thật đang chờ/đã duyệt. Không cho đổi số tiền/định danh hoặc quy tắc của khoản đã trả. Chưa có tự động áp dụng ngoại lệ, quyết toán năm hay chính sách tương lai; kế toán cần rà soát khi pháp luật đổi.

Sổ thuế lọc theo ngày chuyển khoản thực tế trên biên lai do admin nhập; xuất CSV nội bộ, theo dõi thuế giữ và thuế đã ghi nhận nộp. Admin thực hiện nộp thuế bên ngoài, sau đó nhập ngày và mã chứng từ; đây là ghi nhận thủ công, không gọi API ngân hàng/cơ quan thuế, không phải chứng từ khấu trừ điện tử. Dữ liệu demo/sandbox không cấp quyền rút tiền thật. Chưa tính thuế GTGT/TNDN của nền tảng, chưa tích hợp hóa đơn/chứng từ thuế điện tử, chưa tự nộp tờ khai/quyết toán.

