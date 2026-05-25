'use strict';

// Sync hidden InputMode field when Bootstrap tab changes
document.addEventListener('DOMContentLoaded', function () {
    var tabs = document.querySelectorAll('#inputTabs .nav-link');
    tabs.forEach(function (tab) {
        tab.addEventListener('shown.bs.tab', function () {
            var modeInput = document.getElementById('InputMode');
            if (modeInput) modeInput.value = this.dataset.tab;
        });
    });
});

// Loading state on article form submit
document.addEventListener('DOMContentLoaded', function () {
    var form = document.getElementById('articleForm');
    if (!form) return;

    form.addEventListener('submit', function () {
        var btn = document.getElementById('submitBtn');
        if (!btn) return;
        btn.disabled = true;
        btn.querySelector('.btn-text').classList.add('d-none');
        btn.querySelector('.spinner-content').classList.remove('d-none');
    });
});
