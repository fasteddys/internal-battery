class TraitifyCC {
        constructor(model, traitify) {
            this.model = model;
            this.traitify = traitify;
            this.initialize();
        }
    
        initialize() {
            this.setUrl(this.model.assessmentId);
            var assessmentId = this.model.assessmentId;
            this.traitify.setHost(this.model.host);
            this.traitify.setPublicKey(this.model.publicKey);
            var assessment = this.traitify.ui.component();
            assessment.on("SlideDeck.Finished", function () {
                var url = '/traitify/complete/' + assessmentId;
                $("#traitifyInstructions").hide();
                  $.ajax({
                    url: url,
                    type: 'POST',
                    error: function () {
                        ToastService.error('Oops! Something unexpected happened, and we are looking into it.')
                    },
                    success: function (results) {
                        $("#traitify-form-container").show();
                       assessment.target("#traitify");
                       assessment.render("PersonalityTypes");
                    }
                });
            });
            assessment.assessmentID(assessmentId);
            assessment.allowFullscreen();
            assessment.target("#traitify");
            assessment.render("SlideDeck");
            $("#SignUpComponent form").submit(function (e) {
                 var agreedTos = $('#SignUpComponent #termsAndConditionsCheck').is(':checked');
                $('#SignUpComponent #termsAndConditionsCheck').toggleClass('invalid', !agreedTos);
        
                if (agreedTos)
                {
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
                }
                else
                {
                        ToastService.error("Please enter information for all sign-up fields and try again.");
                }
            });
        }

        setUrl(assessmentId) {
            var newurl = window.location.protocol + "//" + window.location.host + "/traitify/" + assessmentId;
            window.history.pushState({ path: newurl }, '', newurl);
        }
    }
    