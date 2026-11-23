(() => {
    const root = document.querySelector('.student-dashboard');
    if (!root) return;
    const cards = [...root.querySelectorAll('[data-study-state]')];
    const buttons = [...root.querySelectorAll('[data-study-filter]')];
    const search = root.querySelector('[data-study-search]');
    const empty = root.querySelector('[data-study-empty]');
    let filter = 'all';
    function normalize(value) { return value.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g,'').replace(/đ/g,'d'); }
    function update() {
        const query = normalize(search.value.trim()); let visible = 0;
        cards.forEach(card => { const show = (filter === 'all' || card.dataset.studyState === filter) && normalize(card.dataset.studyName).includes(query); card.hidden = !show; if (show) visible++; });
        if (empty) empty.hidden = visible > 0 || cards.length === 0;
    }
    buttons.forEach(button => button.addEventListener('click', () => { filter = button.dataset.studyFilter; buttons.forEach(item => { const active = item === button; item.classList.toggle('is-active',active); item.setAttribute('aria-pressed',String(active)); }); update(); }));
    search?.addEventListener('input',update);
    let stamp = null;
    try { stamp = localStorage.getItem('eduflow:learning-updated'); } catch (_) { }
    function refreshIfChanged() { try { if (localStorage.getItem('eduflow:learning-updated') !== stamp) location.reload(); } catch (_) { } }
    window.addEventListener('pageshow',event => { if (event.persisted) location.reload(); });
    window.addEventListener('focus',refreshIfChanged);
    document.addEventListener('visibilitychange',() => { if (!document.hidden) refreshIfChanged(); });
})();
