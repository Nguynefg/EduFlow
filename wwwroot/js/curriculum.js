for (const form of document.querySelectorAll("[data-order]")) {
    const list = form.querySelector("[data-items]");
    let dragged;
    list.addEventListener("dragstart", event => {
        dragged = event.target.closest("[data-id]");
        if (dragged) event.dataTransfer.setData("text/plain", dragged.dataset.id);
    });
    list.addEventListener("dragend", () => { dragged = null; });
    list.addEventListener("dragover", event => {
        if (dragged && event.target.closest("[data-id]")) event.preventDefault();
    });
    list.addEventListener("drop", event => {
        const target = event.target.closest("[data-id]");
        if (!dragged || !target || target === dragged) return;
        event.preventDefault();
        const box = target.getBoundingClientRect();
        if (event.clientY > box.top + box.height / 2) target.after(dragged);
        else target.before(dragged);
    });
    list.addEventListener("click", event => {
        const button = event.target.closest("[data-move]");
        if (!button) return;
        const row = button.closest("[data-id]");
        if (button.dataset.move === "-1") row.previousElementSibling?.before(row);
        else row.nextElementSibling?.after(row);
        button.focus();
    });
    form.addEventListener("submit", async event => {
        event.preventDefault();
        const data = new FormData(form);
        for (const row of list.children) data.append("ids", row.dataset.id);
        const status = form.querySelector("[role=status]");
        const button = form.querySelector("button:not([type])");
        button.disabled = true;
        try {
            const response = await fetch(form.action, { method: "POST", body: data });
            if (response.status !== 204) throw new Error();
            location.reload();
        } catch {
            status.textContent = "Không lưu được thứ tự. Tải lại trang và thử lại.";
            button.disabled = false;
        }
    });
}
