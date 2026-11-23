// Form bài học: xem trước video và tự lấy thời lượng.
// Nguồn video: file chọn từ máy (ưu tiên, giống server) > link nhập tay / chọn từ thư viện.
//  - File .mp4/.webm hoặc link trực tiếp: đọc thời lượng từ thẻ <video> (loadedmetadata)
//  - Link YouTube: dùng YouTube IFrame API để lấy getDuration()
(function () {
    var box = document.querySelector('[data-video-box]');
    if (!box) return;

    var stage = box.querySelector('[data-video-stage]');
    var empty = box.querySelector('[data-video-empty]');
    var sourceEl = box.querySelector('[data-video-source] span');
    var durationEl = box.querySelector('[data-video-duration]');
    var urlInput = document.getElementById('videoUrl');
    var initialUrl = urlInput.value.trim();
    var fileInput = document.getElementById('videoFile');
    var clearBtn = document.querySelector('[data-video-clear-file]');
    var durationText = document.getElementById('durationText');
    var durationSeconds = document.getElementById('durationSeconds');
    var hint = document.querySelector('[data-duration-hint]');

    var objectUrl = null, ytPlayer = null, renderId = 0;

    // ---------- Thời lượng ----------
    function format(sec) {
        sec = Math.max(0, Math.round(sec));
        var h = Math.floor(sec / 3600), m = Math.floor(sec % 3600 / 60), s = sec % 60;
        var pad = function (n) { return (n < 10 ? '0' : '') + n; };
        return h > 0 ? h + ':' + pad(m) + ':' + pad(s) : m + ':' + pad(s);
    }

    // "12:30" -> 750, "1:02:03" -> 3723, "15" -> 15 phút
    function parse(text) {
        text = (text || '').trim();
        if (!text) return 0;
        if (/^\d+$/.test(text)) return parseInt(text, 10) * 60;
        var parts = text.split(':').map(function (p) { return parseInt(p, 10); });
        if (parts.some(isNaN)) return null;
        return parts.reduce(function (total, p) { return total * 60 + p; }, 0);
    }

    function setDuration(sec, auto) {
        sec = Math.round(sec);
        durationSeconds.value = sec;
        durationText.value = sec > 0 ? format(sec) : '';
        durationEl.hidden = !(sec > 0);
        durationEl.querySelector('span').textContent = format(sec);
        if (auto) {
            hint.textContent = 'Đã tự lấy thời lượng từ video.';
            hint.classList.add('is-auto');
            durationText.classList.add('is-auto');
            setTimeout(function () { durationText.classList.remove('is-auto'); }, 1200);
        }
    }

    durationText.addEventListener('input', function () {
        var sec = parse(durationText.value);
        durationText.classList.toggle('is-invalid', sec === null);
        if (sec !== null) {
            durationSeconds.value = sec;
            hint.textContent = 'Đã sửa tay. Định dạng phút:giây (vd 12:30).';
            hint.classList.remove('is-auto');
        }
    });
    durationText.addEventListener('blur', function () {
        var sec = parse(durationText.value);
        if (sec !== null) setDuration(sec, false);
    });

    // ---------- Xem trước ----------
    function youtubeId(url) {
        var m = (url || '').match(/(?:youtube\.com\/(?:watch\?v=|embed\/|shorts\/)|youtu\.be\/)([A-Za-z0-9_-]{11})/);
        return m ? m[1] : null;
    }

    function reset() {
        renderId++;
        if (ytPlayer && ytPlayer.destroy) { try { ytPlayer.destroy(); } catch (e) { } }
        ytPlayer = null;
        stage.querySelectorAll('video, iframe, [data-yt], .lesson-video-error').forEach(function (n) { n.remove(); });
        stage.classList.remove('is-loading');
    }

    function showEmpty(text) {
        reset();
        empty.hidden = false;
        sourceEl.textContent = text || '—';
        durationEl.hidden = true;
    }

    function showError(message) {
        stage.classList.remove('is-loading');
        var err = document.createElement('div');
        err.className = 'lesson-video-error';
        err.textContent = message;
        stage.appendChild(err);
    }

    function playFile(src, label, autoDuration) {
        reset();
        empty.hidden = true;
        sourceEl.textContent = label;
        stage.classList.add('is-loading');
        var id = renderId;
        var video = document.createElement('video');
        video.controls = true;
        video.preload = 'metadata';
        video.playsInline = true;
        video.src = src;
        video.addEventListener('loadedmetadata', function () {
            if (id !== renderId) return;
            stage.classList.remove('is-loading');
            if (isFinite(video.duration) && video.duration > 0 && autoDuration) setDuration(video.duration, true);
            else if (isFinite(video.duration)) { durationEl.hidden = false; durationEl.querySelector('span').textContent = format(video.duration); }
        });
        video.addEventListener('error', function () {
            if (id !== renderId) return;
            video.remove();
            showError('Không phát được video này. Kiểm tra lại link hoặc định dạng (.mp4, .webm).');
        });
        stage.appendChild(video);
    }

    // Nạp YouTube IFrame API một lần
    var ytReady = null;
    function loadYouTubeApi() {
        if (ytReady) return ytReady;
        ytReady = new Promise(function (resolve) {
            if (window.YT && window.YT.Player) return resolve();
            var prev = window.onYouTubeIframeAPIReady;
            window.onYouTubeIframeAPIReady = function () { if (prev) prev(); resolve(); };
            var s = document.createElement('script');
            s.src = 'https://www.youtube.com/iframe_api';
            document.head.appendChild(s);
        });
        return ytReady;
    }

    function playYouTube(vid, autoDuration) {
        reset();
        empty.hidden = true;
        sourceEl.textContent = 'YouTube · ' + vid;
        stage.classList.add('is-loading');
        var id = renderId;
        var holder = document.createElement('div');
        holder.setAttribute('data-yt', '');
        stage.appendChild(holder);
        loadYouTubeApi().then(function () {
            if (id !== renderId) return;
            ytPlayer = new YT.Player(holder, {
                videoId: vid,
                playerVars: { rel: 0, modestbranding: 1 },
                events: {
                    onReady: function (e) {
                        if (id !== renderId) return;
                        stage.classList.remove('is-loading');
                        var tries = 0;
                        // getDuration() đôi khi trả 0 lúc đầu, thử lại vài lần
                        (function read() {
                            var d = e.target.getDuration();
                            if (d > 0) {
                                if (autoDuration) setDuration(d, true);
                                else { durationEl.hidden = false; durationEl.querySelector('span').textContent = format(d); }
                            } else if (tries++ < 10) setTimeout(read, 300);
                        })();
                    },
                    onError: function () {
                        if (id !== renderId) return;
                        showError('Video YouTube không tồn tại hoặc không cho phép nhúng.');
                    }
                }
            });
        });
    }

    // Quyết định nguồn video đang dùng rồi hiển thị
    function refresh(autoDuration) {
        if (objectUrl) { URL.revokeObjectURL(objectUrl); objectUrl = null; }
        var file = fileInput.files && fileInput.files[0];
        clearBtn.hidden = !file;
        if (file) {
            objectUrl = URL.createObjectURL(file);
            playFile(objectUrl, 'File tải lên · ' + file.name, autoDuration);
            return;
        }
        var url = urlInput.value.trim();
        if (!url) return showEmpty();
        var vid = youtubeId(url);
        if (vid) return playYouTube(vid, autoDuration);
        if (/^(https?:\/\/|\/)/i.test(url)) {
            var label = url.indexOf('/uploads/media/') === 0 ? 'Thư viện · ' + decodeURIComponent(url.split('/').pop()).replace(/^[0-9a-f]{8}-/, '') : url;
            var preview = url;
            if (box.dataset.lessonId && url === initialUrl && url.indexOf('/uploads/') === 0) preview = '/MediaStream/Lesson/' + box.dataset.lessonId;
            else if (url.indexOf('/uploads/media/') === 0) preview = '/Media/Video?url=' + encodeURIComponent(url);
            return playFile(preview, label, autoDuration);
        }
        showEmpty('Link chưa hợp lệ');
    }

    // Gõ link: đợi người dùng dừng gõ rồi mới tải
    var typing;
    urlInput.addEventListener('input', function () {
        clearTimeout(typing);
        typing = setTimeout(function () { refresh(true); }, 600);
    });
    urlInput.addEventListener('change', function () { clearTimeout(typing); refresh(true); });
    fileInput.addEventListener('change', function () { refresh(true); });
    clearBtn.addEventListener('click', function () {
        fileInput.value = '';
        var name = fileInput.closest('.studio-file').querySelector('[data-file-name]');
        if (name) name.textContent = 'Chọn file video từ máy';
        refresh(true);
    });

    // Mở trang: hiện video hiện tại; chỉ tự điền thời lượng nếu đang để trống
    var initial = parseInt(durationSeconds.value, 10) || 0;
    setDuration(initial, false);
    refresh(initial === 0);
})();
