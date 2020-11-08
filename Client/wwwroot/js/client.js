var notyf,
    overlayScrollbarsUserInstance,
    overlayScrollbarsChatInstance,
    emojiArea,
    chatSectionReference,
    emojione = window.emojione,
    newMessageCount = 0,
    canSendNotifications = false,
    newMessageNotificationCount = 1;

$.extend($.easing,
    {
        easeInCubic: function (x, t, b, c, d) {
            return c * (t /= d) * t * t + b;
        },
        easeOutCubic: function (x, t, b, c, d) {
            return c * ((t = t / d - 1) * t * t + 1) + b;
        },
        easeInOutCubic: function (x, t, b, c, d) {
            if ((t /= d / 2) < 1) return c / 2 * t * t * t + b;
            return c / 2 * ((t -= 2) * t * t + 2) + b;
        }
    });

$(document).ready(function () {
    $(document).click(function (e) {
        $(".input-wrapper--text").removeClass("focused");
        var target = $(e.target);
        if (target.parents(".input-wrapper--text").length > 0) {
            target.parents(".input-wrapper--text").eq(0).addClass("focused");
        }
    });

    $(document).on("keyup", "#MessageTextArea", function () {
        if ($(this).val().length == 0) {
            $(this).css("height", "0px");
        }

        if ($(this).height() >= 150)
            return;

        $(this).css("height", "0px");
        $(this).css("height", `${$(this)[0].scrollHeight}px`);
    });

    $(document).on("focus", "#MessageTextArea", function () {
        SetTitle("Z-Chat");
    });

});

function ShowNotification(message, type, duration = 5000, position = { x: 'center', y: 'top' }, dismissible = true) {
    if (notyf == null)
        notyf = new Notyf({ types: [{ type: "warning", background: 'darkorange' }, { type: "information", background: 'dodgerblue' }] });

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
        case 3:
            notyf.open({
                type: "information",
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

function AddUser(user) {
    $("#UserList").append(user);
}

function AddUserList(list) {
    $("#UserList").append(list);
}

function RemoveUser(username) {
    $("#UserList").children(`[data-name='${username}']`).eq(0).remove();
}

function InitializeEmojis(id, chatRef) {

    if (chatSectionReference == null)
        chatSectionReference = chatRef;

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
                let emoji = emojione.shortnameToUnicode(button.data("name"));
                tArea.val(tArea.val() + emoji);
                chatSectionReference.invokeMethodAsync("AddEmoji", emoji)
            }
        }
    });
}

function ScrollChatSec(id) {
    overlayScrollbarsChatInstance.scroll({ y: "100%" }, 300, { y: "easeOutCubic" })
}

function SetTitle(title) {
    ++newMessageCount;
    if (title == null) {
        if (!document.hasFocus()) {
            document.title = `(${newMessageCount}) New message`;
            return false;
        }
    }
    else {
        document.title = title;
    }
    newMessageCount = 0;
}

function SubscribeToNotifications() {
    if (!('Notification' in window)) {
        alert("This browser does not support notifications.");
    } else {
        if (checkNotificationPromise()) {
            Notification.requestPermission()
                .then((permission) => {
                    handlePermission(permission);
                })
        } else {
            Notification.requestPermission(function (permission) {
                handlePermission(permission);
            });
        }

        return canSendNotifications;
    }

    function checkNotificationPromise() {
        try {
            Notification.requestPermission().then();
        } catch (e) {
            return false;
        }

        return true;
    }
}

    function handlePermission(permission) {
        if (!('permission' in Notification)) {
            Notification.permission = permission;
        }

        if (Notification.permission === 'denied' || Notification.permission === 'default') {
            canSendNotifications = false;
        } else {
            canSendNotifications = true;
        }
    }

function CheckNotificationPermission(){
canSendNotifications = Notification.permission == "granted" ? true : false;
return canSendNotifications;
}

function SendNotification(message) {
    if (canSendNotifications && !document.hasFocus()) {
        let notification = new Notification("New Message", { body: message, tag: `newMessage${newMessageNotificationCount}` });
        ++newMessageNotificationCount;

        if (newMessageNotificationCount > 3)
            newMessageNotificationCount = 1;
    }
}