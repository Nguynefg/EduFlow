// Hộp chọn ảnh / video từ Thư viện media của giảng viên.
//   MediaPicker.open({ kind: 'image' | 'video' | null, onPick: function (item) {} })
//   Nút khai báo sẵn: <button type="button" data-media-pick="image" data-media-target="#oInput">
//   Summernote: $('#el').summernote(MediaPicker.editorOptions({ height: 300 }))
(function () {
    var dialog, grid, status, tabs, uploadInput, progress, state = { kind: null, onPick: null, items: [] };

    function token() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function el(tag, cls, text) {
        var e = document.createElement(tag);
        if (cls) e.className = cls;
        if (text != null) e.textContent = text;
        return e;
    }

    function icon(name) {
        var i = document.createElement('i');
        i.setAttribute('data-lucide', name);
        return i;
    }

    function refreshIcons() { if (window.lucide) window.lucide.createIcons(); }

    function size(bytes) {
        if (bytes < 1024 * 1024) return Math.max(1, Math.round(bytes / 1024)) + ' KB';
        return (bytes / 1024 / 1024).toFixed(1) + ' MB';
    }

    function build() {
        dialog = el('dialog', 'media-dialog');
        dialog.innerHTML =
            '<div class="media-dialog-head">' +
            '  <div><h2>Thư viện media</h2><p>Chọn file có sẵn hoặc tải file mới lên.</p></div>' +
            '  <button type="button" class="media-close" aria-label="Đóng"><i data-lucide="x"></i></button>' +
            '</div>' +
            '<div class="media-dialog-bar">' +
            '  <div class="media-tabs" role="tablist">' +
            '    <button type="button" data-kind="">Tất cả</button>' +
            '    <button type="button" data-kind="image"><i data-lucide="image"></i>Ảnh</button>' +
            '    <button type="button" data-kind="video"><i data-lucide="film"></i>Video</button>' +
            '  </div>' +
            '  <label class="button small media-upload"><i data-lucide="upload"></i>Tải lên' +
            '    <input type="file" multiple accept=".jpg,.jpeg,.png,.webp,.gif,.mp4,.webm" hidden /></label>' +
            '</div>' +
            '<div class="media-progress" hidden><span></span></div>' +
            '<p class="media-status" role="status"></p>' +
            '<div class="media-grid"></div>';
        document.body.appendChild(dialog);

        grid = dialog.querySelector('.media-grid');
        status = dialog.querySelector('.media-status');
        tabs = dialog.querySelectorAll('.media-tabs button');
        uploadInput = dialog.querySelector('.media-upload input');
        progress = dialog.querySelector('.media-progress');

        dialog.querySelector('.media-close').addEventListener('click', function () { dialog.close(); });
        dialog.addEventListener('click', function (e) { if (e.target === dialog) dialog.close(); });
        tabs.forEach(function (b) {
            b.addEventListener('click', function () { load(b.dataset.kind || null); });
        });
        uploadInput.addEventListener('change', function () { upload(uploadInput.files); uploadInput.value = ''; });

        // Kéo thả file vào hộp thoại để tải lên
        dialog.addEventListener('dragover', function (e) { e.preventDefault(); dialog.classList.add('is-drop'); });
        dialog.addEventListener('dragleave', function (e) { if (e.target === dialog) dialog.classList.remove('is-drop'); });
        dialog.addEventListener('drop', function (e) {
            e.preventDefault(); dialog.classList.remove('is-drop');
            if (e.dataTransfer.files.length) upload(e.dataTransfer.files);
        });
        refreshIcons();
    }

    function setTabs() {
        tabs.forEach(function (b) {
            var k = b.dataset.kind || null;
            b.classList.toggle('is-active', k === state.kind);
            // Khi chỉ được chọn một loại thì khóa tab loại kia
            b.disabled = state.locked && k !== state.kind;
        });
    }

    function load(kind) {
        state.kind = kind;
        setTabs();
        status.textContent = 'Đang tải...';
        grid.innerHTML = '';
        fetch('/Media/List' + (kind ? '?kind=' + kind : ''), { headers: { 'X-Requested-With': 'fetch' } })
            .then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
            .then(function (items) { state.items = items; render(); })
            .catch(function () { status.textContent = 'Không tải được thư viện. Hãy thử lại.'; });
    }

    function render() {
        grid.innerHTML = '';
        status.textContent = state.items.length ? state.items.length + ' file · bấm vào một file để chọn' : '';
        if (!state.items.length) {
            var empty = el('div', 'media-empty');
            empty.appendChild(icon('image-plus'));
            empty.appendChild(el('strong', null, 'Chưa có file nào'));
            empty.appendChild(el('span', null, 'Bấm "Tải lên" hoặc kéo thả file vào đây.'));
            grid.appendChild(empty);
            refreshIcons();
            return;
        }
        state.items.forEach(function (item) {
            var card = el('button', 'media-card');
            card.type = 'button';
            card.title = item.displayName;
            var thumb = el('span', 'media-thumb');
            if (item.kind === 'image') {
                var img = el('img'); img.src = item.url; img.alt = ''; img.loading = 'lazy';
                thumb.appendChild(img);
            } else {
                var v = el('video'); v.src = (item.previewUrl || item.url) + '#t=0.5'; v.muted = true; v.preload = 'metadata';
                thumb.appendChild(v);
                var play = el('span', 'media-play'); play.appendChild(icon('play'));
                thumb.appendChild(play);
            }
            card.appendChild(thumb);
            var meta = el('span', 'media-meta');
            meta.appendChild(el('b', null, item.displayName));
            meta.appendChild(el('small', null, (item.kind === 'image' ? 'Ảnh' : 'Video') + ' · ' + size(item.size)));
            card.appendChild(meta);
            card.addEventListener('click', function () {
                dialog.close();
                if (state.onPick) state.onPick(item);
            });
            grid.appendChild(card);
        });
        refreshIcons();
    }

    // Tải file lên bằng XHR để có thanh tiến trình (video có thể lớn)
    function upload(files) {
        if (!files || !files.length) return;
        var data = new FormData();
        for (var i = 0; i < files.length; i++) data.append('files', files[i]);
        data.append('__RequestVerificationToken', token());

        var bar = progress.querySelector('span');
        progress.hidden = false; bar.style.width = '0%';
        status.textContent = 'Đang tải lên...';

        var xhr = new XMLHttpRequest();
        xhr.open('POST', '/Media/Upload');
        xhr.setRequestHeader('X-Requested-With', 'fetch');
        xhr.upload.onprogress = function (e) {
            if (e.lengthComputable) bar.style.width = Math.round(e.loaded / e.total * 100) + '%';
        };
        xhr.onload = function () {
            progress.hidden = true;
            var res = null;
            try { res = JSON.parse(xhr.responseText); } catch (e) { }
            if (xhr.status !== 200 || !res) { status.textContent = 'Tải lên thất bại. File có thể quá lớn.'; return; }
            load(state.locked ? state.kind : null);
            if (res.errors.length) setTimeout(function () { status.textContent = res.errors.join(' '); }, 400);
        };
        xhr.onerror = function () { progress.hidden = true; status.textContent = 'Mất kết nối khi tải lên.'; };
        xhr.send(data);
    }

    // Tải ảnh trực tiếp (dán / kéo ảnh vào trình soạn thảo) rồi trả về danh sách link
    function uploadFiles(files) {
        var data = new FormData();
        for (var i = 0; i < files.length; i++) data.append('files', files[i]);
        data.append('__RequestVerificationToken', token());
        return fetch('/Media/Upload', { method: 'POST', body: data, headers: { 'X-Requested-With': 'fetch' } })
            .then(function (r) { return r.json(); });
    }

    function open(options) {
        if (!dialog) build();
        state.onPick = options.onPick;
        state.locked = !!options.kind;
        dialog.showModal();
        load(options.kind || null);
    }

    function videoNode(item) {
        var v = document.createElement('video');
        v.src = item.previewUrl || item.url;
        v.controls = true;
        v.setAttribute('preload', 'metadata');
        v.style.maxWidth = '100%';
        return v;
    }

    // Cấu hình Summernote: thêm nút "Thư viện" và tải ảnh lên thư viện thay vì nhúng base64
    function editorOptions(extra) {
        var LibraryButton = function (context) {
            var ui = $.summernote.ui;
            return ui.button({
                contents: '<i class="note-icon-picture"></i> Thư viện',
                tooltip: 'Chèn ảnh / video từ thư viện media',
                click: function () {
                    context.invoke('editor.saveRange');
                    open({
                        kind: null,
                        onPick: function (item) {
                            context.invoke('editor.restoreRange');
                            if (item.kind === 'image') context.invoke('editor.insertImage', item.url, function ($img) {
                                $img.attr('alt', item.displayName).css('max-width', '100%');
                            });
                            else context.invoke('editor.insertNode', videoNode(item));
                        }
                    });
                }
            }).render();
        };

        var options = {
            lang: 'vi-VN',
            height: 300,
            toolbar: [
                ['style', ['style']],
                ['font', ['bold', 'italic', 'underline', 'strikethrough', 'clear']],
                ['color', ['forecolor', 'backcolor']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['table', ['table']],
                ['insert', ['link', 'library', 'hr']],
                ['view', ['fullscreen', 'codeview']]
            ],
            buttons: { library: LibraryButton },
            callbacks: {
                onImageUpload: function (files) {
                    var $editor = $(this);
                    uploadFiles(files).then(function (res) {
                        res.items.forEach(function (item) {
                            if (item.kind === 'image') $editor.summernote('insertImage', item.url);
                            else $editor.summernote('insertNode', videoNode(item));
                        });
                        if (res.errors.length) alert(res.errors.join('\n'));
                    }).catch(function () { alert('Không tải được ảnh lên.'); });
                }
            }
        };
        return Object.assign(options, extra || {});
    }

    // Nút chọn file khai báo bằng thuộc tính data-media-pick
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-media-pick]');
        if (!btn) return;
        e.preventDefault();
        open({
            kind: btn.dataset.mediaPick || null,
            onPick: function (item) {
                var target = btn.dataset.mediaTarget && document.querySelector(btn.dataset.mediaTarget);
                if (target) { target.value = item.url; target.dispatchEvent(new Event('change', { bubbles: true })); }
                btn.dispatchEvent(new CustomEvent('media:picked', { bubbles: true, detail: item }));
            }
        });
    });

    window.MediaPicker = { open: open, editorOptions: editorOptions };
})();

// Xem trước video đang có trong trình soạn thảo qua endpoint của chủ sở hữu.
(() => {
    function refresh(root) {
        if (!(root instanceof Element)) return;
        const videos = root.matches('video') ? [root] : root.querySelectorAll('video');
        videos.forEach(video => {
            if (!video.closest('.note-editable')) return;
            const source = video.getAttribute('src') || '';
            if (source.startsWith('/uploads/media/')) video.src = '/Media/Video?url=' + encodeURIComponent(source);
        });
    }
    const observer = new MutationObserver(records => records.forEach(record => {
        if (record.type === 'attributes') refresh(record.target);
        else record.addedNodes.forEach(refresh);
    }));
    observer.observe(document.body, {subtree:true, childList:true, attributes:true, attributeFilter:['src']});
})();
