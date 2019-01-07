$(document).ready(function () {
    $(".modal .profile-edit-modal-edit-option input").keyup(function () {
        var modalParent = $(this).parents().eq(5);
        var modalParentId = modalParent.attr('id');
        var inputId = $(this).attr('id');
        if ($("#" + inputId).val() === "") {
            $(this).parent().parent().removeClass("regex-failure");
            var invalidClassExists = false;
            $("#" + modalParentId + " .profile-edit-modal-edit-option").each(function () {
                if ($(this).hasClass("regex-failure")) {
                    invalidClassExists = true;
                }
            });
            if (!invalidClassExists)
                $("#" + modalParentId + "SaveButton").prop('disabled', false);
        }
        else {
            var inputRegEx = new RegExp($(this).data("val-regex-pattern"));
            var regexTest = inputRegEx.test($(this).val());
            if (!regexTest) {
                $(this).parent().parent().addClass("regex-failure");
                $("#" + modalParentId + "SaveButton").prop('disabled', true);
            }
            else {
                $(this).parent().parent().removeClass("regex-failure");
                invalidClassExists = false;
                $("#" + modalParentId + " .profile-edit-modal-edit-option").each(function () {
                    if ($(this).hasClass("regex-failure")) {
                        invalidClassExists = true;
                    }
                });
                if (!invalidClassExists)
                    $("#" + modalParentId + "SaveButton").prop('disabled', false);
            }
        }

    });
});