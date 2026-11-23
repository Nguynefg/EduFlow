(() => {
  const actions = {
    'toggle-theme': () => window.toggleTheme(),
    'toggle-admin-theme': () => window.toggleAdminTheme(),
    'toggle-admin-sidebar': () => window.toggleAdminSidebar(),
    'close-auth': () => window.closeAuthModal(),
    'auth-login': () => window.switchAuthMode('login'),
    'auth-register': () => window.switchAuthMode('register')
  };
  document.addEventListener('click', event => {
    const target = event.target.closest('[data-ui-action]');
    if (target && actions[target.dataset.uiAction]) actions[target.dataset.uiAction]();
  });
})();
