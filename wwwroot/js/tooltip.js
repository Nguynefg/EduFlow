// Chú thích nổi cho mọi phần tử có data-tip (dùng chung cho Admin và Studio giảng viên).
// Gắn vào <body> với position: fixed nên không bị khung cuộn (overflow) che khuất,
// tự lật xuống dưới khi phía trên không đủ chỗ và không tràn ra mép màn hình.
(function () {
    var tip = null, owner = null;

    function getTip() {
        if (!tip) {
            tip = document.createElement('div');
            tip.className = 'ui-tooltip';
            tip.setAttribute('role', 'tooltip');
            document.body.appendChild(tip);
        }
        return tip;
    }

    function show(el) {
        var text = el.getAttribute('data-tip');
        if (!text) return;
        var t = getTip();
        owner = el;
        t.textContent = text;
        t.classList.remove('is-below');
        t.classList.add('is-visible');

        var r = el.getBoundingClientRect(), gap = 8, margin = 8;
        var w = t.offsetWidth, h = t.offsetHeight;
        var top = r.top - h - gap;
        if (top < margin) { top = r.bottom + gap; t.classList.add('is-below'); }
        var center = r.left + r.width / 2;
        var left = Math.min(Math.max(center - w / 2, margin), window.innerWidth - w - margin);
        t.style.top = top + 'px';
        t.style.left = left + 'px';
        t.style.setProperty('--arrow-x', (center - left) + 'px');
    }

    function hide() {
        if (tip) tip.classList.remove('is-visible');
        owner = null;
    }

    function find(target) { return target && target.closest ? target.closest('[data-tip]') : null; }

    document.addEventListener('mouseover', function (e) {
        var el = find(e.target);
        if (el && el !== owner) show(el);
        else if (!el && owner) hide();
    });
    document.addEventListener('focusin', function (e) { var el = find(e.target); if (el) show(el); });
    document.addEventListener('focusout', hide);
    document.addEventListener('mousedown', hide);
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') hide(); });
    window.addEventListener('scroll', hide, true);
    window.addEventListener('resize', hide);
    window.hideTooltip = hide;
})();
