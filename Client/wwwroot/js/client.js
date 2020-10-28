var notyf, overlayScrollbarsUserInstance, overlayScrollbarsChatInstance, emojiArea, chatSectionReference,emojione = window.emojione;

$(document).ready(function () {
    $(document).click(function (e) {
        $(".input-wrapper--text").removeClass("focused");
        var target = $(e.target);
        if (target.parents(".input-wrapper--text").length > 0) {
            target.parents(".input-wrapper--text").eq(0).addClass("focused");
        }
    });

});

function ShowNotification(message, type, duration = 5000, position = { x: 'center', y: 'top' }, dismissible = true) {
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

function FocusElement(selector) {
    document.querySelector(selector).focus();
}

function InitializeOS(id, mode = "os-theme-dark", section) {

    if (section == "user") {
        overlayScrollbarsUserInstance = OverlayScrollbars(document.getElementById(id), {
            className: mode,
            scrollbars: {
                autoHide: "leave"
            },
            overflowBehavior: {
                x: "hidden"
            }
        });
    }
    else {
        overlayScrollbarsChatInstance = OverlayScrollbars(document.getElementById(id), {
            className: mode,
            scrollbars: {
                autoHide: "leave"
            },
            overflowBehavior: {
                x: "hidden"
            }
        });
    }
}

function DestroyOS(section) {
    if (section == "user") {
        if (overlayScrollbarsUserInstance != null)
            overlayScrollbarsUserInstance.destroy();
    } else {
        if (overlayScrollbarsChatInstance != null)
            overlayScrollbarsChatInstance.destroy();
    }
}

function AddMessage(message) {
    $("#ChatContent .os-content").append(message);
}

function AddUser(User) {
    $("#UserList").append(User);
}

function InitializeEmojis(id) {

    emojiArea = $(`#${id}`).emojioneArea({
        standalone: true,
        search: false,
        autocomplete: false,
        attributes: {
            spellcheck: false,
            autocomplete: "off",
            shortnames: false,
            saveEmojisAs: "unicode"
        },
        events: {
            emojibtn_click: function (button, event) {
                let tArea = $("#MessageTextArea");
                tArea.val(tArea.val() + emojione.shortnameToUnicode(button.data("name")));
                // tArea.trigger('change');
            }
        }
    });
}