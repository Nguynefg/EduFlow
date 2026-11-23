(() => {
 const admin = document.body.classList.contains('admin-page');
 const themeKey = admin ? 'eduflow_admin_theme' : 'eduflow_theme';
 function icons() { if (window.lucide) window.lucide.createIcons(); }
 function themeLabels() {
  const dark = document.documentElement.classList.contains('dark');
  const text = document.getElementById('themeToggleText'); if (text) text.textContent = dark ? 'Sáng' : 'Tối';
  const icon = document.getElementById('themeToggleIcon'); if (icon) icon.setAttribute('data-lucide', dark ? 'sun' : 'moon');
  const adminText = document.getElementById('adminThemeText'); if (adminText) adminText.textContent = dark ? 'Tối' : 'Sáng';
  const adminIcon = document.getElementById('adminThemeIcon'); if (adminIcon) adminIcon.setAttribute('data-lucide', dark ? 'sun' : 'moon');
  document.documentElement.style.colorScheme = dark ? 'dark' : 'light';
  document.querySelectorAll('[data-ui-action="toggle-admin-theme"]').forEach(button => button.setAttribute('aria-pressed', String(dark)));
  icons();
 }
 window.toggleTheme = window.toggleAdminTheme = () => { const dark = document.documentElement.classList.toggle('dark'); try { localStorage.setItem(themeKey, dark ? 'dark' : 'light'); } catch {} themeLabels(); };
 window.toggleAdminSidebar = () => { document.getElementById('adminSidebar')?.classList.toggle('-translate-x-full'); document.getElementById('sidebarBackdrop')?.classList.toggle('hidden'); };
 document.addEventListener('keydown', event => { if (event.key === 'Escape') { document.querySelectorAll('.account-menu[open]').forEach(x => x.removeAttribute('open')); document.getElementById('adminSidebar')?.classList.add('-translate-x-full'); document.getElementById('sidebarBackdrop')?.classList.add('hidden'); } });
 document.addEventListener('click', event => { document.querySelectorAll('.account-menu[open]').forEach(x => { if (!x.contains(event.target)) x.removeAttribute('open'); }); });
 document.querySelectorAll('form[data-confirm]').forEach(form => form.addEventListener('submit', event => { if (!confirm(form.dataset.confirm)) event.preventDefault(); }));
 themeLabels();
})();
(() => {
 const dialog = document.getElementById('authModal');
 let opener;
 const passwordFields = Array.from(document.querySelectorAll('[data-password-toggle]')).flatMap(button => {
   const input = document.getElementById(button.getAttribute('aria-controls'));
   if (!input || !button.closest(".auth-password-field")?.contains(input)) return [];
   const setVisible = visible => {
     input.type = visible ? 'text' : 'password';
     button.toggleAttribute('data-password-visible', visible);
     button.setAttribute('aria-label', (visible ? 'Ẩn ' : 'Hiện ') + button.dataset.passwordLabel);
   };
   button.addEventListener('click', () => setVisible(input.type === 'password'));
   button.hidden = false;
   return [() => setVisible(false)];
 });
 const hidePasswords = () => passwordFields.forEach(hide => hide());
 if (!dialog) return;
 window.switchAuthMode = mode => { hidePasswords(); const login = mode === 'login'; document.getElementById('modalLoginForm').classList.toggle('hidden', !login); document.getElementById('modalRegisterForm').classList.toggle('hidden', login); document.getElementById('authModalTitle').textContent = login ? 'Đăng nhập EduFlow' : 'Tạo tài khoản EduFlow'; document.querySelectorAll('[data-auth-tab]').forEach(tab => { tab.classList.toggle('bg-white', tab.dataset.authTab === mode); tab.classList.toggle('text-blue-600', tab.dataset.authTab === mode); }); };
 window.closeAuthModal = () => dialog.close();
 document.querySelectorAll('[data-open-auth]').forEach(link => link.addEventListener('click', event => { event.preventDefault(); opener = link; window.switchAuthMode(link.dataset.openAuth); dialog.showModal(); }));
 dialog.addEventListener('close', () => { hidePasswords(); opener?.focus(); });
 dialog.addEventListener('click', event => { if (event.target === dialog) { const rect = dialog.getBoundingClientRect(); if (event.clientX < rect.left || event.clientX > rect.right || event.clientY < rect.top || event.clientY > rect.bottom) dialog.close(); } });
})();
