// Đồng hồ chỉ để hiển thị; server luôn tự kiểm tra hạn thanh toán khi xác nhận.
(function () {
    var label = document.querySelector('[data-qr-seconds]');
    if (!label) return;
    var end = Date.now() + Number(label.dataset.qrSeconds) * 1000;
    function update() {
        var seconds = Math.max(0, Math.ceil((end - Date.now()) / 1000));
        var minutes = Math.floor(seconds / 60);
        label.textContent = seconds > 0 ? 'Mã còn hiệu lực ' + minutes + ':' + String(seconds % 60).padStart(2, '0') : 'Đơn đã hết hạn. Vui lòng quay lại giỏ hàng để tạo đơn mới.';
        if (seconds === 0) {
            document.querySelector('[data-demo-confirm]').disabled = true;
            document.querySelector('.qr-frame').hidden = true;
            clearInterval(timer);
        }
    }
    var timer = setInterval(update, 1000);
    update();
})();
