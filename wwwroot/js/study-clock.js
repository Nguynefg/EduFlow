(() => {
    const form = document.querySelector('[data-study-clock]');
    if (!form) return;
    const token = form.querySelector('[name="__RequestVerificationToken"]').value;
    let session = null, busy = false;
    async function tick() {
        if (busy) return;
        const active = !document.hidden && document.hasFocus();
        if (!session && !active) return;
        busy = true;
        try {
            const url = session ? '/StudyClock/Tick' : '/StudyClock/Start';
            const data = new URLSearchParams({__RequestVerificationToken:token});
            if (session) { data.set('session',session); data.set('active',String(active)); }
            else data.set('lessonId',form.dataset.studyClock);
            const response = await fetch(url,{method:'POST',body:data,keepalive:true});
            if (!response.ok) { session = null; return; }
            const result = await response.json(); if (result.session) session = result.session;
        } catch (_) { session = null; }
        finally { busy = false; }
    }
    tick();setInterval(tick,15000);
    document.addEventListener('visibilitychange',tick);window.addEventListener('focus',tick);window.addEventListener('blur',tick);
})();
