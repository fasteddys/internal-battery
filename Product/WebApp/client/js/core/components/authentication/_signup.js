﻿$("#SignUpForm").submit(function (e) {
    e.preventDefault();
    var toastrOptions = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": false,
        "progressBar": false,
        "positionClass": "toast-top-full-width",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "3000",
        "extendedTimeOut": "1000",
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

    


    // Ensure user only submits to backend if form has values for all fields.
    if ($("#SignUpComponent #Email").val() && $("#SignUpComponent #Password").val() && $("#SignUpComponent #ReenterPassword").val()) {

        // If passwords don't match, tell user and prevent form submission.
        if ($("#SignUpComponent #Password").val() !== $("#SignUpComponent #ReenterPassword").val()) {
            $("#SignUpOverlay").remove();
            toastr.error("The passwords you have entered do not match.", 'Whoops...', toastrOptions);
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
                        toastr.error(html.description, 'Whoops...', toastrOptions);
                    }
                }
            });
        }
    }
    else {
        $("#SignUpOverlay").remove();
        toastr.error("Please enter information for all sign-up fields and try again.", toastrOptions);
    }
    
    
    

}); 

// Allow Enter key to submit form
$("#SignUpComponent input").on('keypress', function (e) {
    if (e.which === 13) {
        $("#SignUpForm").submit();
    }
});

