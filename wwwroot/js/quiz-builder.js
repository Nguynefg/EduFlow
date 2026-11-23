// Trình soạn câu hỏi bài kiểm tra (trang Thêm / Sửa bài học).
// - Bài kiểm tra đã lưu: lưu / sửa / xóa / nhân bản / sắp xếp bằng AJAX — server trả về HTML danh sách mới (_QuizQuestions).
// - Bài chưa lưu (chế độ nháp): câu hỏi giữ trên trang, ghi JSON vào ô ẩn DraftQuestionsJson và lưu cùng bài học.
(function () {
    var editor = document.querySelector('[data-quiz-editor]');
    if (!editor) return;

    var container = document.querySelector('[data-quiz-container]');
    var rowsBox = editor.querySelector('[data-options]');
    var addBtn = editor.querySelector('[data-add-option]');
    var countEl = editor.querySelector('[data-options-count]');
    var labelEl = editor.querySelector('[data-options-label]');
    var errorEl = editor.querySelector('[data-quiz-error]');
    var titleEl = editor.querySelector('[data-quiz-editor-title]');
    var cancelBtn = editor.querySelector('[data-quiz-cancel]');
    var submitBtn = editor.querySelector('[data-quiz-submit]');
    var textEl = editor.querySelector('textarea[name="QuestionText"]');
    var idEl = editor.querySelector('input[name="Id"]');
    var lessonId = editor.dataset.lesson;
    var base = editor.dataset.base;                      // /Curriculum/Questions/{courseId}
    var MAX = 6, LETTERS = 'ABCDEF';
    var isDraft = editor.dataset.draft === 'true';
    var lessonForm = document.getElementById('lessonForm');
    var draftInput = document.querySelector('[data-quiz-drafts]');
    var drafts = [], seq = 0;                            // chế độ nháp: [{ id, type, text, options, correct }]

    var state = { type: 'single', options: ['', '', '', ''], correct: 0 };

    function url(action) { return base.replace('/Questions/', '/' + action + '/'); }
    function token() { return editor.querySelector('input[name="__RequestVerificationToken"]').value; }
    function icons() { if (window.lucide) window.lucide.createIcons(); }
    function minRows() { return state.type === 'fill' ? 1 : 2; }

    // ---------- Vẽ các dòng lựa chọn ----------
    function readRows() {
        state.options = Array.prototype.map.call(rowsBox.querySelectorAll('input[type=text]'), function (i) { return i.value; });
        var checked = rowsBox.querySelector('input[type=radio]:checked');
        if (checked) state.correct = parseInt(checked.value, 10);
    }

    function renderRows(focusIndex) {
        rowsBox.innerHTML = '';
        var fill = state.type === 'fill';
        state.options.forEach(function (text, i) {
            var row = document.createElement('div');
            row.className = 'quiz-option-row' + (!fill && i === state.correct ? ' is-correct' : '');

            if (fill) {
                var ok = document.createElement('span');
                ok.className = 'quiz-row-mark';
                ok.innerHTML = '<i data-lucide="check"></i>';
                row.appendChild(ok);
            } else {
                var radioWrap = document.createElement('label');
                radioWrap.className = 'quiz-row-radio';
                radioWrap.title = 'Đánh dấu là đáp án đúng';
                var radio = document.createElement('input');
                radio.type = 'radio'; radio.name = 'CorrectIndex'; radio.value = i; radio.checked = i === state.correct;
                radio.setAttribute('aria-label', 'Đáp án đúng ' + LETTERS[i]);
                radio.addEventListener('change', function () { readRows(); renderRows(); });
                var letter = document.createElement('span');
                letter.textContent = LETTERS[i];
                radioWrap.appendChild(radio); radioWrap.appendChild(letter);
                row.appendChild(radioWrap);
            }

            var input = document.createElement('input');
            input.type = 'text'; input.name = 'Options[' + i + ']'; input.maxLength = 500; input.value = text;
            input.placeholder = fill ? (i === 0 ? 'Đáp án đúng' : 'Cách viết khác cũng chấp nhận') : 'Lựa chọn ' + LETTERS[i];
            input.addEventListener('keydown', function (e) {
                // Enter: sang dòng kế tiếp hoặc thêm dòng mới
                if (e.key === 'Enter' && !e.ctrlKey && !e.metaKey) {
                    e.preventDefault();
                    readRows();
                    if (i === state.options.length - 1 && state.options.length < MAX) { state.options.push(''); renderRows(i + 1); }
                    else { var next = rowsBox.querySelectorAll('input[type=text]')[i + 1]; if (next) next.focus(); }
                }
            });
            row.appendChild(input);

            var remove = document.createElement('button');
            remove.type = 'button'; remove.className = 'quiz-row-remove';
            remove.setAttribute('aria-label', 'Xóa dòng'); remove.dataset.tip = 'Xóa dòng';
            remove.innerHTML = '<i data-lucide="x"></i>';
            remove.disabled = state.options.length <= minRows();
            remove.addEventListener('click', function () {
                readRows();
                state.options.splice(i, 1);
                if (state.correct === i) state.correct = 0;
                else if (state.correct > i) state.correct--;
                renderRows(Math.min(i, state.options.length - 1));
            });
            row.appendChild(remove);
            rowsBox.appendChild(row);
        });

        addBtn.hidden = state.options.length >= MAX;
        addBtn.querySelector('span').textContent = fill ? 'Thêm cách viết được chấp nhận' : 'Thêm lựa chọn';
        labelEl.innerHTML = fill
            ? 'Đáp án đúng <small class="muted">— không phân biệt hoa thường, thêm các cách viết khác nếu có</small>'
            : 'Các lựa chọn <small class="muted">— chọn ô tròn ở đáp án đúng</small>';
        countEl.textContent = state.options.length + ' / ' + MAX;
        icons();
        if (focusIndex != null) { var f = rowsBox.querySelectorAll('input[type=text]')[focusIndex]; if (f) f.focus(); }
    }

    addBtn.addEventListener('click', function () {
        readRows();
        if (state.options.length < MAX) { state.options.push(''); renderRows(state.options.length - 1); }
    });

    editor.querySelectorAll('input[name="QuestionType"]').forEach(function (r) {
        r.addEventListener('change', function () {
            readRows();
            state.type = r.value;
            var filled = state.options.filter(function (o) { return o.trim(); });
            if (state.type === 'fill') {
                // Sang điền khuyết: giữ đáp án đúng làm đáp án chấp nhận
                var right = state.options[state.correct] || '';
                state.options = [right.trim() ? right : (filled[0] || '')];
            } else if (state.options.length < 2) {
                state.options = state.options.concat(['', '', '']).slice(0, 4);
                state.correct = 0;
            }
            renderRows();
        });
    });

    // ---------- Soạn mới / sửa ----------
    function setType(type) {
        state.type = type;
        editor.querySelector('input[name="QuestionType"][value="' + type + '"]').checked = true;
    }

    function resetEditor() {
        idEl.value = '';
        textEl.value = '';
        setType('single');
        state.options = ['', '', '', ''];
        state.correct = 0;
        titleEl.innerHTML = '<i data-lucide="plus-circle"></i>Thêm câu hỏi mới';
        cancelBtn.hidden = true;
        editor.classList.remove('is-editing');
        hideError();
        renderRows();
    }

    function loadQuestion(card) {
        var q = JSON.parse(card.dataset.question);
        idEl.value = q.id;
        textEl.value = q.text;
        setType(q.type === 'fill' ? 'fill' : 'single');
        state.options = q.options.length ? q.options.slice() : [''];
        while (state.type === 'single' && state.options.length < 2) state.options.push('');
        state.correct = q.correct || 0;
        var no = card.querySelector('.quiz-no').textContent;
        titleEl.innerHTML = '<i data-lucide="pencil"></i>Đang sửa ' + no.toLowerCase();
        cancelBtn.hidden = false;
        editor.classList.add('is-editing');
        hideError();
        renderRows();
        editor.scrollIntoView({ behavior: 'smooth', block: 'start' });
        setTimeout(function () { textEl.focus(); }, 300);
    }

    cancelBtn.addEventListener('click', resetEditor);

    function showError(msg) { errorEl.textContent = msg; errorEl.hidden = false; }
    function hideError() { errorEl.hidden = true; }

    function validate() {
        readRows();
        if (!textEl.value.trim()) { textEl.focus(); return 'Nhập nội dung câu hỏi.'; }
        var filled = state.options.filter(function (o) { return o.trim(); });
        if (state.type === 'fill') {
            if (!filled.length) return 'Nhập ít nhất 1 đáp án đúng.';
        } else {
            if (filled.length < 2) return 'Câu trắc nghiệm cần ít nhất 2 lựa chọn.';
            if (!(state.options[state.correct] || '').trim()) return 'Ô tròn đáp án đúng đang ở một lựa chọn trống.';
        }
        var lower = filled.map(function (o) { return o.trim().toLowerCase(); });
        if (lower.some(function (o, i) { return lower.indexOf(o) !== i; })) return 'Các lựa chọn không được trùng nhau.';
        return null;
    }

    // ---------- Gửi lên server ----------
    function post(action, data) {
        data.append('__RequestVerificationToken', token());
        data.append('LessonId', lessonId);   // khớp cả tham số lessonId lẫn model.LessonId
        container.classList.add('is-busy');
        return fetch(url(action), { method: 'POST', body: data, headers: { 'X-Requested-With': 'fetch' } })
            .then(function (r) {
                return r.text().then(function (body) {
                    container.classList.remove('is-busy');
                    if (!r.ok) throw new Error(body || 'Có lỗi xảy ra, hãy thử lại.');
                    container.innerHTML = body;
                    icons();
                    return body;
                });
            })
            .catch(function (err) { container.classList.remove('is-busy'); throw err; });
    }

    // ---------- Chế độ nháp (bài chưa lưu) ----------
    function esc(s) {
        return String(s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; });
    }

    // Bỏ dòng trống, giữ đúng vị trí đáp án đúng
    function makeDraft(id, type, text, options, correct) {
        var kept = [], newCorrect = 0;
        options.forEach(function (o, i) {
            if (!(o || '').trim()) return;
            if (i === correct) newCorrect = kept.length;
            kept.push(o.trim());
        });
        return { id: id || 'd' + (++seq), type: type === 'fill' ? 'fill' : 'single', text: text.trim(), options: kept, correct: type === 'fill' ? 0 : newCorrect };
    }

    function syncDrafts() {
        if (!draftInput) return;
        draftInput.value = drafts.length ? JSON.stringify(drafts.map(function (d) {
            return { QuestionText: d.text, QuestionType: d.type, Options: d.options, CorrectIndex: d.correct };
        })) : '';
    }

    function renderDrafts() {
        syncDrafts();
        var single = drafts.filter(function (d) { return d.type !== 'fill'; }).length;
        var passEl = lessonForm && lessonForm.querySelector('input[name="PassScore"]');
        var pass = passEl ? parseInt(passEl.value, 10) : NaN;
        var html = '<div class="quiz-summary">' +
            '<span><i data-lucide="list-checks"></i><b>' + drafts.length + '</b> câu hỏi chưa lưu</span>' +
            '<span><i data-lucide="circle-dot"></i>' + single + ' trắc nghiệm</span>' +
            '<span><i data-lucide="text-cursor-input"></i>' + (drafts.length - single) + ' điền khuyết</span>' +
            (drafts.length && !isNaN(pass) ? '<span class="quiz-summary-pass"><i data-lucide="target"></i>Cần đúng ≥ ' + Math.ceil(drafts.length * pass / 100) + ' / ' + drafts.length + ' câu để đạt ' + pass + '%</span>' : '') +
            '</div>';
        if (!drafts.length) {
            html += '<div class="quiz-empty"><i data-lucide="message-circle-question"></i><strong>Chưa có câu hỏi nào</strong><span>Soạn câu hỏi đầu tiên ở khung phía trên.</span></div>';
        }
        html += '<ol class="quiz-list" data-quiz-list>';
        drafts.forEach(function (d, n) {
            var fill = d.type === 'fill';
            html += '<li class="quiz-card" data-id="' + d.id + '" data-question="' + esc(JSON.stringify(d)) + '" draggable="true">' +
                '<div class="quiz-card-head">' +
                '<span class="curriculum-grip" aria-hidden="true"><i data-lucide="grip-vertical"></i></span>' +
                '<span class="quiz-no">Câu ' + (n + 1) + '</span>' +
                '<span class="quiz-type' + (fill ? ' is-fill' : '') + '">' + (fill ? 'Điền khuyết' : 'Trắc nghiệm') + '</span>' +
                '<div class="quiz-card-actions">' +
                '<button type="button" class="quiz-act" data-quiz-move="-1" data-tip="Lên" aria-label="Lên"' + (n === 0 ? ' disabled' : '') + '><i data-lucide="chevron-up"></i></button>' +
                '<button type="button" class="quiz-act" data-quiz-move="1" data-tip="Xuống" aria-label="Xuống"' + (n === drafts.length - 1 ? ' disabled' : '') + '><i data-lucide="chevron-down"></i></button>' +
                '<button type="button" class="quiz-act" data-quiz-edit data-tip="Sửa câu hỏi" aria-label="Sửa"><i data-lucide="pencil"></i></button>' +
                '<button type="button" class="quiz-act" data-quiz-duplicate data-tip="Nhân bản" aria-label="Nhân bản"><i data-lucide="copy"></i></button>' +
                '<button type="button" class="quiz-act is-danger" data-quiz-delete data-tip="Xóa câu hỏi" aria-label="Xóa"><i data-lucide="trash-2"></i></button>' +
                '</div></div>' +
                '<p class="quiz-text">' + esc(d.text) + '</p>';
            if (fill) {
                html += '<div class="quiz-fill-answers"><small>Đáp án chấp nhận:</small>' +
                    d.options.map(function (o) { return '<span><i data-lucide="check"></i>' + esc(o) + '</span>'; }).join('') + '</div>';
            } else {
                html += '<ul class="quiz-options">' + d.options.map(function (o, i) {
                    var ok = i === d.correct;
                    return '<li class="' + (ok ? 'is-correct' : '') + '"><span class="quiz-letter">' + LETTERS[i] + '</span>' +
                        '<span class="quiz-option-text">' + esc(o) + '</span>' + (ok ? '<i data-lucide="circle-check"></i>' : '') + '</li>';
                }).join('') + '</ul>';
            }
            html += '</li>';
        });
        html += '</ol>';
        container.innerHTML = html;
        icons();
    }

    function indexOfDraft(id) {
        for (var i = 0; i < drafts.length; i++) if (drafts[i].id === id) return i;
        return -1;
    }

    // Chạy thao tác: nháp thì sửa mảng tại chỗ, ngược lại gửi lên server
    function run(action, data, draftFn) {
        if (!isDraft) return post(action, data);
        try { draftFn(); } catch (err) { return Promise.reject(err); }
        renderDrafts();
        return Promise.resolve();
    }

    function saveDraftFromEditor(editingId) {
        var d = makeDraft(editingId, state.type, textEl.value, state.options, state.correct);
        var i = editingId ? indexOfDraft(editingId) : -1;
        if (i >= 0) drafts[i] = d; else drafts.push(d);
        return d.id;
    }

    function flash(id) {
        var card = id ? container.querySelector('.quiz-card[data-id="' + id + '"]') : container.querySelector('.quiz-card:last-child');
        if (!card) return;
        card.classList.add('is-new');
        card.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        setTimeout(function () { card.classList.remove('is-new'); }, 1600);
    }

    editor.addEventListener('submit', function (e) {
        e.preventDefault();
        var error = validate();
        if (error) return showError(error);
        hideError();
        var editingId = idEl.value;
        var data = new FormData(editor);
        data.delete('__RequestVerificationToken'); data.delete('LessonId');
        submitBtn.disabled = true;
        run('SaveQuestion', data, function () { saveDraftFromEditor(editingId); }).then(function () {
            resetEditor();
            flash(editingId);
            textEl.focus();
        }).catch(function (err) { showError(err.message); })
          .finally(function () { submitBtn.disabled = false; });
    });

    editor.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) { e.preventDefault(); editor.requestSubmit(); }
    });

    // ---------- Thao tác trên danh sách ----------
    function currentIds() {
        return Array.prototype.map.call(container.querySelectorAll('.quiz-card'), function (c) { return c.dataset.id; });
    }

    function saveOrder(ids, focusId) {
        var data = new FormData();
        ids.forEach(function (id) { data.append('ids', id); });
        return run('ReorderQuestions', data, function () {
            drafts = ids.map(function (x) { return drafts[indexOfDraft(x)]; });
        }).then(function () { flash(focusId); }).catch(function (err) { alert(err.message); });
    }

    container.addEventListener('click', function (e) {
        var card = e.target.closest('.quiz-card');
        if (!card) return;
        if (window.hideTooltip) window.hideTooltip();
        var id = card.dataset.id;

        if (e.target.closest('[data-quiz-edit]')) return loadQuestion(card);

        if (e.target.closest('[data-quiz-delete]')) {
            if (!confirm('Xóa ' + card.querySelector('.quiz-no').textContent.toLowerCase() + '?')) return;
            var del = new FormData(); del.append('questionId', id);
            run('DeleteQuestion', del, function () { drafts.splice(indexOfDraft(id), 1); })
                .then(function () { if (idEl.value === id) resetEditor(); }).catch(function (err) { alert(err.message); });
            return;
        }

        if (e.target.closest('[data-quiz-duplicate]')) {
            var dup = new FormData(); dup.append('questionId', id);
            run('DuplicateQuestion', dup, function () {
                var i = indexOfDraft(id), src = drafts[i];
                drafts.splice(i + 1, 0, makeDraft(null, src.type, src.text, src.options, src.correct));
            }).catch(function (err) { alert(err.message); });
            return;
        }

        var move = e.target.closest('[data-quiz-move]');
        if (move) {
            var ids = currentIds(), i = ids.indexOf(id), j = i + parseInt(move.dataset.quizMove, 10);
            if (j < 0 || j >= ids.length) return;
            ids.splice(j, 0, ids.splice(i, 1)[0]);
            saveOrder(ids, id);
        }
    });

    // Kéo thả để sắp xếp
    var dragged = null, before = '';
    container.addEventListener('dragstart', function (e) {
        dragged = e.target.closest('.quiz-card');
        before = currentIds().join(',');
        if (dragged) { dragged.classList.add('is-dragging'); e.dataTransfer.effectAllowed = 'move'; }
    });
    container.addEventListener('dragover', function (e) {
        var target = e.target.closest('.quiz-card');
        if (!dragged || !target || target === dragged) return;
        e.preventDefault();
        var box = target.getBoundingClientRect();
        if (e.clientY > box.top + box.height / 2) target.after(dragged); else target.before(dragged);
    });
    container.addEventListener('dragend', function () {
        if (!dragged) return;
        dragged.classList.remove('is-dragging');
        var id = dragged.dataset.id;
        dragged = null;
        if (currentIds().join(',') !== before) saveOrder(currentIds(), id);
    });

    // ---------- Chế độ nháp: nạp lại câu hỏi (khi form bài học báo lỗi) + gửi kèm khi lưu bài học ----------
    if (isDraft) {
        try {
            JSON.parse((draftInput && draftInput.value) || '[]').forEach(function (q) {
                drafts.push(makeDraft(null, q.QuestionType, q.QuestionText || '', q.Options || [], q.CorrectIndex || 0));
            });
        } catch (err) { drafts = []; }
        renderDrafts();

        if (lessonForm) {
            lessonForm.addEventListener('submit', function (e) {
                var type = lessonForm.querySelector('input[name="LessonType"]:checked');
                if (type && type.value === 'quiz' && textEl.value.trim()) {
                    // Câu đang soạn dở trong khung: thêm luôn vào danh sách nếu hợp lệ
                    var error = validate();
                    if (error) {
                        e.preventDefault();
                        showError(error + ' (câu hỏi đang soạn chưa được thêm)');
                        editor.scrollIntoView({ behavior: 'smooth', block: 'start' });
                        return;
                    }
                    saveDraftFromEditor(idEl.value);
                    resetEditor();
                    renderDrafts();
                }
                syncDrafts();
            });
            var passInput = lessonForm.querySelector('input[name="PassScore"]');
            if (passInput) passInput.addEventListener('input', renderDrafts);
        }
    }

    resetEditor();
})();
