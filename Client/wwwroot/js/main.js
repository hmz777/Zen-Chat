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

    switch (type) {
        case "Success":
            notyf.success({
                duration: duration,
                position: position,
                dismissible: dismissible,
                message: message
            });
            break;
        case "Failure":
            notyf.error({
                duration: duration,
                position: position,
                dismissible: dismissible,
                message: message
            });
            break;
        case "warning":
            notyf.open({
                type: "warning",
                duration: duration,
                position: position,
                dismissible: dismissible,
                message: message
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
    let form = $(this);
    let SubmitButton = form.find('button:submit');

    ClearForm(form);
    ToggleSubmitLoader(SubmitButton);
    ShowNotification(response.message, response.status, 7000, { x: 'center', y: 'top' }, true);

    if (response.link != null) {
        location.href = link;
    }
}