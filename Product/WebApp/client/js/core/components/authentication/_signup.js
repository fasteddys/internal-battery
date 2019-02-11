$("#SignUpForm").submit(function (e) {
    e.preventDefault();
    var signUpToastOptions = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": false,
        "progressBar": false,
        "positionClass": "toast-top-full-width",
        "preventDuplicates": false,
        "onclick": null,
        "timeOut": "0",
        "extendedTimeOut": "0",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };

    $("body").prepend("<div class=\"overlay\" id=\"SignUpOverlay\"><div id=\"loading-img\" ></div></div>");
    $("#SignUpOverlay").show();

    // Put red border on input box if field is blank.
    $("#SignUpComponent input").each(function () {
        if (!$(this).val()) {
            $(this).addClass("invalid");
        }
        else {
            $(this).removeClass("invalid");
        }
    });

    if (!(new RegExp($("#SignUpEmail input").data("val-regex-pattern")).test($("#SignUpEmail input").val()))) {
        $("#SignUpOverlay").remove();
        toastr.error("Invalid email address. Please update your supplied email and try again.", signUpToastOptions);
        return;
    }


    // Ensure user only submits to backend if form has values for all fields.
    if ($("#SignUpComponent #Email").val() && $("#SignUpComponent #Password").val() && $("#SignUpComponent #ReenterPassword").val()) {

        // If passwords don't match, tell user and prevent form submission.
        if ($("#SignUpComponent #Password").val() !== $("#SignUpComponent #ReenterPassword").val()) {
            $("#SignUpOverlay").remove();
            toastr.error("The passwords you have entered do not match.", 'Whoops...', signUpToastOptions);
        }
        else {
            $.ajax({
                type: "POST",
                url: '/Home/CampaignSignUp',
                data: $(this).serialize(),
                success: function (html) {
                    if (html.statusCode === 200) {
                        $("#SignUpOverlay").remove();
                        // If submission is OK, redirect them to authenticated course checkout page.
                        window.location.href = html.description;
                    }
                    else {
                        $("#SignUpOverlay").remove();
                        // If there's an error on submission, display it to user in graceful toast message.
                        toastr.error(html.description, 'Whoops...', signUpToastOptions);
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $("#SignUpOverlay").remove();
                    toastr.error("Unfortunately, there was an error with your submission. Please try again later.", signUpToastOptions);
                }
            });
        }
    }
    else {
        $("#SignUpOverlay").remove();
        toastr.error("Please enter information for all sign-up fields and try again.", signUpToastOptions);
    }
    
    
    

}); 

// Allow Enter key to submit form
$("#SignUpComponent input").on('keypress', function (e) {
    if (e.which === 13) {
        $("#SignUpForm").submit();
    }
});

