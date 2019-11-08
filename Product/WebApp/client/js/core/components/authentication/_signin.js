$("#SignInForm").submit(function (e) {
    e.preventDefault();

    $("body").prepend("<div class=\"overlay\" id=\"SignInOverlay\"><div id=\"loading-img\" ></div></div>");
    $("#SignInOverlay").show();
    
    // Ensure user only submits to backend if form has values for all fields.
    if ($("#SignInForm #EmailAddress").val() && $("#SignInForm #Password").val()) {
        e.currentTarget.submit();
    }
    else {
        $("#SignInOverlay").remove();
        ToastService.error("Please enter information for all sign-in fields and try again.");
    }
}); 

$(document).ready(function () {
    $("#SignInForm #Password").val("");

    var error = $("#SignInValidationSummary ul li").text();
    if (error.length != 0) {
        ToastService.error(error, 'Oops!');
    }

    var urlParams = new URLSearchParams(window.location.search);
    var message = urlParams.get('message');
    var success = urlParams.get('success');
    if (success === 'true') {
        ToastService.success(message, 'Success');
    } else if (success === 'false') {
        ToastService.error(message, 'Failure');
    }
}); 