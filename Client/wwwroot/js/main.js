$(document).ready(function () {
    $(document).click(function (e) {
        $(".input-wrapper--text").removeClass("focused");
        var target = $(e.target);
        if (target.parents(".input-wrapper--text").length > 0) {
            target.parents(".input-wrapper--text").eq(0).addClass("focused");
        }
    });

    $.validator.addMethod("mustbetrue",
        function (value, element, parameters) {
            return element.checked;
        });

    $.validator.unobtrusive.adapters.add("mustbetrue", [], function (options) {
        options.rules.mustbetrue = {};
        options.messages["mustbetrue"] = options.message;
    });
});