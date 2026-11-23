(() => {
    if (location.hash === '#qa') {
        const tab = document.querySelector('[data-tab="qa"]');
        if (tab) { tab.click(); document.getElementById('qa')?.scrollIntoView({block:'start'}); }
    }
})();
