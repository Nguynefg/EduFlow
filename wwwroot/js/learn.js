// Trang học: lưu vị trí, gửi tiến độ thực xem lên server, tự hoàn thành khi đạt 80%.
(function () {
    function icons() { if (window.lucide) window.lucide.createIcons(); }

    // ---------- Tab Tổng quan / Tài liệu / Giảng viên ----------
    var tabs = document.querySelectorAll('[data-tab]');
    tabs.forEach(function (tab) {
        tab.addEventListener('click', function () {
            tabs.forEach(function (t) { t.classList.toggle('is-active', t === tab); t.setAttribute('aria-selected', t === tab); });
            document.querySelectorAll('[data-tab-panel]').forEach(function (p) { p.hidden = p.dataset.tabPanel !== tab.dataset.tab; });
        });
    });

    // ---------- Cập nhật tiến độ trên trang ----------
    function setPercent(percent) {
        var p = Math.round(percent);
        document.querySelectorAll('[data-course-percent]').forEach(function (el) { el.textContent = p + '%'; });
        document.querySelectorAll('[data-course-ring]').forEach(function (el) { el.style.setProperty('--p', p); });
        document.querySelectorAll('[data-course-bar]').forEach(function (el) { el.style.width = p + '%'; });
    }

    var form = document.querySelector('[data-complete-form]');
    var video = document.querySelector('[data-lesson-video]');
    var frame = document.getElementById('lessonYoutube');
    if (form && (video || frame)) {
        var tokenField = form.querySelector('[name="__RequestVerificationToken"]');
        if (!tokenField) return;
        var token = tokenField.value;
        var lessonId = form.querySelector('[name="lessonId"]').value;
        var status = document.querySelector('[data-watch-status]');
        var saveState = document.querySelector('[data-save-state]');
        var saveButton = document.querySelector('[data-save-now]');
        var session = null, busy = false, queued = false, queuedSeek = false;
        var seeking = false, discontinuity = false, last = null, yt = null, ready = !!video;
        var retryAt = 0, stopped = false, completed = form.dataset.initialCompleted === 'true';
        var resume = parseInt(form.dataset.resume || '0', 10), resuming = resume > 0;
        function position() {
            var value = video ? video.currentTime : yt && yt.getCurrentTime ? yt.getCurrentTime() : 0;
            return Number.isFinite(value) ? Math.max(0, Math.floor(value)) : 0;
        }
        function isPlaying() { return !document.hidden && !seeking && (video ? !video.paused && !video.ended : yt && yt.getPlayerState() === 1); }
        function time(value) { return Math.floor(value / 60) + ':' + String(value % 60).padStart(2, '0'); }
        function message(text, failed) { if (saveState) { saveState.textContent = text; saveState.classList.toggle('is-error', !!failed); } }
        function unlockNext() {
            var disabled = document.querySelector('[data-next-locked]');
            if (disabled && form.dataset.nextUrl) {
                var link = document.createElement('a'); link.className = 'lr-btn'; link.href = form.dataset.nextUrl; link.textContent = 'Bài tiếp'; disabled.replaceWith(link);
            }
            var row = document.querySelector('[data-outline-lesson="' + lessonId + '"]');
            if (row) row.classList.add('is-done');
            var next = form.dataset.nextId && document.querySelector('[data-outline-lesson="' + form.dataset.nextId + '"]');
            if (next) {
                next.classList.remove('is-locked');
                var item = next.querySelector('[data-lesson-url]');
                if (item) {
                    var anchor = document.createElement('a'); anchor.className = 'lr-item'; anchor.href = item.dataset.lessonUrl; anchor.innerHTML = item.innerHTML;
                    var icon = anchor.querySelector('svg,i'); if (icon) { var replacement = document.createElement('i'); replacement.dataset.lucide = 'circle-play'; icon.replaceWith(replacement); }
                    item.replaceWith(anchor); icons();
                }
            }
        }
        async function send(url, values) {
            var data = new FormData(); data.append('__RequestVerificationToken', token);
            Object.keys(values).forEach(function (key) { data.append(key, values[key]); });
            var response = await fetch(url, {method:'POST', body:data, keepalive:true});
            var type = response.headers.get('content-type') || '';
            if (response.redirected || response.status === 401 || response.status === 403 || (response.ok && type.indexOf('application/json') < 0)) {
                stopped = true; throw new Error('Phiên đăng nhập đã thay đổi. Hãy đăng nhập và mở lại bài học.');
            }
            if (!response.ok) { var error = new Error('Chưa lưu được tiến độ. Hệ thống sẽ thử lại; bạn có thể bấm Lưu tiến độ.'); error.sessionChanged = response.status === 409; throw error; }
            return response.json();
        }
        async function tick(mode) {
            if (!ready || (stopped && mode !== 'manual')) return;
            if (mode === 'manual') { stopped = false; retryAt = 0; }
            if (mode === 'seek') discontinuity = true;
            if (busy) { queued = true; queuedSeek = queuedSeek || mode === 'seek'; return; }
            if (Date.now() < retryAt) return;
            var pos = position();
            if (resuming && pos === 0 && resume > 0) return;
            resuming = false;
            var sample = {position:pos, at:performance.now(), playing:isPlaying()};
            if (!session && document.hidden) return;
            if (mode === 'poll' && session && last && !sample.playing && !last.playing && pos === last.position) return;
            busy = true;
            if (saveButton) saveButton.disabled = true;
            try {
                if (!session) {
                    var started = await send(form.dataset.startUrl, {lessonId:lessonId, position:pos});
                    session = started.session; last = sample; discontinuity = false;
                    message('Đã kết nối theo dõi. Tiến độ được lưu tự động.');
                } else {
                    var elapsed = last ? (sample.at-last.at)/1000 : 0;
                    var delta = last ? pos-last.position : 0;
                    // So sánh với thời gian thực, không coi mọi khoảng hơn 8 giây là tua.
                    var watched = !!last && last.playing && !discontinuity && delta >= 0 && delta <= elapsed*1.25+1;
                    var result = await send(form.dataset.positionUrl, {session:session, position:pos, playing:watched});
                    last = sample; discontinuity = false;
                    setPercent(result.percent);
                    if (status) status.textContent = result.completed ? 'Bài học đã hoàn thành · tiến độ đã được lưu.' : 'Đã xem ' + Math.min(100,Math.floor(result.watched*100/Math.max(1,result.duration))) + '% video · cần 80% để hoàn thành bài.';
                    message('Đã lưu vị trí ' + time(result.position) + ' · ' + new Date(result.savedAt).toLocaleTimeString('vi-VN'));
                    if (window.EduFlowLearningChanged) window.EduFlowLearningChanged();
                    document.querySelectorAll('[data-course-done]').forEach(function (el) { el.textContent = result.completedCount; });
                    if (result.completed && !completed) { completed = true; unlockNext(); }
                }
            } catch (error) {
                if (error.sessionChanged) { session = null; last = null; retryAt = 0; queued = true; message('Đang kết nối lại phiên học...'); }
                else { retryAt = Date.now()+10000; message(error.message || 'Mất kết nối. Tiến độ sẽ được lưu lại khi có mạng.',true); }
            } finally {
                busy = false; if (saveButton) saveButton.disabled = false;
                if (queued && !document.hidden) { var reset = queuedSeek; queued = false; queuedSeek = false; tick(reset ? 'seek' : 'poll'); }
            }
        }
        if (video) {
            function restore() { if (resume>0 && resume<video.duration) video.currentTime=resume; else resuming=false; }
            video.addEventListener('loadedmetadata',restore,{once:true}); if(video.readyState>=1)restore();
            video.addEventListener('play',function(){tick('poll');});
            video.addEventListener('seeking',function(){seeking=true;discontinuity=true;});
            video.addEventListener('seeked',function(){seeking=false;tick('seek');});
            video.addEventListener('pause',function(){tick('pause');});
            video.addEventListener('ended',function(){tick('ended');});
            video.addEventListener('ratechange',function(){if(video.playbackRate>1)video.playbackRate=1;});
        } else {
            var previousReady=window.onYouTubeIframeAPIReady;
            function mount(){yt=new YT.Player('lessonYoutube',{events:{onReady:function(){ready=true;if(resume>0)yt.seekTo(resume,true);else resuming=false;tick('seek');},onStateChange:function(){tick('poll');},onPlaybackRateChange:function(){if(yt.getPlaybackRate()>1)yt.setPlaybackRate(1);}}});}
            if(window.YT&&window.YT.Player)mount();else{window.onYouTubeIframeAPIReady=function(){if(previousReady)previousReady();mount();};var script=document.createElement('script');script.src='https://www.youtube.com/iframe_api';document.head.appendChild(script);}
        }
        if(saveButton)saveButton.addEventListener('click',function(){tick('manual');});
        setInterval(function(){tick('poll');},2000);
        document.addEventListener('visibilitychange',function(){if(document.hidden){if(video)video.pause();else if(yt&&yt.pauseVideo)yt.pauseVideo();}tick('pause');});
        window.addEventListener('pagehide',function(){tick('pause');});
        window.addEventListener('online',function(){retryAt=0;tick('manual');});
    }
// ---------- Bài kiểm tra: đếm câu đã trả lời, hỏi lại khi còn câu trống ----------
    var quiz = document.querySelector('[data-quiz-form]');
    if (quiz) {
        var countEl = quiz.querySelector('[data-quiz-count]');
        var questions = quiz.querySelectorAll('.lr-question');
        function answered() {
            return Array.prototype.filter.call(questions, function (q) {
                var radio = q.querySelector('input[type=radio]:checked');
                var text = q.querySelector('input[type=text]');
                return radio || (text && text.value.trim());
            }).length;
        }
        function update() { if (countEl) countEl.textContent = 'Đã trả lời ' + answered() + '/' + questions.length + ' câu'; }
        quiz.addEventListener('input', update);
        quiz.addEventListener('change', function (e) {
            if (e.target.type === 'radio') {
                e.target.closest('.lr-options').querySelectorAll('.lr-option').forEach(function (o) { o.classList.remove('is-picked'); });
                e.target.closest('.lr-option').classList.add('is-picked');
            }
            update();
        });
        quiz.addEventListener('submit', function (e) {
            var left = questions.length - answered();
            if (left > 0 && !confirm('Còn ' + left + ' câu chưa trả lời. Vẫn nộp bài?')) e.preventDefault();
        });
        update();
    }
})();
