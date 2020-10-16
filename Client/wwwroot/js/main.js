var notyf;

$(document).ready(function () {
    $(document).click(function (e) {
        $(".input-wrapper--text").removeClass("focused");
        var target = $(e.target);
        if (target.parents(".input-wrapper--text").length > 0) {
            target.parents(".input-wrapper--text").eq(0).addClass("focused");
        }
    });

    if ($.validator != null) {

        $.validator.addMethod("mustbetrue",
            function (value, element, parameters) {
                return element.checked;
            });

        $.validator.unobtrusive.adapters.add("mustbetrue", [], function (options) {
            options.rules.mustbetrue = {};
            options.messages["mustbetrue"] = options.message;
        });
    }
});

function ToggleSubmitLoader(button) {
    button.toggleClass("on");
    button.toggleClass("disabled");

    if (button.prop("disabled")) {
        $("input:submit, button:submit").prop("disabled", false);
    }
    else {
        $("input:submit, button:submit").prop("disabled", true);
    }
}

function ClearForm(form) {
    form.trigger('reset');
}

function ShowNotification(message, type, duration, position, dismissible) {
    if (notyf == null)
        notyf = new Notyf({ types: [{ type: "warning", background: 'darkorange' }] });

    let ErrorMessage = "";
    if (Array.isArray(message)) {
        message.forEach(function (val, index, array) {
            ErrorMessage += `- ${val} <br />`;
        });
    }
    else
        ErrorMessage = message;

    switch (type) {
        case 1:
            notyf.success({
                duration: duration,
                position: position,
                dismissible: dismissible,
                message: ErrorMessage,
                className: 'toast-custom-notyf',
            });
            break;
        case 0:
            notyf.error({
                duration: duration,
                position: position,
                dismissible: dismissible,
                message: ErrorMessage,
                className: 'toast-custom-notyf',
            });
            break;
        case 2:
            notyf.open({
                type: "warning",
                duration: duration,
                position: position,
                dismissible: dismissible,
                message: ErrorMessage,
                className: 'toast-custom-notyf',
            });
            break;
        default:
            break;
    }
}

function FormBeforeSubmit() {
    let button = $(this).find("button:submit");
    ToggleSubmitLoader(button);
}

function FormAfterSubmit(response) {
    let data = response.status == 500 || response.status == 400 ? response.responseJSON : response;
    let form = $(this);
    let SubmitButton = form.find('button:submit');

    ClearForm(form);
    ToggleSubmitLoader(SubmitButton);

    if (data != null) {
        ShowNotification(data.message, data.messageStatus, 7000, { x: 'center', y: 'top' }, true);

        if (data.link != null) {
            location.href = link;
        }
    }
    else {
        ShowNotification("Sorry, an error happened on our end. Try again later.",
            0,
            7000,
            { x: 'center', y: 'top' },
            true)
    }
}