window.EduFlowLearningChanged = function () {
    try { localStorage.setItem('eduflow:learning-updated', String(Date.now())); } catch (_) { }
};
window.EduFlowLearningChanged();
window.addEventListener('pageshow', function (event) { if (event.persisted) location.reload(); });
