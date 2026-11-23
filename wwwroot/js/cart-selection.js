// Gửi lựa chọn lên server để tính lại tổng tiền và mã giảm giá.
(function () {
    var form = document.getElementById('cartForm');
    if (!form) return;
    var all = form.querySelector('[data-select-all]');
    var picks = form.querySelectorAll('[data-course-pick]');
    var review = document.getElementById('reviewSelection');

    function updateSelection() {
        form.requestSubmit(review);
    }
    picks.forEach(function (input) {
        input.addEventListener('change', updateSelection);
    });
    all.addEventListener('change', function () {
        picks.forEach(function (input) { input.checked = all.checked; });
        updateSelection();
    });
    var selected = Array.from(picks).filter(function (input) { return input.checked; }).length;
    all.indeterminate = selected > 0 && selected < picks.length;
})();
