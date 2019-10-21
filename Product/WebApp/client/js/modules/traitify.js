class TraitifyCC {
    constructor(model, traitify, isAuthenticated) {
        this.model = model;
        this.traitify = traitify;
        this.initialize();
        this.isAuthenticated = isAuthenticated;
    }

    initialize() {
        this.setUrl(this.model.assessmentId);
        var assessmentId = this.model.assessmentId;
        this.traitify.setHost(this.model.host);
        this.traitify.setPublicKey(this.model.publicKey);
        var assessment = this.traitify.ui.component();
        assessment.on("SlideDeck.Finished", function () {
            var url = '/traitify/complete/' + assessmentId;
            $("#traitifyInstructions").hide();
            $.ajax({
                url: url,
                type: 'POST',
                error: function () {
                    ToastService.error('Oops! Something unexpected happened, and we are looking into it.')
                },
                success: function (results) {
                    if (results.isAuthenticated) {
                        assessment.render("Results");
                    } else {
                        $("#Email").val(results.email);
                        $("#traitify-hidden-signup-container").show()
                        assessment.render("PersonalityTraits");
                    }
                    assessment.target("#traitify");
                }
            });
        });
        assessment.target("#traitify");
        if(this.model.isComplete)
        {
            assessment.assessmentID(this.model.assessmentId);
            $("#traitifyInstructions").hide();
            if(this.model.isAuthenticated)
            {
                assessment.render("Results");
            }
            else
            {
                $("#Email").val(this.model.email);
                $("#traitify-hidden-signup-container").show()
                assessment.render("PersonalityTraits");
            }
        }
        else
        {
            assessment.assessmentID(assessmentId);
            assessment.allowFullscreen();
            assessment.render("SlideDeck");
        }

        $("#SignUpComponent").submit(function (e) {
            e.preventDefault();

            var agreedTos = $('#SignUpComponent #termsAndConditionsCheck').is(':checked');
            $('#SignUpComponent #termsAndConditionsCheck').toggleClass('invalid', !agreedTos);
            if ($("#SignUpComponent #Email").val() && $("#SignUpComponent #Password").val() && $("#SignUpComponent #ReenterPassword").val() && agreedTos) {
                $.ajax({
                    type: "POST",
                    url: "/traitify/createaccount",
                    data: $(this).serialize()
                }).done(res => {
                    alert("It worked!");
                }).fail(res => {
                    var errorText = "Unfortunately, there was an error with your submission. Please try again later.";
                    if (res.responseJSON.description != null)
                        errorText = res.responseJSON.description;
                    ToastService.error(errorText, 'Whoops...');
                });
            } else {
                ToastService.error("Please enter information for all sign-up fields and try again.");
            }
        });
    }

    setUrl(assessmentId) {
        var newurl = window.location.protocol + "//" + window.location.host + "/traitify/" + assessmentId;
        window.history.pushState({
            path: newurl
        }, '', newurl);
    }
}