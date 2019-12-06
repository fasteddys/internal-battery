$("#ResetPasswordForm").submit(function (e) {
    e.preventDefault();

    $("body").prepend("<div class=\"overlay\" id=\"ResetPasswordOverlay\"><div id=\"loading-img\" ></div></div>");
    $("#ResetPasswordOverlay").show();

    // Ensure user only submits to backend if form has values for all fields.
    if ($("#ResetPasswordForm #EmailAddress").val()) {
        $.ajax({
            type: "POST",
            url: $(this).attr("action"),
            data: $(this).serialize()
        }).done(res => {
            $('.resetpassword-modal').modal();
        }).fail(res => {
            var errorText = "Unfortunately, there was an error with your submission. Please try again later.";
            if (res.responseJSON.description != null)
                errorText = res.responseJSON.description;
            ToastService.error(errorText, 'Whoops...');
        }).always(() => {
            $("#ResetPasswordOverlay").remove();
        });
    }
    else {
        $("#ResetPasswordOverlay").remove();
        ToastService.error("Please enter your email address to continue.");
    }
});

$(document).ready(function () {

    $("#ResetPasswordForm #EmailAddress").val("");

    var error = $("#ResetPasswordValidationSummary ul li").text();
    if (error.length != 0) {
        ToastService.error(error, 'Oops!');
    }
}); 