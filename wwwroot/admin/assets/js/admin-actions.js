// Nút icon mở hộp thoại nhập lý do (dùng cho Từ chối hồ sơ giảng viên)
// <button data-reason="idHopThoai" data-value="id cần xử lý" data-name="tên hiển thị">
document.addEventListener('click', function (event) {
    var button = event.target.closest('[data-reason]');
    if (button) {
        if (window.hideTooltip) window.hideTooltip();
        var dialog = document.getElementById(button.dataset.reason);
        dialog.querySelector('[data-target]').value = button.dataset.value;
        var name = dialog.querySelector('[data-name]');
        if (name) name.textContent = button.dataset.name || '';
        dialog.querySelector('textarea').value = '';
        dialog.showModal();
        return;
    }
    // Nút "Hủy" trong hộp thoại
    var close = event.target.closest('[data-dialog-close]');
    if (close) close.closest('dialog').close();
});
