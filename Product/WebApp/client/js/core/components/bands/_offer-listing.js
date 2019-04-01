$(".claim-offer-button").on("click", function (e) {

    CareerCircleAPI.claimOffer($(this).data("offer"))
        .then(function (payload) {
            $(".career-circle-code").html(payload.data.code);
            $('#OfferListingModal').modal();
        })
        .catch(function (err) {
            ToastService.error('Something unexpected happened, and we are looking into it.');
        })
        .finally(function () {
            $('.resume-control button').prop('disabled', false);
            $('.resume-control button').removeClass('disabled');
        });
});