(() => {
  let theme;
  try { theme = localStorage.getItem('eduflow_admin_theme') || localStorage.getItem('eduflow_theme'); } catch {}
  const dark = theme === 'dark' || (theme !== 'light' && window.matchMedia('(prefers-color-scheme: dark)').matches);
  document.documentElement.classList.toggle('dark', dark);
  document.documentElement.style.colorScheme = dark ? 'dark' : 'light';
})();
