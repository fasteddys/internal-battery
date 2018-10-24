// Write your Javascript code.
$(document).ready(function () {
    $("#SameAsAboveCheckbox").change(function () {
        if (this.checked) {
            $('.billing-info-container').hide();
            $('.billing-info-container input, .billing-info-container select').removeAttr('required');
        }
        else {
            $('.billing-info-container').show();
            $('.billing-info-container input, .billing-info-container select').prop('required', true);
        }
    });
    /*
    $("#TermsOfServiceCheckbox").change(function () {
        if (this.checked) {
            $('#EnrollmentSubmitButton').prop('disabled', false);
        }
        else {
            $('#EnrollmentSubmitButton').prop('disabled', true);
        }
    });
    */

    $('.play-button').on('click', function () {
        $(this).hide();
        $('.enrollment-success-video-thumbnail').hide();
        $('#EnrollmentSuccessVideo').prop('controls', true);
        document.getElementById("EnrollmentSuccessVideo").play();
    });

    $('#PromoCodeApplyButton').on('click', function () {
        var promoCode = $('#PromoCodeInput').val();
        var courseGuid = $('#CourseGuid').val();
        var subscriberGuid = $('#SubscriberGuid').val();

        if (promoCode !== undefined && promoCode !== '') {
            var getUrl = "/Course/PromoCode/" + promoCode + "/" + courseGuid + "/" + subscriberGuid;
            $.ajax({
                url: getUrl, success: function (result) {
                    if (result.validationMessage !== null) {
                        // show/hide conditionally if property has value
                        $('.promotional-code-validation').show();
                        $('#ValidationMessage').html(result.validationMessage);
                    } else {
                        $('.promotional-code-validation').hide();
                        $('#PromoCodeTotal').html("-$" + result.discount);
                        $('#CourseTotal').html(result.FinalCost);
                        $('#PromoCodeRedemptionGuid').val(result.promoCodeRedemptionGuid);
                    }
                }
            });
        }
        else {
            $('#ValidationMessage').html('No promotional code was supplied; please enter a value and try again.');
        }
    });

    //$('#PromoCodeApplyButton').on('click', function () {
    //    var promoCode = $('#PromoCodeInput').val();
    //    var coursePrice = $('#CoursePrice').val();
    //    var courseGuid = $('#CourseGuid').val();
    //    if (promoCode !== undefined && promoCode !== '' && promoCodeIsValid(promoCode)) {
    //        var getUrl = "/Course/PromoCode/" + courseGuid + "/" + promoCode;
    //        $.ajax({
    //            url: getUrl, success: function (result) {
    //                var resultAsJson = $.parseJSON(result);
    //                $('#PromoCodeTotal').html("-$" + resultAsJson.AmountOffCourse);
    //                $('#CourseTotal').html(resultAsJson.NewCoursePrice);
    //                $('#PromoCodeForSubmission').val(promoCode);
    //            }
    //        });
    //    }
    //    else {
    //        alert("Promo code invalid; please try again.");
    //    }

    //});

    // TODO: Implement client side security form promo code.
    function promoCodeIsValid(code) {
        return true;
    }

});


