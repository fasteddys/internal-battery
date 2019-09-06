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
                $.ajax({
                    url: '/Traitify/complete',
                    type: 'GET',
                    dataType: 'json',
                    data: {
                        assessmentId: assessmentId
                    },
                    error: function () {
                        ToastService.error('Oops! Something unexpected happened, and we are looking into it.')
                    },
                    success: function (results) {
                        $('.traitify-modal').modal();
                    }
                });
            });
            assessment.assessmentID(assessmentId);
            assessment.allowFullscreen();
            assessment.target("#traitify");
            assessment.render("SlideDeck");
        }

        setUrl(assessmentId) {
            var newurl = window.location.protocol + "//" + window.location.host + "/traitify/" + assessmentId;
            window.history.pushState({ path: newurl }, '', newurl);
        }
    }
    