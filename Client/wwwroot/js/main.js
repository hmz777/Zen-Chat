var notyf, sideNav, roomLink, roomGo, sideNavToggled = false;

$(document).ready(function () {

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

    sideNav = $(".sidenav").eq(0);

    $(".nav-toggler").click(function () {
        sideNav.toggleClass("show");
        sideNavToggled = !sideNavToggled;
    });

    $("[data-modal]").click(function () {
        let modalId = $(this).attr("data-modal");
        $("#" + modalId).addClass("show");
    });

    $("[data-modal-close]").click(function () {
        $(this).parents(".modal-cont").removeClass("show");
    });

    roomLink = $("#RoomLink").eq(0);
    roomGo = $("#RoomGo").eq(0);

    $("#RoomName").keyup(function () {
        let url = Domain + $(this).val();
        roomLink.text(url);
        roomGo.attr("href", url);
        $(this).parents("form").attr("action", url);
    });

    $(".modal-cont").click(function (e) {
        if ($(e.target).hasClass("modal-cont")) {
            $(".modal-cont").removeClass("show");
        }
    });

    $(document).click(function (e) {
        $(".input-wrapper--text").removeClass("focused");
        var target = $(e.target);
        if (target.parents(".input-wrapper--text").length > 0) {
            target.parents(".input-wrapper--text").eq(0).addClass("focused");
        }
        else if ($(".sidenav").hasClass("show") && !target.hasClass("nav-toggler") && target.parents(".nav-toggler").length == 0) {
            $(".sidenav").removeClass("show");
        }
    });
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
            location.href = data.link;
        }
    }
    else {
        ShowNotification("Sorry, an error happened on our end. Try again later.",
            0,
            7000,
            { x: 'center', y: 'top' },
            true);
    }
}