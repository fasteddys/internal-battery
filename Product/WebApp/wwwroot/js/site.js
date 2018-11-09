
$(document).ready(function () {


    $('#SelectedCountry').change(function () {
        var selectedCountry = $("#SelectedCountry").val();
        var stateSelect = $('#SelectedState');
        stateSelect.empty();
        if (selectedCountry != null && selectedCountry != '') {
            $.getJSON(url, { countryGuid: selectedCountry }, function (states) {
                if (states != null && !jQuery.isEmptyObject(states)) {
                    stateSelect.append($('<option/>', {
                        value: null,
                        text: "---"
                    }));
                    $.each(states.value, function (index, state) {
                        stateSelect.append($('<option/>', {
                            value: state.code,
                            text: state.name
                        }));
                    });
                };
            });
        }
    });

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

    // Uncheck checkbox on page load.
    $('#TermsOfServiceCheckbox').prop('checked', false);
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
                if (html.statusCode === "200") {
                    location.reload();
                }
                else {
                    $('#ValidationSummary ul').html('');
                    var errorMessages = html.description.split(",");
                    for (i = 0; i < errorMessages.length; i++) {
                        if (i != errorMessages.length -1) {
                            var li = document.createElement("li");
                            li.innerHTML = errorMessages[i];
                            $('#ValidationSummary ul').append(li);
                        }
                        
                    }
                }
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

    //$('.courses-in-progress-list-desktop li').find(".course-listing").animate({ width: '0' });

    $('.courses-in-progress-list-desktop li').each(function () {
        var progressBar = $(this).find(".progress-bar");
        var newWidth = $(progressBar).attr('aria-valuenow') + "%";
        $(progressBar).animate({ width: newWidth });
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
                        $('#PromoCodeTotal').html("-$" + result.discount.toFixed(2));
                        $('#CourseTotal').html("$" + result.finalCost.toFixed(2));
                        $('#PromoCodeRedemptionGuid').val(result.promoCodeRedemptionGuid);
                        $('#PromoCodeApplyButton').prop('disabled', true);
                        $('#PromoCodeApplyButton').css('color', 'white');
                        if (result.finalCost === 0)
                            $('#BraintreePaymentContainer').hide();
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
});


