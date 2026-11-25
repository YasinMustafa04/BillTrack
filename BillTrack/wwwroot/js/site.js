// Защита от двойно изпращане: след първото валидно изпращане бутоните се заключват
document.addEventListener('submit', function (e) {
    const form = e.target;

    // Ако клиентската валидация е спряла формата, нищо не заключваме
    if (e.defaultPrevented || !(form instanceof HTMLFormElement)) return;

    if (form.dataset.submitted === 'true') {
        e.preventDefault();
        return;
    }

    form.dataset.submitted = 'true';
    form.querySelectorAll('button[type="submit"]').forEach(function (button) {
        button.disabled = true;
    });
});

// При връщане с бутона "Back" браузърът може да върне страницата със заключени бутони
window.addEventListener('pageshow', function (e) {
    if (!e.persisted) return;

    document.querySelectorAll('form').forEach(function (form) {
        delete form.dataset.submitted;
        form.querySelectorAll('button[type="submit"]').forEach(function (button) {
            button.disabled = false;
        });
    });
});
