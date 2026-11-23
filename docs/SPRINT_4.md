# Sprint 4 Đánh giá và hỏi đáp

Chạy docs/07_sprint4.sql trên đúng EduFlowDB trước khi dùng. Hai bảng course_reviews/discussions đã có trong SQL gốc sẽ được giữ nguyên. Bảng phụ discussion_votes lưu từng tài khoản đã upvote, khóa kép chống bình chọn trùng. Không xóa dữ liệu cũ. Chưa tự áp dụng migration trên máy.

Trang chi tiết khóa học có phần đánh giá, phân bố 1–5 sao, điểm trung bình, sửa nhận xét và phản hồi giảng viên. Điều kiện 30% tính từ số bài đã hoàn thành thuộc khóa hiện tại và quyền ghi danh còn hiệu lực, không tin phần trăm client gửi lên. Mỗi học viên có đúng một đánh giá mỗi khóa. Sửa đánh giá bị ẩn không tự hiện lại; điểm trung bình chỉ tính đánh giá công khai. Admin kiểm duyệt tại /Admin/Community.

Trang học có tab Hỏi đáp. Người chưa mua, bị thu hồi quyền học hoặc chưa mở khóa bài không thể đọc/gửi/trả lời/bình chọn qua URL trực tiếp. Giảng viên sở hữu/admin quản lý tại /Community/Manage mà không phải mua khóa của mình. Câu trả lời nằm dưới một câu hỏi của cùng bài, không tạo lồng vô hạn. Tìm kiếm cả câu hỏi và câu trả lời đang được phép xem; phân trang, ghim ưu tiên trên cùng. Mốc video phải trong thời lượng video. Ẩn câu hỏi che toàn bộ câu trả lời với học viên. Chỉ giảng viên sở hữu/admin được ghim, đánh dấu giải quyết, ẩn hoặc hiện. Upvote là bật/tắt, không tự vote nội dung mình.

Mọi thao tác ghi dùng POST và antiforgery. Văn bản đánh giá/hỏi đáp được Razor encode, không render HTML người dùng. Giới hạn độ dài ở server. Quyền admin kiểm tra lại từ CSDL; không tin cờ quản trị do form gửi.

Kiểm chứng: 67 kiểm tra đã qua trên SQLite trong bộ nhớ và HTTP TestHost (nghiệp vụ, phân quyền, render Razor, chống giả mạo và encode HTML). Không ghi vào EduFlowDB thật. Khóa đồng thời và script migration SQL Server chưa được chạy kiểm chứng trên CSDL đang sử dụng. Các file kiểm tra tạm đã được xóa.

