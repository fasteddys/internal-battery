$("#SignUpForm").submit(function (e) {
    e.preventDefault();

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

    var agreedTos = $('#SignUpComponent #termsAndConditionsCheck').is(':checked');
    $('#SignUpComponent #termsAndConditionsCheck').toggleClass('invalid', !agreedTos);

    // Ensure user only submits to backend if form has values for all fields.
    if ($("#SignUpComponent #Email").val() && $("#SignUpComponent #Password").val() && $("#SignUpComponent #ReenterPassword").val() && agreedTos) {

        var failedRegexTest = false;

        $("#SignUpComponent input").each(function () {
            var regex = new RegExp($(this).data("val-regex-pattern"));
            var regexTest = regex.test($(this).val());
            if (!regexTest) {
                $("#SignUpOverlay").remove();
                $(this).addClass("invalid");
                failedRegexTest = true;
                ToastService.error($(this).data("val-regex"));
            }
            else {
                $(this).removeClass("invalid");
            }
        });

        if (failedRegexTest) {
            return;
        }

        if (!(new RegExp($("#SignUpEmail input").data("val-regex-pattern")).test($("#SignUpEmail input").val()))) {
            $("#SignUpEmail input").addClass("invalid");
            $("#SignUpOverlay").remove();
            ToastService.error("Invalid email address. Please update your supplied email and try again.");
            return;
        }
        else {
            $("#SignUpEmail input").removeClass("invalid");
        }
        // If passwords don't match, tell user and prevent form submission.
        if ($("#SignUpComponent #Password").val() !== $("#SignUpComponent #ReenterPassword").val()) {
            $("#SignUpOverlay").remove();
            ToastService.error("The passwords you have entered do not match.", 'Whoops...');
        }
        else {
            $.ajax({
                type: "POST",
                url: '/Home/CampaignSignUp',
                data: $(this).serialize()
            }).done(res => {
                window.location.href = res.description;
            }).fail(res => {
                var errorText = "Unfortunately, there was an error with your submission. Please try again later.";
                if (res.responseJSON.description != null)
                    errorText = res.responseJSON.description;
                ToastService.error(errorText, 'Whoops...');
            }).always(() => {
                $("#SignUpOverlay").remove();
            });
        }
    }
    else {
        $("#SignUpOverlay").remove();
        ToastService.error("Please enter information for all sign-up fields and try again.");
    }
    
    
    

}); 

/**
 * THIS IS THROWAWAY CODE FOR THE EXPRESS SIGN UP PAGE
 * */

$("#ExpressSignUpForm").submit(function (e) {
    e.preventDefault();

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

        var failedRegexTest = false;

        $("#SignUpComponent input").each(function () {
            var regex = new RegExp($(this).data("val-regex-pattern"));
            var regexTest = regex.test($(this).val());
            if (!regexTest) {
                $("#SignUpOverlay").remove();
                $(this).addClass("invalid");
                failedRegexTest = true;
                ToastService.error($(this).data("val-regex"));
            }
            else {
                $(this).removeClass("invalid");
            }
        });

        if (failedRegexTest) {
            return;
        }

        if (!(new RegExp($("#SignUpEmail input").data("val-regex-pattern")).test($("#SignUpEmail input").val()))) {
            $("#SignUpEmail input").addClass("invalid");
            $("#SignUpOverlay").remove();
            ToastService.error("Invalid email address. Please update your supplied email and try again.");
            return;
        }
        else {
            $("#SignUpEmail input").removeClass("invalid");
        }
        // If passwords don't match, tell user and prevent form submission.
        if ($("#SignUpComponent #Password").val() !== $("#SignUpComponent #ReenterPassword").val()) {
            $("#SignUpOverlay").remove();
            ToastService.error("The passwords you have entered do not match.", 'Whoops...');
        }
        else {
            $.ajax({
                type: "POST",
                url: '/Home/ExpressSignUp',
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
                        ToastService.error(html.description, 'Whoops...');
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $("#SignUpOverlay").remove();
                    ToastService.error("Unfortunately, there was an error with your submission. Please try again later.");
                }
            });
        }
    }
    else {
        $("#SignUpOverlay").remove();
        ToastService.error("Please enter information for all sign-up fields and try again.");
    }




}); 