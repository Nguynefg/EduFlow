(() => {
    const form = document.getElementById('payoutForm');
    if (!form) return;
    const amount = form.querySelector('[name="amount"]');
    const voluntary = document.getElementById('voluntaryTax');
    const money = value => new Intl.NumberFormat('vi-VN').format(value) + ' đ';
    function preview() {
        const gross = Number(amount.value);
        const valid = amount.value.trim() !== '' && Number.isSafeInteger(gross) && gross > 0;
        const rate = gross >= Number(form.dataset.taxThreshold) || voluntary.checked ? Number(form.dataset.taxRate) : 0;
        const tax = Math.round(gross * rate / 100);
        document.getElementById('taxPreview').textContent = valid ? money(tax) : '—';
        document.getElementById('netPreview').textContent = valid ? money(gross - tax) : '—';
    }
    amount.addEventListener('input', preview);
    voluntary.addEventListener('change', preview);
    preview();
})();
