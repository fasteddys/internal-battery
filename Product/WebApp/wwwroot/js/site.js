
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



    $("#TermsOfServiceCheckbox").change(function () {
        if (this.checked) {
            $('#EnrollmentSubmitButton').prop('disabled', false);
        }
        else {
            $('#EnrollmentSubmitButton').prop('disabled', true);
        }
    });


    $('.edit-profile-info-button').on('click', function () {
        $('.personal-info-display').slideToggle();
        $('#PersonalInfoFormContainer').slideToggle();
        $('.edit-profile-info-button').toggleClass('expanded');

        $('.edit-profile-info-button').each(function () {
            if ($(this).hasClass("expanded") && !$(this).hasClass('profile-mobile-view')) {
                $(this).html("Cancel");
            } else {
                if (!$(this).hasClass('profile-mobile-cancel')) {

                    $(this).html("Edit");
                }
            }
        });

    });

    $('#PersonalInfoForm').submit(function (e) {
        $.ajax({
            type: "POST",
            url: '/home/UpdateProfileInformation',
            data: $(this).serialize(),
            success: function (html) {
                location.reload();
            }
        });
        e.preventDefault();

    });
    $('.play-button').on('click', function () {
        $(this).hide();
        $('.enrollment-success-video-thumbnail').hide();
        $('#EnrollmentSuccessVideo').prop('controls', true);
        document.getElementById("EnrollmentSuccessVideo").play();
    });

    $('.courses-in-progress-list-desktop li').each(function () {
        $(this).hover(
            function () {
                $(this).find(".course-listing").animate({ width: '0' });
                $(this).find(".progress").animate({ width: '100%' });
            },
            function () {
                $(this).find(".course-listing").animate({ width: '98%' });
                $(this).find(".progress").animate({ width: '0' });
            }
        );
    });

    $('#PromoCodeApplyButton').on('click', function () {
        var _promoCode = $('#PromoCodeInput').val();
        var _courseGuid = $('#CourseGuid').val();
        var _subscriberGuid = $('#SubscriberGuid').val();

        if (_promoCode !== undefined && $.trim(_promoCode) !== '') {
            var form = $('#CourseCheckoutForm');
            var token = $('input[name="__RequestVerificationToken"]', form).val();
            var postUrl = "/Course/PromoCodeValidation/" + _promoCode + "/" + _courseGuid + "/" + _subscriberGuid;

            $.ajax({
                url: postUrl,
                type: 'POST',
                contentType: 'application/x-www-form-urlencoded',
                headers: { 'RequestVerificationToken': token },
                success: function (result) {
                    if (result.isValid) {
                        $('#ValidationMessageSuccess span').html(result.validationMessage);
                        $('#ValidationMessageSuccess').show();
                        $('#ValidationMessageError').hide();
                        $('#PromoCodeTotal').html("-$" + result.discount);
                        $('#CourseTotal').html(result.finalCost);
                        $('#PromoCodeRedemptionGuid').val(result.promoCodeRedemptionGuid);
                        $('#PromoCodeApplyButton').prop('disabled', true);
                        $('#PromoCodeApplyButton').css('color', 'white');
                    } else {
                        $('#ValidationMessageError span').html(result.validationMessage);
                        $('#ValidationMessageSuccess').hide();
                        $('#ValidationMessageError').show();
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $('#ValidationMessageError span').html('Woops! Something went wrong - please try another promo code.');
                    $('#ValidationMessageSuccess').hide();
                    $('#ValidationMessageError').show();
                }
            });
        }
        else {
            $('#ValidationMessageError span').html('No promotional code was supplied; please enter a value and try again.');
            $('#ValidationMessageSuccess').hide();
            $('#ValidationMessageError').show();
        }
    });

    $('.selection-radios-container input').change(function () {
        if ($('#InstructorLedRadio').is(':checked')) {
            $('#InstructorLedInputField').prop('disabled', false);;
        }
        else {
            $('#InstructorLedInputField').prop('disabled', true);;
        }
    });

    $('#UpdatedCountry').change(function () {
        var country = $(this).val();
        var states = locationsList[country];
        $("#UpdatedStateInput").html("");
        for (i = 0; i < states.length; i++) {
            $('<option>').val(states[i].id).text(states[i].name).appendTo('#UpdatedStateInput');
        }
    });

    $('#UpdatedStateInput').change(function () {
        $('#UpdatedState').val($(this).val());
    });

});


