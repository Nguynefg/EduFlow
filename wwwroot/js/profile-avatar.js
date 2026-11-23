// Xem trước ảnh trên máy; chỉ tải lên khi bấm Lưu thay đổi.
(function () {
    var box = document.querySelector('[data-avatar-upload]');
    if (!box) return;
    var input = box.querySelector('input[type=file]');
    var image = box.querySelector('[data-avatar-preview]');
    var placeholder = box.querySelector('[data-avatar-placeholder]');
    var filename = box.querySelector('[data-avatar-filename]');
    var choose = box.querySelector('[data-avatar-choose]');
    var reset = box.querySelector('[data-avatar-reset]');
    var original = image.getAttribute('src') || '';
    var previewUrl = null;
    input.hidden = true;
    choose.hidden = false;
    choose.addEventListener('click', function () { input.click(); });
    function restore() {
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        previewUrl = null;
        image.setAttribute('src', original);
        image.hidden = !original;
        placeholder.hidden = !!original;
        input.value = '';
        reset.hidden = true;
        filename.textContent = 'Ảnh vuông sẽ hiển thị đẹp nhất.';
    }
    reset.addEventListener('click', restore);
    input.addEventListener('change', function () {
        var file = input.files[0];
        if (!file) return;
        if (!/\.(jpe?g|png|webp)$/i.test(file.name) || file.size > 5 * 1024 * 1024) {
            restore();
            filename.textContent = 'Vui lòng chọn ảnh JPG, PNG hoặc WebP tối đa 5 MB.';
            return;
        }
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        previewUrl = URL.createObjectURL(file);
        image.src = previewUrl;
        image.hidden = false;
        placeholder.hidden = true;
        filename.textContent = file.name;
        reset.hidden = false;
    });
})();
